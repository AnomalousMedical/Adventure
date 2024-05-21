using DiligentEngine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    public class FSRImageBlitterImpl : IRTImageBlitterImpl
    {
        const TEXTURE_FORMAT ColorBufferFormat = TEXTURE_FORMAT.TEX_FORMAT_RGBA8_UNORM;

        AutoPtr<ITexture> colorRT;
        AutoPtr<ITexture> upsamplePassRT;

        AutoPtr<IPipelineState> imageBlitPSO;
        AutoPtr<IShaderResourceBinding> imageBlitSRB;

        AutoPtr<IPipelineState> upsamplePSO;
        AutoPtr<IShaderResourceBinding> upsampleSRB;

        public void CreateBuffers(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader)
        {
            CreateUpsamplePSO(graphicsEngine, shaderLoader);
            CreateImageBlitPSO(graphicsEngine, shaderLoader);
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

            var ImmutableSamplers = new List<ImmutableSamplerDesc>
            {
                new ImmutableSamplerDesc{ ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_Texture", Desc = SamLinearClampDesc }
            };

            PSOCreateInfo.PSODesc.ResourceLayout.ImmutableSamplers = ImmutableSamplers;
            PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC;

            upsamplePSO = m_pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
            upsampleSRB = upsamplePSO.Obj.CreateShaderResourceBinding(true);
        }

        private void CreateImageBlitPSO(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader)
        {
            var m_pDevice = graphicsEngine.RenderDevice;
            var m_pSwapChain = graphicsEngine.SwapChain;

            GraphicsPipelineStateCreateInfo PSOCreateInfo = new GraphicsPipelineStateCreateInfo();

            PSOCreateInfo.PSODesc.Name = "Image blit PSO";
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
            ShaderCI.Desc.Name = "Image blit VS";
            ShaderCI.Source = shaderLoader.LoadShader("assets/FSRSharpen.vsh");
            using var pVS = m_pDevice.CreateShader(ShaderCI);
            //VERIFY_EXPR(pVS != nullptr);

            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_PIXEL;
            ShaderCI.EntryPoint = "main";
            ShaderCI.Desc.Name = "Image blit PS";
            ShaderCI.Source = shaderLoader.LoadShader("assets/FSRSharpen.psh");
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

            var ImmutableSamplers = new List<ImmutableSamplerDesc>
            {
                new ImmutableSamplerDesc{ ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_Texture", Desc = SamLinearClampDesc }
            };

            PSOCreateInfo.PSODesc.ResourceLayout.ImmutableSamplers = ImmutableSamplers;
            PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC;

            imageBlitPSO = m_pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
            //VERIFY_EXPR(m_pImageBlitPSO != nullptr);

            imageBlitSRB = imageBlitPSO.Obj.CreateShaderResourceBinding(true);

            //VERIFY_EXPR(m_pImageBlitSRB != nullptr);
        }

        public void Dispose()
        {
            colorRT?.Dispose();
            imageBlitSRB.Dispose();
            imageBlitPSO.Dispose();
            upsampleSRB.Dispose();
            upsamplePSO.Dispose();
        }

        public void Blit(GraphicsEngine graphicsEngine)
        {
            var swapChain = graphicsEngine.SwapChain;
            var immediateContext = graphicsEngine.ImmediateContext;

            {
                //Upsample
                upsampleSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_Texture").Set(colorRT.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                immediateContext.SetRenderTarget(upsamplePassRT.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_RENDER_TARGET), null, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                immediateContext.SetPipelineState(upsamplePSO.Obj);
                immediateContext.CommitShaderResources(upsampleSRB.Obj, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                var Attribs = new DrawAttribs();
                Attribs.NumVertices = 4;
                immediateContext.Draw(Attribs);
            }

            {
                //Image Blit
                imageBlitSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_Texture").Set(upsamplePassRT.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                var pRTV = swapChain.GetCurrentBackBufferRTV();
                immediateContext.SetRenderTarget(pRTV, null, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                immediateContext.SetPipelineState(imageBlitPSO.Obj);
                immediateContext.CommitShaderResources(imageBlitSRB.Obj, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                var Attribs = new DrawAttribs();
                Attribs.NumVertices = 4;
                immediateContext.Draw(Attribs);
            }
        }

        public ITexture Texture => colorRT.Obj;

        public uint Width => colorRT.Obj.GetDesc_Width;

        public uint Height => colorRT.Obj.GetDesc_Height;

        public void WindowResize(GraphicsEngine graphicsEngine, UInt32 width, UInt32 height)
        {
            UInt32 colorWidth = 1920;
            UInt32 colorHeight = 1080;

            var m_pDevice = graphicsEngine.RenderDevice;

            // Check if the image needs to be recreated.
            if (upsamplePassRT != null &&
                upsamplePassRT.Obj.GetDesc_Width == width &&
                upsamplePassRT.Obj.GetDesc_Height == height)
            {
                return;
            }

            if (colorWidth == 0 || colorHeight == 0)
            {
                return;
            }

            colorRT?.Dispose();
            colorRT = null;

            upsamplePassRT?.Dispose();
            upsamplePassRT = null;

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

            {
                // Create window-size upsample image.
                var RTDesc = new TextureDesc();
                RTDesc.Name = "Upsample buffer";
                RTDesc.Type = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D;
                RTDesc.Width = width;
                RTDesc.Height = height;
                RTDesc.BindFlags = BIND_FLAGS.BIND_RENDER_TARGET | BIND_FLAGS.BIND_SHADER_RESOURCE;
                RTDesc.Format = ColorBufferFormat;

                upsamplePassRT = m_pDevice.CreateTexture(RTDesc, null);
            }
        }
    }
}
