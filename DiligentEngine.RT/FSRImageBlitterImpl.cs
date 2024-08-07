﻿using DiligentEngine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct FSRConstants
    {
        public UInt32 inputSizeW;
        public UInt32 inputSizeH;
        public UInt32 outSizeW;
        public UInt32 outSizeH;
    };

    public class FSRImageBlitterImpl : IRTImageBlitterImpl
    {
        const TEXTURE_FORMAT ColorBufferFormat = TEXTURE_FORMAT.TEX_FORMAT_RGBA32_FLOAT;

        AutoPtr<ITexture> colorRT;

        AutoPtr<IPipelineState> upsamplePSO;
        AutoPtr<IShaderResourceBinding> upsampleSRB;

        private AutoPtr<IBuffer> m_fsrConstants;
        private FSRConstants fsrConstants;

        private float renderPercent;

        public FSRImageBlitterImpl(DiligentEngineOptions options)
        {
            renderPercent = options.FSR1RenderPercentage;
        }

        public void CreateBuffers(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader)
        {
            unsafe
            {
                var m_pDevice = graphicsEngine.RenderDevice;

                BufferDesc CBDesc = new BufferDesc();
                CBDesc.Name = "FSR Screen Size Buffer";
                CBDesc.Size = (ulong)sizeof(FSRConstants);
                CBDesc.Usage = USAGE.USAGE_DEFAULT;
                CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                m_fsrConstants = m_pDevice.CreateBuffer(CBDesc);
            }

            CreateUpsamplePSO(graphicsEngine, shaderLoader);
        }

        private void CreateUpsamplePSO(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader)
        {
            var m_pDevice = graphicsEngine.RenderDevice;
            var m_pSwapChain = graphicsEngine.SwapChain;

            GraphicsPipelineStateCreateInfo PSOCreateInfo = new GraphicsPipelineStateCreateInfo();

            PSOCreateInfo.PSODesc.Name = "Upsample PSO";
            PSOCreateInfo.PSODesc.PipelineType = PIPELINE_TYPE.PIPELINE_TYPE_GRAPHICS;

            PSOCreateInfo.GraphicsPipeline.NumRenderTargets = 1;
            PSOCreateInfo.GraphicsPipeline.RTVFormats_0 = m_pSwapChain.GetDesc_ColorBufferFormat;
            PSOCreateInfo.GraphicsPipeline.PrimitiveTopology = PRIMITIVE_TOPOLOGY.PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
            PSOCreateInfo.GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_NONE;
            PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.DepthEnable = false;

            ShaderCreateInfo ShaderCI = new ShaderCreateInfo();
            ShaderCI.UseCombinedTextureSamplers = true;
            ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;
            ShaderCI.ShaderCompiler = SHADER_COMPILER.SHADER_COMPILER_DXC;

            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_VERTEX;
            ShaderCI.EntryPoint = "main";
            ShaderCI.Desc.Name = "Image upsample VS";
            ShaderCI.Source = shaderLoader.LoadShader("assets/FSRUpsample.vsh");
            using var pVS = m_pDevice.CreateShader(ShaderCI);
            //VERIFY_EXPR(pVS != nullptr);

            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_PIXEL;
            ShaderCI.EntryPoint = "main";
            ShaderCI.Desc.Name = "Image upsample PS";
            ShaderCI.Source = shaderLoader.LoadShader("assets/FSRUpsample.psh");
            using var pPS = m_pDevice.CreateShader(ShaderCI);
            //VERIFY_EXPR(pPS != nullptr);

            PSOCreateInfo.pVS = pVS.Obj;
            PSOCreateInfo.pPS = pPS.Obj;

            var SamLinearClampDesc = new SamplerDesc
            {
                MinFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
                MagFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
                MipFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
                AddressU = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_CLAMP,
                AddressV = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_CLAMP,
                AddressW = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_CLAMP
            };

            var Vars = new List<ShaderResourceVariableDesc>
            {
                new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL | SHADER_TYPE.SHADER_TYPE_VERTEX, Name = "FSRConstants", Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC}
            };
            PSOCreateInfo.PSODesc.ResourceLayout.Variables = Vars;

            var ImmutableSamplers = new List<ImmutableSamplerDesc>
            {
                new ImmutableSamplerDesc{ ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_Texture", Desc = SamLinearClampDesc }
            };

            PSOCreateInfo.PSODesc.ResourceLayout.ImmutableSamplers = ImmutableSamplers;
            PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC;

            upsamplePSO = m_pDevice.CreateGraphicsPipelineState(PSOCreateInfo);

            upsamplePSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "FSRConstants").Set(m_fsrConstants.Obj);
            upsamplePSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "FSRConstants").Set(m_fsrConstants.Obj);

            upsampleSRB = upsamplePSO.Obj.CreateShaderResourceBinding(true);
        }

        public void Dispose()
        {
            colorRT?.Dispose();
            upsampleSRB.Dispose();
            upsamplePSO.Dispose();
            m_fsrConstants.Dispose();
        }

        public void Blit(GraphicsEngine graphicsEngine)
        {
            var swapChain = graphicsEngine.SwapChain;
            var immediateContext = graphicsEngine.ImmediateContext;

            {
                //Upsample only
                upsampleSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_Texture").Set(colorRT.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                var pRTV = swapChain.GetCurrentBackBufferRTV();
                immediateContext.SetRenderTarget(pRTV, null, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                immediateContext.SetPipelineState(upsamplePSO.Obj);
                immediateContext.CommitShaderResources(upsampleSRB.Obj, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                var Attribs = new DrawAttribs();
                Attribs.NumVertices = 4;
                immediateContext.Draw(Attribs);
            }
        }

        public ITexture RTTexture => colorRT.Obj;

        public uint Width => colorRT.Obj.GetDesc_Width;

        public uint Height => colorRT.Obj.GetDesc_Height;

        public uint FullWidth => fsrConstants.outSizeW;

        public uint FullHeight => fsrConstants.outSizeH;

        public void WindowResize(GraphicsEngine graphicsEngine, UInt32 width, UInt32 height)
        {
            // Check if the image needs to be recreated.
            if (colorRT != null &&
                fsrConstants.outSizeW == width &&
                fsrConstants.outSizeH == height)
            {
                return;
            }


            UInt32 colorWidth = (UInt32)(width * renderPercent);
            UInt32 colorHeight = (UInt32)(height * renderPercent);

            //Match scale if res will be too small
            if(colorWidth == 0 || colorHeight == 0)
            {
                colorWidth = width;
                colorHeight = height;
            }

            var m_pDevice = graphicsEngine.RenderDevice;
            var immediateContext = graphicsEngine.ImmediateContext;

            if (colorWidth == 0 || colorHeight == 0)
            {
                return;
            }

            colorRT?.Dispose();
            colorRT = null;

            {
                // Create color image.
                var RTDesc = new TextureDesc();
                RTDesc.Name = "Color buffer";
                RTDesc.Type = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D;
                RTDesc.Width = colorWidth;
                RTDesc.Height = colorHeight;
                RTDesc.BindFlags = BIND_FLAGS.BIND_UNORDERED_ACCESS | BIND_FLAGS.BIND_SHADER_RESOURCE;
                RTDesc.ClearValue.Format = ColorBufferFormat;
                RTDesc.Format = ColorBufferFormat;

                colorRT = m_pDevice.CreateTexture(RTDesc, null);
            }

            fsrConstants.inputSizeW = colorRT.Obj.GetDesc_Width;
            fsrConstants.inputSizeH = colorRT.Obj.GetDesc_Height;
            fsrConstants.outSizeW = width;
            fsrConstants.outSizeH = height;
            unsafe
            {
                fixed (FSRConstants* constantsPtr = &fsrConstants)
                {
                    var barriers = new List<StateTransitionDesc>(1); //TODO: Persist this and don't make it every frame
                    barriers.Add(new StateTransitionDesc { pResource = m_fsrConstants.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_COPY_DEST, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    immediateContext.TransitionResourceStates(barriers);
                    immediateContext.UpdateBuffer(m_fsrConstants.Obj, 0, (uint)sizeof(FSRConstants), new IntPtr(constantsPtr), RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_VERIFY);
                }
            }
        }
    }
}
