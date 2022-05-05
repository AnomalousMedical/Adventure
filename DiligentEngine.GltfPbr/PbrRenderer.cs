﻿using DiligentEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Uint8 = System.Byte;
using Int8 = System.SByte;
using Bool = System.Boolean;
using Uint32 = System.UInt32;
using Uint64 = System.UInt64;
using Float32 = System.Single;
using Uint16 = System.UInt16;
using PVoid = System.IntPtr;
using float4 = Engine.Vector4;
using float3 = Engine.Vector3;
using float2 = Engine.Vector2;
using float4x4 = Engine.Matrix4x4;
using BOOL = System.Boolean;
using System.Collections;
using Engine;

namespace DiligentEngine.GltfPbr
{ 
    public class PbrRenderer : IDisposable
    {
        public const int DefaultNormal = 0x00FF7F7F;
        public const int DefaultPhysical = 0x0000FF00;

        static readonly SamplerDesc Sam_LinearClamp = new SamplerDesc
        {
            MinFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
            MagFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
            MipFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
            AddressU = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_CLAMP,
            AddressV = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_CLAMP,
            AddressW = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_CLAMP
        };

        const Uint32 TexDim = 8;

        const Uint32 BRDF_LUT_Dim = 512;
        const TEXTURE_FORMAT IrradianceCubeFmt = TEXTURE_FORMAT.TEX_FORMAT_RGBA32_FLOAT;
        const TEXTURE_FORMAT PrefilteredEnvMapFmt = TEXTURE_FORMAT.TEX_FORMAT_RGBA16_FLOAT;
        const Uint32 IrradianceCubeDim = 64;
        const Uint32 PrefilteredEnvMapDim = 256;

        private PbrRendererCreateInfo m_Settings;
        private readonly ShaderLoader<PbrRenderer> shaderLoader;
        private AutoPtr<ITextureView> m_pBRDF_LUT_SRV;

        private PSOCache m_PSOCache = new PSOCache();

        private AutoPtr<ITextureView> m_pWhiteTexSRV;
        private AutoPtr<ITextureView> m_pBlackTexSRV;
        private AutoPtr<ITextureView> m_pDefaultNormalMapSRV;
        private AutoPtr<ITextureView> m_pDefaultPhysDescSRV;

        private AutoPtr<ITextureView> m_pIrradianceCubeSRV;
        private AutoPtr<ITextureView> m_pPrefilteredEnvMapSRV;
        private AutoPtr<IPipelineState> m_pPrecomputeIrradianceCubePSO;
        private AutoPtr<IPipelineState> m_pPrefilterEnvMapPSO;
        private AutoPtr<IShaderResourceBinding> m_pPrecomputeIrradianceCubeSRB;
        private AutoPtr<IShaderResourceBinding> m_pPrefilterEnvMapSRB;

        private AutoPtr<IBuffer> m_TransformsCB;
        private AutoPtr<IBuffer> m_GLTFAttribsCB;
        private AutoPtr<IBuffer> m_PrecomputeEnvMapAttribsCB;
        private AutoPtr<IBuffer> m_JointsBuffer;

        public PbrRenderer(
            IRenderDevice pDevice, 
            IDeviceContext pCtx, 
            PbrRendererCreateInfo CI, 
            ShaderLoader<PbrRenderer> shaderLoader
        )
        {
            this.m_Settings = CI;
            this.shaderLoader = shaderLoader;

            if (m_Settings.UseIBL)
            {
                PrecomputeBRDF(pDevice, pCtx);

                TextureDesc TexDesc = new TextureDesc();
                TexDesc.Name = "Irradiance cube map for GLTF renderer";
                TexDesc.Type = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_CUBE;
                TexDesc.Usage = USAGE.USAGE_DEFAULT;
                TexDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE | BIND_FLAGS.BIND_RENDER_TARGET;
                TexDesc.Width = IrradianceCubeDim;
                TexDesc.Height = IrradianceCubeDim;
                TexDesc.Format = IrradianceCubeFmt;
                TexDesc.ArraySize = 6;
                TexDesc.MipLevels = 0;

                using var IrradainceCubeTex = pDevice.CreateTexture(TexDesc, null);
                m_pIrradianceCubeSRV = new AutoPtr<ITextureView>(IrradainceCubeTex.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                TexDesc.Name = "Prefiltered environment map for GLTF renderer";
                TexDesc.Width = PrefilteredEnvMapDim;
                TexDesc.Height = PrefilteredEnvMapDim;
                TexDesc.Format = PrefilteredEnvMapFmt;
                using var PrefilteredEnvMapTex = pDevice.CreateTexture(TexDesc, null);
                m_pPrefilteredEnvMapSRV = new AutoPtr<ITextureView>(PrefilteredEnvMapTex.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));
            }

            unsafe
            {
                TextureDesc TexDesc = new TextureDesc();
                TexDesc.Name = "White texture for GLTF renderer";
                TexDesc.Type = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D_ARRAY;
                TexDesc.Usage = USAGE.USAGE_IMMUTABLE;
                TexDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
                TexDesc.Width = TexDim;
                TexDesc.Height = TexDim;
                TexDesc.Format = TEXTURE_FORMAT.TEX_FORMAT_RGBA8_UNORM;
                TexDesc.MipLevels = 1;

                int dataLength = (int)(TexDim * TexDim);
                Uint32* Data = stackalloc Uint32[dataLength];
                Span<Uint32> DataSpan = new Span<Uint32>(Data, dataLength);
                DataSpan.Fill(0xFFFFFFFF);
                var Level0Data = new TextureSubResData { pData = new IntPtr(Data), Stride = TexDim * 4 };
                var InitData = new TextureData { pSubResources = new List<TextureSubResData> { Level0Data } };
                using var pWhiteTex = pDevice.CreateTexture(TexDesc, InitData);
                m_pWhiteTexSRV = new AutoPtr<ITextureView>(pWhiteTex.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                TexDesc.Name = "Black texture for GLTF renderer";
                DataSpan.Fill(0); //for (auto & c : Data) c = 0;
                using var pBlackTex = pDevice.CreateTexture(TexDesc, InitData);
                m_pBlackTexSRV = new AutoPtr<ITextureView>(pBlackTex.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                TexDesc.Name = "Default normal map for GLTF renderer";
                DataSpan.Fill(DefaultNormal); //for (auto & c : Data) c = 0x00FF7F7F;
                using var pDefaultNormalMap = pDevice.CreateTexture(TexDesc, InitData);
                m_pDefaultNormalMapSRV = new AutoPtr<ITextureView>(pDefaultNormalMap.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                TexDesc.Name = "Default physical description map for GLTF renderer";
                DataSpan.Fill(DefaultPhysical);  //for (auto & c : Data) c = 0x0000FF00;
                using var pDefaultPhysDesc = pDevice.CreateTexture(TexDesc, InitData);
                m_pDefaultPhysDescSRV = new AutoPtr<ITextureView>(pDefaultPhysDesc.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

                var Barriers = new List<StateTransitionDesc>
                {
                    new StateTransitionDesc{pResource = pWhiteTex.Obj,         OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                    new StateTransitionDesc{pResource = pBlackTex.Obj,         OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                    new StateTransitionDesc{pResource = pDefaultNormalMap.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                    new StateTransitionDesc{pResource = pDefaultPhysDesc.Obj,  OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE}
                };

                pCtx.TransitionResourceStates(Barriers);

                using var pDefaultSampler = pDevice.CreateSampler(Sam_LinearClamp);
                m_pWhiteTexSRV.Obj.SetSampler(pDefaultSampler.Obj);
                m_pBlackTexSRV.Obj.SetSampler(pDefaultSampler.Obj);
                m_pDefaultNormalMapSRV.Obj.SetSampler(pDefaultSampler.Obj);
            }

            if (CI.RTVFmt != TEXTURE_FORMAT.TEX_FORMAT_UNKNOWN || CI.DSVFmt != TEXTURE_FORMAT.TEX_FORMAT_UNKNOWN)
            {
                unsafe
                {
                    {
                        BufferDesc CBDesc = new BufferDesc();
                        CBDesc.Name = "GLTF node transforms CB";
                        CBDesc.Size = (uint)sizeof(GLTFNodeShaderTransforms);
                        CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                        CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                        CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                        m_TransformsCB = pDevice.CreateBuffer(CBDesc);
                    }

                    {
                        BufferDesc CBDesc = new BufferDesc();
                        CBDesc.Name = "GLTF attribs CB";
                        CBDesc.Size = (uint)(sizeof(GLTFAttribs));
                        CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                        CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                        CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                        m_GLTFAttribsCB = pDevice.CreateBuffer(CBDesc);
                    }

                    {
                        BufferDesc CBDesc = new BufferDesc();
                        CBDesc.Name = "GLTF joint tranforms";
                        CBDesc.Size = (uint)sizeof(float4x4) * m_Settings.MaxJointCount;
                        CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                        CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                        CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                        m_JointsBuffer = pDevice.CreateBuffer(CBDesc);
                    }

                    var Barriers = new List<StateTransitionDesc>
                    {
                        new StateTransitionDesc{pResource = m_TransformsCB.Obj,  OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                        new StateTransitionDesc{pResource = m_GLTFAttribsCB.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                        new StateTransitionDesc{pResource = m_JointsBuffer.Obj,  OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                    };
                    pCtx.TransitionResourceStates(Barriers);

                    var vsSource = shaderLoader.LoadShader("GLTF_PBR/private/RenderGLTF_PBR.vsh", "Common/public", "GLTF_PBR/public");
                    var psSource = shaderLoader.LoadShader("GLTF_PBR/private/RenderGLTF_PBR.psh", "Common/public", "GLTF_PBR/public", "PostProcess/ToneMapping/public");

                    CreatePSO(pDevice, false, false, vsSource, psSource);
                    CreatePSO(pDevice, true, false, vsSource, psSource);

                    CreatePSO(pDevice, false, true, vsSource, psSource);
                    //CreatePSO(pDevice, true, true, vsSource, psSource); //Sprites don't need shadows
                }
            }
        }

        public void Dispose()
        {
            m_pPrefilterEnvMapSRB?.Dispose();
            m_pPrefilterEnvMapPSO?.Dispose();
            m_pPrecomputeIrradianceCubeSRB?.Dispose();
            m_pPrecomputeIrradianceCubePSO?.Dispose();
            m_PrecomputeEnvMapAttribsCB?.Dispose();
            m_PSOCache.Dispose();
            m_TransformsCB.Dispose();
            m_GLTFAttribsCB.Dispose();
            m_JointsBuffer.Dispose();
            m_pDefaultPhysDescSRV.Dispose();
            m_pDefaultNormalMapSRV.Dispose();
            m_pBlackTexSRV.Dispose();
            m_pWhiteTexSRV.Dispose();
            m_pPrefilteredEnvMapSRV?.Dispose();
            m_pIrradianceCubeSRV?.Dispose();
            m_pBRDF_LUT_SRV?.Dispose();
        }
        private void PrecomputeBRDF(IRenderDevice pDevice, IDeviceContext pCtx)
        {
            TextureDesc TexDesc = new TextureDesc();
            TexDesc.Name = "GLTF BRDF Look-up texture";
            TexDesc.Type = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D;
            TexDesc.Usage = USAGE.USAGE_DEFAULT;
            TexDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE | BIND_FLAGS.BIND_RENDER_TARGET;
            TexDesc.Width = BRDF_LUT_Dim;
            TexDesc.Height = BRDF_LUT_Dim;
            TexDesc.Format = TEXTURE_FORMAT.TEX_FORMAT_RG16_FLOAT;
            TexDesc.MipLevels = 1;
            using var pBRDF_LUT = pDevice.CreateTexture(TexDesc, null);
            m_pBRDF_LUT_SRV = new AutoPtr<ITextureView>(pBRDF_LUT.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));

            GraphicsPipelineStateCreateInfo PSOCreateInfo = new GraphicsPipelineStateCreateInfo();
            PipelineStateDesc PSODesc = PSOCreateInfo.PSODesc;
            GraphicsPipelineDesc GraphicsPipeline = PSOCreateInfo.GraphicsPipeline;

            PSODesc.Name = "Precompute GLTF BRDF LUT PSO";
            PSODesc.PipelineType = PIPELINE_TYPE.PIPELINE_TYPE_GRAPHICS;

            GraphicsPipeline.NumRenderTargets = 1;
            GraphicsPipeline.RTVFormats_0 = TexDesc.Format;
            GraphicsPipeline.PrimitiveTopology = PRIMITIVE_TOPOLOGY.PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
            GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_NONE;
            GraphicsPipeline.DepthStencilDesc.DepthEnable = false;

            ShaderCreateInfo ShaderCI = new ShaderCreateInfo();
            ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;
            ShaderCI.UseCombinedTextureSamplers = true;

            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_VERTEX;
            ShaderCI.EntryPoint = "FullScreenTriangleVS";
            ShaderCI.Desc.Name = "Full screen triangle VS";
            ShaderCI.Source = shaderLoader.LoadShader("Common/private/FullScreenTriangleVS.fx");
            using var pVS = pDevice.CreateShader(ShaderCI);

            // Create pixel shader
            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_PIXEL;
            ShaderCI.EntryPoint = "PrecomputeBRDF_PS";
            ShaderCI.Desc.Name = "Precompute GLTF BRDF PS";
            ShaderCI.Source = shaderLoader.LoadShader("GLTF_PBR/private/PrecomputeGLTF_BRDF.psh", "Common/private/", "Common/public/");
            using var pPS = pDevice.CreateShader(ShaderCI);

            // Finally, create the pipeline state
            PSOCreateInfo.pVS = pVS.Obj;
            PSOCreateInfo.pPS = pPS.Obj;
            using var PrecomputeBRDF_PSO = pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
            pCtx.SetPipelineState(PrecomputeBRDF_PSO.Obj);

            var pRTVs = pBRDF_LUT.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_RENDER_TARGET); //Only one of these
            pCtx.SetRenderTarget(pRTVs, null, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            var attrs = new DrawAttribs { NumVertices = 3, Flags = DRAW_FLAGS.DRAW_FLAG_VERIFY_ALL };
            pCtx.Draw(attrs);

            var Barriers = new List<StateTransitionDesc>()
            {
                new StateTransitionDesc{pResource = pBRDF_LUT.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE}
            };
            pCtx.TransitionResourceStates(Barriers);
        }

        private void CreatePSO(IRenderDevice pDevice, bool enableShadows, bool isSprite, String vsSource, String psSource)
        {
            GraphicsPipelineStateCreateInfo PSOCreateInfo = new GraphicsPipelineStateCreateInfo();
            PipelineStateDesc PSODesc = PSOCreateInfo.PSODesc;
            GraphicsPipelineDesc GraphicsPipeline = PSOCreateInfo.GraphicsPipeline;

            PSODesc.Name = "Render GLTF PBR PSO";
            PSODesc.PipelineType = PIPELINE_TYPE.PIPELINE_TYPE_GRAPHICS;

            GraphicsPipeline.NumRenderTargets = 1;
            GraphicsPipeline.RTVFormats_0 = m_Settings.RTVFmt;
            GraphicsPipeline.DSVFormat = m_Settings.DSVFmt;
            GraphicsPipeline.PrimitiveTopology = PRIMITIVE_TOPOLOGY.PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
            GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_BACK;
            GraphicsPipeline.RasterizerDesc.FrontCounterClockwise = m_Settings.FrontCCW;

            ShaderCreateInfo ShaderCI = new ShaderCreateInfo();
            ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;
            ShaderCI.UseCombinedTextureSamplers = true;

            var Macros = new ShaderMacroHelper();
            Macros.AddShaderMacro("MAX_JOINT_COUNT", m_Settings.MaxJointCount);
            Macros.AddShaderMacro("ALLOW_DEBUG_VIEW", m_Settings.AllowDebugView);
            Macros.AddShaderMacro("TONE_MAPPING_MODE", "TONE_MAPPING_MODE_UNCHARTED2");
            Macros.AddShaderMacro("GLTF_PBR_USE_IBL", m_Settings.UseIBL);
            Macros.AddShaderMacro("GLTF_PBR_USE_AO", m_Settings.UseAO);
            Macros.AddShaderMacro("GLTF_PBR_USE_EMISSIVE", m_Settings.UseEmissive);
            Macros.AddShaderMacro("USE_TEXTURE_ATLAS", m_Settings.UseTextureAtlas);
            Macros.AddShaderMacro("PBR_WORKFLOW_METALLIC_ROUGHNESS", (Int32)PbrWorkflow.PBR_WORKFLOW_METALL_ROUGH);
            Macros.AddShaderMacro("PBR_WORKFLOW_SPECULAR_GLOSINESS", (Int32)PbrWorkflow.PBR_WORKFLOW_SPEC_GLOSS);
            Macros.AddShaderMacro("GLTF_ALPHA_MODE_OPAQUE", (Int32)PbrAlphaMode.ALPHA_MODE_OPAQUE);
            Macros.AddShaderMacro("GLTF_ALPHA_MODE_MASK", (Int32)PbrAlphaMode.ALPHA_MODE_MASK);
            Macros.AddShaderMacro("GLTF_ALPHA_MODE_BLEND", (Int32)PbrAlphaMode.ALPHA_MODE_BLEND);
            Macros.AddShaderMacro("ANOMALOUS_USE_SIMPLE_SHADOW", enableShadows);
            Macros.AddShaderMacro("ANOMALOUS_USE_SPRITE", isSprite);
            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_VERTEX;
            ShaderCI.EntryPoint = "main";
            ShaderCI.Desc.Name = "GLTF PBR VS";
            ShaderCI.Source = vsSource;
            using var pVS = pDevice.CreateShader(ShaderCI, Macros);

            // Create pixel shader
            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_PIXEL;
            ShaderCI.EntryPoint = "main";
            ShaderCI.Desc.Name = "GLTF PBR PS";
            ShaderCI.Source = psSource;
            using var pPS = pDevice.CreateShader(ShaderCI, Macros);

            var Inputs = new List<LayoutElement>
            {
                new LayoutElement{InputIndex = 0, BufferSlot = 0, NumComponents = 3, ValueType = VALUE_TYPE.VT_FLOAT32},   //float3 Pos     : ATTRIB0;
                new LayoutElement{InputIndex = 1, BufferSlot = 0, NumComponents = 3, ValueType = VALUE_TYPE.VT_FLOAT32},   //float3 Normal  : ATTRIB1;
                new LayoutElement{InputIndex = 2, BufferSlot = 0, NumComponents = 2, ValueType = VALUE_TYPE.VT_FLOAT32},   //float2 UV0     : ATTRIB2;
                new LayoutElement{InputIndex = 3, BufferSlot = 0, NumComponents = 2, ValueType = VALUE_TYPE.VT_FLOAT32},   //float2 UV1     : ATTRIB3;
                new LayoutElement{InputIndex = 4, BufferSlot = 1, NumComponents = 4, ValueType = VALUE_TYPE.VT_FLOAT32},   //float4 Joint0  : ATTRIB4;
                new LayoutElement{InputIndex = 5, BufferSlot = 1, NumComponents = 4, ValueType = VALUE_TYPE.VT_FLOAT32}    //float4 Weight0 : ATTRIB5;
            };
            PSOCreateInfo.GraphicsPipeline.InputLayout.LayoutElements = Inputs;

            PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_MUTABLE;
            var Vars = new List<ShaderResourceVariableDesc>
            {
                new ShaderResourceVariableDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_VERTEX, Name = "cbTransforms",      Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC},
                new ShaderResourceVariableDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL,  Name = "cbGLTFAttribs",     Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC},
                new ShaderResourceVariableDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_VERTEX, Name = "cbJointTransforms", Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC}
            };

            var ImtblSamplers = new List<ImmutableSamplerDesc>();
            if (m_Settings.UseImmutableSamplers)
            {
                var colorSampler = m_Settings.ColorMapImmutableSampler;
                if (isSprite)
                {
                    colorSampler = m_Settings.ColorMapImmutableSamplerSprite;
                }
                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_ColorMap", Desc = colorSampler });
                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_PhysicalDescriptorMap", Desc = m_Settings.PhysDescMapImmutableSampler });
                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_NormalMap", Desc = m_Settings.NormalMapImmutableSampler });
                if (enableShadows)
                {
                    var ComparsionSampler = new SamplerDesc();
                    ComparsionSampler.ComparisonFunc = COMPARISON_FUNCTION.COMPARISON_FUNC_LESS;
                    ComparsionSampler.MinFilter = FILTER_TYPE.FILTER_TYPE_COMPARISON_LINEAR;
                    ComparsionSampler.MagFilter = FILTER_TYPE.FILTER_TYPE_COMPARISON_LINEAR;
                    ComparsionSampler.MipFilter = FILTER_TYPE.FILTER_TYPE_COMPARISON_LINEAR;
                    ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_ShadowMap", Desc = ComparsionSampler });
                }
            }

            if (m_Settings.UseAO)
            {
                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_AOMap", Desc = m_Settings.AOMapImmutableSampler });
            }

            if (m_Settings.UseEmissive)
            {
                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_EmissiveMap", Desc = m_Settings.EmissiveMapImmutableSampler });
            }

            if (m_Settings.UseIBL)
            {
                Vars.Add(new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, Name = "g_BRDF_LUT", Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC });

                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_BRDF_LUT", Desc = Sam_LinearClamp });
                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_IrradianceMap", Desc = Sam_LinearClamp });
                ImtblSamplers.Add(new ImmutableSamplerDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_PrefilteredEnvMap", Desc = Sam_LinearClamp });
            }

            PSODesc.ResourceLayout.Variables = Vars;
            PSODesc.ResourceLayout.ImmutableSamplers = ImtblSamplers;

            PSOCreateInfo.pVS = pVS.Obj;
            PSOCreateInfo.pPS = pPS.Obj;

            {
                var Key = new PSOKey(PbrAlphaMode.ALPHA_MODE_OPAQUE, false, enableShadows, isSprite);

                using var pSingleSidedOpaquePSO = pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
                m_PSOCache.AddPSO(Key, pSingleSidedOpaquePSO.Obj);

                PSOCreateInfo.GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_NONE;

                Key.DoubleSided = true;

                using var pDobleSidedOpaquePSO = pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
                m_PSOCache.AddPSO(Key, pDobleSidedOpaquePSO.Obj);
            }

            PSOCreateInfo.GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_BACK;

            var RT0 = PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets_0;
            RT0.BlendEnable = true;
            RT0.SrcBlend = BLEND_FACTOR.BLEND_FACTOR_SRC_ALPHA;
            RT0.DestBlend = BLEND_FACTOR.BLEND_FACTOR_INV_SRC_ALPHA;
            RT0.BlendOp = BLEND_OPERATION.BLEND_OPERATION_ADD;
            RT0.SrcBlendAlpha = BLEND_FACTOR.BLEND_FACTOR_INV_SRC_ALPHA;
            RT0.DestBlendAlpha = BLEND_FACTOR.BLEND_FACTOR_ZERO;
            RT0.BlendOpAlpha = BLEND_OPERATION.BLEND_OPERATION_ADD;

            {
                var Key = new PSOKey(PbrAlphaMode.ALPHA_MODE_BLEND, false, enableShadows, isSprite);

                using var pSingleSidedBlendPSO = pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
                m_PSOCache.AddPSO(Key, pSingleSidedBlendPSO.Obj);

                PSOCreateInfo.GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_NONE;

                Key.DoubleSided = true;

                using var pDoubleSidedBlendPSO = pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
                m_PSOCache.AddPSO(Key, pDoubleSidedBlendPSO.Obj);
            }

            foreach (var PSO in m_PSOCache.Items)
            {
                if (m_Settings.UseIBL)
                {
                    PSO.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_BRDF_LUT").Set(m_pBRDF_LUT_SRV.Obj);
                }
                PSO.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "cbTransforms").Set(m_TransformsCB.Obj);
                PSO.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "cbGLTFAttribs").Set(m_GLTFAttribsCB.Obj);
                PSO.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "cbJointTransforms").Set(m_JointsBuffer.Obj);
            }
        }

        private void InitCommonSRBVars(IShaderResourceBinding pSRB,
                                          IBuffer pCameraAttribs,
                                          IBuffer pLightAttribs)
        {
            if (pCameraAttribs != null)
            {
                pSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "cbCameraAttribs")?.Set(pCameraAttribs);
                pSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "cbCameraAttribs")?.Set(pCameraAttribs);
            }

            if (pLightAttribs != null)
            {
                pSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "cbLightAttribs")?.Set(pLightAttribs);
                pSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "cbLightAttribs")?.Set(pLightAttribs);
            }

            if (m_Settings.UseIBL)
            {
                pSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_IrradianceMap")?.Set(m_pIrradianceCubeSRV.Obj);
                pSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_PrefilteredEnvMap")?.Set(m_pPrefilteredEnvMapSRV.Obj);
            }
        }

        private static void SetTexture(ITexture pTexture, ITextureView pDefaultTexSRV, String VarName, IShaderResourceBinding pSRB) //
        {
            AutoPtr<ITextureView> textureViewPtr = null;
            try
            {
                ITextureView pTexSRV = null;

                if (pTexture != null)
                {
                    if (pTexture.GetDesc_Type == RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D_ARRAY) //This is the only one
                    {
                        pTexSRV = pTexture.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE);
                    }
                    else
                    {
                        TextureViewDesc SRVDesc = new TextureViewDesc();
                        SRVDesc.ViewType = TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE;
                        SRVDesc.TextureDim = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D_ARRAY;
                        SRVDesc.NumMipLevels = pTexture.GetDesc_MipLevels;
                        textureViewPtr = pTexture.CreateView(SRVDesc);
                        pTexSRV = textureViewPtr.Obj;
                    }
                }

                if (pTexSRV == null)
                {
                    pTexSRV = pDefaultTexSRV;
                }

                pSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, VarName)?.Set(pTexSRV);
            }
            finally
            {
                textureViewPtr?.Dispose();
            }
        }

        public AutoPtr<IShaderResourceBinding> CreateMaterialSRB(
                                          IBuffer pCameraAttribs,
                                          IBuffer pLightAttribs,
                                          ITexture baseColorMap = null,
                                          ITexture normalMap = null,
                                          ITexture physicalDescriptorMap = null,
                                          ITexture aoMap = null,
                                          ITexture emissiveMap = null,
                                          ITextureView shadowMapSRV = null,
                                          PbrAlphaMode alphaMode = PbrAlphaMode.ALPHA_MODE_OPAQUE,
                                          bool doubleSided = false,
                                          bool isSprite = false)
        {
            var pPSO = m_PSOCache.GetPSO(new PSOKey(alphaMode, doubleSided, shadowMapSRV != null, isSprite));

            //Replaces ppMaterialSRB, this is returned
            var pSRB = pPSO.CreateShaderResourceBinding(true);
            if (pSRB == null)
            {
                throw new Exception("Failed to create material SRB");
            }

            InitCommonSRBVars(pSRB.Obj, pCameraAttribs, pLightAttribs);

            SetTexture(baseColorMap, m_pWhiteTexSRV.Obj, "g_ColorMap", pSRB.Obj);
            SetTexture(physicalDescriptorMap, m_pDefaultPhysDescSRV.Obj, "g_PhysicalDescriptorMap", pSRB.Obj);
            SetTexture(normalMap, m_pDefaultNormalMapSRV.Obj, "g_NormalMap", pSRB.Obj);
            if (m_Settings.UseAO)
            {
                SetTexture(aoMap, m_pWhiteTexSRV.Obj, "g_AOMap", pSRB.Obj);
            }
            if (m_Settings.UseEmissive)
            {
                SetTexture(emissiveMap, m_pBlackTexSRV.Obj, "g_EmissiveMap", pSRB.Obj);
            }

            if (shadowMapSRV != null)
            {
                pSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_ShadowMap").Set(shadowMapSRV);
            }

            return pSRB;
        }

        public void PrecomputeCubemaps(IRenderDevice pDevice, IDeviceContext pCtx, ITextureView pEnvironmentMap)
        {
            if (!m_Settings.UseIBL)
            {
                //LOG_WARNING_MESSAGE("IBL is disabled, so precomputing cube maps will have no effect");
                return;
            }

            if (m_PrecomputeEnvMapAttribsCB == null)
            {
                unsafe
                {
                    BufferDesc CBDesc = new BufferDesc();
                    CBDesc.Name = "Precompute env map attribs CB";
                    CBDesc.Size = (uint)sizeof(PrecomputeEnvMapAttribs);
                    CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                    CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                    CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                    m_PrecomputeEnvMapAttribsCB = pDevice.CreateBuffer(CBDesc);
                }
            }

            if (m_pPrecomputeIrradianceCubePSO == null)
            {
                ShaderCreateInfo ShaderCI = new ShaderCreateInfo();
                ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;
                ShaderCI.UseCombinedTextureSamplers = true;

                ShaderMacroHelper Macros = new ShaderMacroHelper();
                Macros.AddShaderMacro("NUM_PHI_SAMPLES", 64);
                Macros.AddShaderMacro("NUM_THETA_SAMPLES", 32);
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_VERTEX;
                ShaderCI.EntryPoint = "main";
                ShaderCI.Desc.Name = "Cubemap face VS";
                ShaderCI.Source = shaderLoader.LoadShader("GLTF_PBR/private/CubemapFace.vsh");
                using var pVS = pDevice.CreateShader(ShaderCI, Macros);

                // Create pixel shader
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_PIXEL;
                ShaderCI.EntryPoint = "main";
                ShaderCI.Desc.Name = "Precompute irradiance cube map PS";
                ShaderCI.Source = shaderLoader.LoadShader("GLTF_PBR/private/ComputeIrradianceMap.psh");
                using var pPS = pDevice.CreateShader(ShaderCI, Macros);

                GraphicsPipelineStateCreateInfo PSOCreateInfo = new GraphicsPipelineStateCreateInfo();
                PipelineStateDesc PSODesc = PSOCreateInfo.PSODesc;
                GraphicsPipelineDesc GraphicsPipeline = PSOCreateInfo.GraphicsPipeline;

                PSODesc.Name = "Precompute irradiance cube PSO";
                PSODesc.PipelineType = PIPELINE_TYPE.PIPELINE_TYPE_GRAPHICS;

                GraphicsPipeline.NumRenderTargets = 1;
                GraphicsPipeline.RTVFormats_0 = IrradianceCubeFmt;
                GraphicsPipeline.PrimitiveTopology = PRIMITIVE_TOPOLOGY.PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
                GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_NONE;
                GraphicsPipeline.DepthStencilDesc.DepthEnable = false;

                PSOCreateInfo.pVS = pVS.Obj;
                PSOCreateInfo.pPS = pPS.Obj;

                PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC;
                var Vars = new List<ShaderResourceVariableDesc>
                {
                    new ShaderResourceVariableDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, Name = "g_EnvironmentMap", Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC}
                };
                PSODesc.ResourceLayout.Variables = Vars;

                var ImtblSamplers = new List<ImmutableSamplerDesc>
                {
                    new ImmutableSamplerDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_PIXEL, SamplerOrTextureName = "g_EnvironmentMap", Desc = Sam_LinearClamp}
                };
                PSODesc.ResourceLayout.ImmutableSamplers = ImtblSamplers;

                m_pPrecomputeIrradianceCubePSO = pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
                m_pPrecomputeIrradianceCubePSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "cbTransform").Set(m_PrecomputeEnvMapAttribsCB.Obj);
                m_pPrecomputeIrradianceCubeSRB = m_pPrecomputeIrradianceCubePSO.Obj.CreateShaderResourceBinding(true);
            }

            if (m_pPrefilterEnvMapPSO == null)
            {
                ShaderCreateInfo ShaderCI = new ShaderCreateInfo();
                ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;
                ShaderCI.UseCombinedTextureSamplers = true;

                ShaderMacroHelper Macros = new ShaderMacroHelper();
                Macros.AddShaderMacro("OPTIMIZE_SAMPLES", 1);

                // Create vertex shader
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_VERTEX;
                ShaderCI.EntryPoint = "main";
                ShaderCI.Desc.Name = "Cubemap face VS";
                ShaderCI.Source = shaderLoader.LoadShader("GLTF_PBR/private/CubemapFace.vsh", "Common/public");
                using var pVS = pDevice.CreateShader(ShaderCI, Macros);

                // Create pixel shader
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_PIXEL;
                ShaderCI.EntryPoint = "main";
                ShaderCI.Desc.Name = "Prefilter environment map PS";
                ShaderCI.Source = shaderLoader.LoadShader("GLTF_PBR/private/PrefilterEnvMap.psh", "Common/public");
                using var pPS = pDevice.CreateShader(ShaderCI, Macros);

                GraphicsPipelineStateCreateInfo PSOCreateInfo = new GraphicsPipelineStateCreateInfo();
                PipelineStateDesc PSODesc = PSOCreateInfo.PSODesc;
                GraphicsPipelineDesc GraphicsPipeline = PSOCreateInfo.GraphicsPipeline;

                PSODesc.Name = "Prefilter environment map PSO";
                PSODesc.PipelineType = PIPELINE_TYPE.PIPELINE_TYPE_GRAPHICS;

                GraphicsPipeline.NumRenderTargets = 1;
                GraphicsPipeline.RTVFormats_0 = PrefilteredEnvMapFmt;
                GraphicsPipeline.PrimitiveTopology = PRIMITIVE_TOPOLOGY.PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
                GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_NONE;
                GraphicsPipeline.DepthStencilDesc.DepthEnable = false;

                PSOCreateInfo.pVS = pVS.Obj;
                PSOCreateInfo.pPS = pPS.Obj;

                PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC;
                // clang-format off
                var Vars = new List<ShaderResourceVariableDesc>
                {
                    new ShaderResourceVariableDesc(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_EnvironmentMap", SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC)
                };
                PSODesc.ResourceLayout.Variables = Vars;

                var ImtblSamplers = new List<ImmutableSamplerDesc>
                {
                    new ImmutableSamplerDesc(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_EnvironmentMap", Sam_LinearClamp)
                };
                PSODesc.ResourceLayout.ImmutableSamplers = ImtblSamplers;

                m_pPrefilterEnvMapPSO = pDevice.CreateGraphicsPipelineState(PSOCreateInfo);
                m_pPrefilterEnvMapPSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "cbTransform").Set(m_PrecomputeEnvMapAttribsCB.Obj);
                m_pPrefilterEnvMapPSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "FilterAttribs").Set(m_PrecomputeEnvMapAttribsCB.Obj);
                m_pPrefilterEnvMapSRB = m_pPrefilterEnvMapPSO.Obj.CreateShaderResourceBinding(true);
            }


            var Matrices = new float4x4[6]
            {
                /* +X */ float4x4.RotationY(+MathFloat.PI / 2f),
                /* -X */ float4x4.RotationY(-MathFloat.PI / 2f),
                /* +Y */ float4x4.RotationX(-MathFloat.PI / 2f),
                /* -Y */ float4x4.RotationX(+MathFloat.PI / 2f),
                /* +Z */ float4x4.Identity,
                /* -Z */ float4x4.RotationY(MathFloat.PI)
            };

            pCtx.SetPipelineState(m_pPrecomputeIrradianceCubePSO.Obj);
            m_pPrecomputeIrradianceCubeSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_EnvironmentMap").Set(pEnvironmentMap);
            pCtx.CommitShaderResources(m_pPrecomputeIrradianceCubeSRB.Obj, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            var pIrradianceCube = m_pIrradianceCubeSRV.Obj.GetTexture();
            var IrradianceCubeDesc_MipLevels = pIrradianceCube.GetDesc_MipLevels;
            for (Uint32 mip = 0; mip < IrradianceCubeDesc_MipLevels; ++mip)
            {
                for (Uint32 face = 0; face < 6; ++face)
                {
                    var RTVDesc = new TextureViewDesc { ViewType = TEXTURE_VIEW_TYPE.TEXTURE_VIEW_RENDER_TARGET, TextureDim = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D_ARRAY };
                    RTVDesc.Name = "RTV for irradiance cube texture";
                    RTVDesc.MostDetailedMip = mip;
                    RTVDesc.FirstArraySlice = face;
                    RTVDesc.NumArraySlices = 1;
                    using var pRTV = pIrradianceCube.CreateView(RTVDesc);
                    pCtx.SetRenderTarget(pRTV.Obj, null, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                    unsafe
                    {
                        IntPtr data = pCtx.MapBuffer(m_PrecomputeEnvMapAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);

                        PrecomputeEnvMapAttribs* Attribs = (PrecomputeEnvMapAttribs*)data.ToPointer(); //(pCtx, m_PrecomputeEnvMapAttribsCB, MAP_WRITE, MAP_FLAG_DISCARD);
                        Attribs->Rotation = Matrices[face];

                        pCtx.UnmapBuffer(m_PrecomputeEnvMapAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
                    }

                    var drawAttrs = new DrawAttribs { NumVertices = 4, Flags = DRAW_FLAGS.DRAW_FLAG_VERIFY_ALL };
                    pCtx.Draw(drawAttrs);
                }
            }

            pCtx.SetPipelineState(m_pPrefilterEnvMapPSO.Obj);
            m_pPrefilterEnvMapSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_PIXEL, "g_EnvironmentMap").Set(pEnvironmentMap);
            pCtx.CommitShaderResources(m_pPrefilterEnvMapSRB.Obj, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            var pPrefilteredEnvMap = m_pPrefilteredEnvMapSRV.Obj.GetTexture();
            var PrefilteredEnvMapDesc_MipLevels = pPrefilteredEnvMap.GetDesc_MipLevels;
            var PrefilteredEnvMapDesc_Width = pPrefilteredEnvMap.GetDesc_Width;
            for (Uint32 mip = 0; mip < PrefilteredEnvMapDesc_MipLevels; ++mip)
            {
                for (Uint32 face = 0; face < 6; ++face)
                {
                    var RTVDesc = new TextureViewDesc { ViewType = TEXTURE_VIEW_TYPE.TEXTURE_VIEW_RENDER_TARGET, TextureDim = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D_ARRAY };
                    RTVDesc.Name = "RTV for prefiltered env map cube texture";
                    RTVDesc.MostDetailedMip = mip;
                    RTVDesc.FirstArraySlice = face;
                    RTVDesc.NumArraySlices = 1;
                    using var pRTV = pPrefilteredEnvMap.CreateView(RTVDesc);
                    pCtx.SetRenderTarget(pRTV.Obj, null, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                    unsafe
                    {
                        IntPtr data = pCtx.MapBuffer(m_PrecomputeEnvMapAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);

                        PrecomputeEnvMapAttribs* Attribs = (PrecomputeEnvMapAttribs*)data;// (pCtx, m_PrecomputeEnvMapAttribsCB, MAP_WRITE, MAP_FLAG_DISCARD);
                        Attribs->Rotation = Matrices[face];
                        Attribs->Roughness = (float)(mip) / (float)(PrefilteredEnvMapDesc_MipLevels);
                        Attribs->EnvMapDim = (float)(PrefilteredEnvMapDesc_Width);
                        Attribs->NumSamples = 256;

                        pCtx.UnmapBuffer(m_PrecomputeEnvMapAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
                    }

                    var drawAttrs = new DrawAttribs { NumVertices = 4, Flags = DRAW_FLAGS.DRAW_FLAG_VERIFY_ALL };
                    pCtx.Draw(drawAttrs);
                }
            }

            var Barriers = new List<StateTransitionDesc>
            {
                new StateTransitionDesc{pResource = m_pPrefilteredEnvMapSRV.Obj.GetTexture(), OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE },
                new StateTransitionDesc{pResource = m_pIrradianceCubeSRV.Obj.GetTexture(),    OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE}
            };
            pCtx.TransitionResourceStates(Barriers);
        }

        //-------------------------- RENDERING --------------------------------

        public unsafe void Begin(IDeviceContext pCtx)
        {
            if (m_JointsBuffer != null)
            {
                IntPtr data = pCtx.MapBuffer(m_JointsBuffer.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);
                // In next-gen backends, dynamic buffers must be mapped before the first use in every frame
                var pJoints = (float4x4*)data.ToPointer();

                pCtx.UnmapBuffer(m_JointsBuffer.Obj, MAP_TYPE.MAP_WRITE);
            }
        }

        public void Render(IDeviceContext pCtx,
            IShaderResourceBinding materialSRB,
            IBuffer vertexBuffer,
            IBuffer skinVertexBuffer,
            IBuffer indexBuffer,
            Uint32 numIndices,
            ref Vector3 position,
            ref Quaternion rotation,
            PbrRenderAttribs renderAttribs
            )
        {
            //Have to take inverse of rotations to have renderer render them correctly
            var nodeMatrix = rotation.inverse().toRotationMatrix4x4() * Matrix4x4.Translation(position);
            Render(pCtx, materialSRB, vertexBuffer, skinVertexBuffer, indexBuffer, numIndices, ref nodeMatrix, renderAttribs);
        }

        public void Render(IDeviceContext pCtx,
            IShaderResourceBinding materialSRB,
            IBuffer vertexBuffer,
            IBuffer skinVertexBuffer,
            IBuffer indexBuffer,
            Uint32 numIndices,
            ref Vector3 position,
            ref Quaternion rotation,
            ref Vector3 scale,
            PbrRenderAttribs renderAttribs
            )
        {
            //Have to take inverse of rotations to have renderer render them correctly
            var nodeMatrix = Matrix4x4.Scale(scale) * rotation.inverse().toRotationMatrix4x4() * Matrix4x4.Translation(position);
            Render(pCtx, materialSRB, vertexBuffer, skinVertexBuffer, indexBuffer, numIndices, ref nodeMatrix, renderAttribs);
        }

        private unsafe void Render(IDeviceContext pCtx,
            IShaderResourceBinding materialSRB,
            IBuffer vertexBuffer,
            IBuffer skinVertexBuffer,
            IBuffer indexBuffer,
            Uint32 numIndices,
            ref Matrix4x4 nodeMatrix,
            PbrRenderAttribs renderAttribs
            )
        {
            IBuffer[] pBuffs = new IBuffer[] { vertexBuffer, skinVertexBuffer };
            pCtx.SetVertexBuffers(0, (uint)pBuffs.Length, pBuffs, null, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION, SET_VERTEX_BUFFERS_FLAGS.SET_VERTEX_BUFFERS_FLAG_RESET);
            pCtx.SetIndexBuffer(indexBuffer, 0, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            var Key = new PSOKey (renderAttribs.AlphaMode, renderAttribs.DoubleSided, renderAttribs.GetShadows, renderAttribs.IsSprite);
            var pCurrPSO = m_PSOCache.GetPSO(Key);
            pCtx.SetPipelineState(pCurrPSO);

            pCtx.CommitShaderResources(materialSRB, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            unsafe
            {
                IntPtr data = pCtx.MapBuffer(m_TransformsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);
                var transform = (GLTFNodeShaderTransforms*)data.ToPointer();

                transform->NodeMatrix = nodeMatrix;
                transform->JointCount = 0;

                pCtx.UnmapBuffer(m_TransformsCB.Obj, MAP_TYPE.MAP_WRITE);
            }

            unsafe
            {
                IntPtr data = pCtx.MapBuffer(m_JointsBuffer.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);
                var joints = (float4x4*)data.ToPointer();

                if (renderAttribs.IsSprite) //Get rid of this if
                {
                    float left = renderAttribs.SpriteUVLeft;
                    float top = renderAttribs.SpriteUVTop;
                    float right = renderAttribs.SpriteUVRight;
                    float bottom = renderAttribs.SpriteUVBottom;
                    //Cramming uv index into the Joint0 part of the struct for now and using the first joint matrix to pass the uvs
                    joints[0] = new float4x4(
                        //For some reason the shader reads this as column, then row
                        //It seems like the uvs would go down the first 2 columns not across these rows
                        //This needs to be refactored with its own data structures
                        left, right, right, left,
                        top, top, bottom, bottom,
                        0, 0, 0, 0,
                        0, 0, 0, 0
                        );
                }

                pCtx.UnmapBuffer(m_JointsBuffer.Obj, MAP_TYPE.MAP_WRITE);
            }

            unsafe
            {
                IntPtr data = pCtx.MapBuffer(m_GLTFAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);
                var pGLTFAttribs = (GLTFAttribs*)data.ToPointer();

                var MaterialInfo = &pGLTFAttribs->MaterialInfo;

                MaterialInfo->BaseColorFactor = renderAttribs.BaseColorFactor;
                MaterialInfo->EmissiveFactor = renderAttribs.EmissiveFactor;
                MaterialInfo->SpecularFactor = renderAttribs.SpecularFactor;

                MaterialInfo->Workflow = (int)renderAttribs.Workflow;
                MaterialInfo->BaseColorTextureUVSelector = renderAttribs.BaseColorTextureUVSelector;
                MaterialInfo->PhysicalDescriptorTextureUVSelector = renderAttribs.PhysicalDescriptorTextureUVSelector;
                MaterialInfo->NormalTextureUVSelector = renderAttribs.NormalTextureUVSelector;

                MaterialInfo->OcclusionTextureUVSelector = renderAttribs.OcclusionTextureUVSelector;
                MaterialInfo->EmissiveTextureUVSelector = renderAttribs.EmissiveTextureUVSelector;
                MaterialInfo->BaseColorSlice = renderAttribs.BaseColorSlice;
                MaterialInfo->PhysicalDescriptorSlice = renderAttribs.PhysicalDescriptorSlice;

                MaterialInfo->NormalSlice = renderAttribs.NormalSlice;
                MaterialInfo->OcclusionSlice = renderAttribs.OcclusionSlice;
                MaterialInfo->EmissiveSlice = renderAttribs.EmissiveSlice;
                MaterialInfo->MetallicFactor = renderAttribs.MetallicFactor;

                MaterialInfo->RoughnessFactor = renderAttribs.RoughnessFactor;
                MaterialInfo->AlphaMode = (int)renderAttribs.AlphaMode;
                MaterialInfo->AlphaMaskCutoff = renderAttribs.AlphaMaskCutoff;
                MaterialInfo->Dummy0 = renderAttribs.Dummy0;

                MaterialInfo->BaseColorUVScaleBias = renderAttribs.BaseColorUVScaleBias;
                MaterialInfo->PhysicalDescriptorUVScaleBias = renderAttribs.PhysicalDescriptorUVScaleBias;
                MaterialInfo->NormalMapUVScaleBias = renderAttribs.NormalMapUVScaleBias;
                MaterialInfo->OcclusionUVScaleBias = renderAttribs.OcclusionUVScaleBias;
                MaterialInfo->EmissiveUVScaleBias = renderAttribs.EmissiveUVScaleBias;

                var ShaderParams = &pGLTFAttribs->RenderParameters;

                ShaderParams->AverageLogLum = renderAttribs.AverageLogLum;
                ShaderParams->MiddleGray = renderAttribs.MiddleGray;
                ShaderParams->WhitePoint = renderAttribs.WhitePoint;
                ShaderParams->IBLScale = renderAttribs.IBLScale;
                ShaderParams->DebugViewType = (int)renderAttribs.DebugViewType;
                ShaderParams->OcclusionStrength = renderAttribs.OcclusionStrength;
                ShaderParams->EmissionScale = renderAttribs.EmissionScale;
                ShaderParams->PrefilteredCubeMipLevels = m_Settings.UseIBL ? m_pPrefilteredEnvMapSRV.Obj.GetTexture().GetDesc_MipLevels : 0f; //This line is valid

                pCtx.UnmapBuffer(m_GLTFAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
            }

            DrawIndexedAttribs DrawAttrs = new DrawIndexedAttribs();
            DrawAttrs.IndexType = VALUE_TYPE.VT_UINT32;
            DrawAttrs.NumIndices = numIndices;
            DrawAttrs.Flags = DRAW_FLAGS.DRAW_FLAG_VERIFY_ALL;
            pCtx.DrawIndexed(DrawAttrs);
        }
    }
}
