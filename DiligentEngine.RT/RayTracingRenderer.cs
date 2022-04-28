﻿using DiligentEngine.RT.ShaderSets;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    public class RayTracingRenderer : IDisposable
    {
        //Camera Settings
        float YFov = MathFloat.PI / 4.0f;
        float ZNear = 0.1f;
        float ZFar = 350f;

        private readonly GraphicsEngine graphicsEngine;
        private readonly RTImageBlitter imageBlitter;
        private readonly RTCameraAndLight cameraAndLight;
        private readonly GeneralShaders generalShaders;
        private byte maxRecursionDepth = 8;

        private AutoPtr<IBuffer> m_ConstantsCB;
        private Constants m_Constants;
        private AutoPtr<IPipelineState> m_pRayTracingPSO;
        private AutoPtr<IShaderResourceBinding> m_pRayTracingSRB;
        private AutoPtr<IShaderBindingTable> m_pSBT;
        AutoPtr<IBuffer> m_ScratchBuffer;
        AutoPtr<IBuffer> m_InstanceBuffer;
        uint lastNumInstances = 0;
        bool rebuildPipeline = true;
        bool rebindShaderResources;

        public event Action<RayTracingPipelineStateCreateInfo> OnSetupCreateInfo;


        public delegate void ShaderResourceBinder(IShaderResourceBinding rayTracingSRB);

        List<ShaderResourceBinder> shaderResourceBinders = new List<ShaderResourceBinder>();

        public unsafe RayTracingRenderer
        (
            GraphicsEngine graphicsEngine,
            RTImageBlitter imageBlitter,
            RTCameraAndLight cameraAndLight,
            GeneralShaders generalShaders
        )
        {
            this.graphicsEngine = graphicsEngine;
            this.imageBlitter = imageBlitter;
            this.cameraAndLight = cameraAndLight;
            this.generalShaders = generalShaders;
            maxRecursionDepth = (byte)Math.Min(maxRecursionDepth, graphicsEngine.RenderDevice.DeviceProperties_MaxRayTracingRecursionDepth);
            m_Constants = Constants.CreateDefault(maxRecursionDepth);

            generalShaders.Setup(cameraAndLight);
        }

        public void Dispose()
        {
            m_pSBT?.Dispose();
            DestroyPSO();
        }

        private void DestroyPSO()
        {
            m_InstanceBuffer?.Dispose();
            m_ScratchBuffer?.Dispose();
            m_pRayTracingSRB?.Dispose();
            m_pRayTracingPSO?.Dispose();
            m_ConstantsCB?.Dispose();
        }

        private unsafe RayTracingPipelineStateCreateInfo CreatePSOCreateInfo()
        {
            // Prepare ray tracing pipeline description.
            var PSOCreateInfo = new RayTracingPipelineStateCreateInfo();

            PSOCreateInfo.PSODesc.Name = "Ray tracing PSO";
            PSOCreateInfo.PSODesc.PipelineType = PIPELINE_TYPE.PIPELINE_TYPE_RAY_TRACING;

            // Setup shader groups
            PSOCreateInfo.pGeneralShaders = new List<RayTracingGeneralShaderGroup>();

            PSOCreateInfo.pTriangleHitShaders = new List<RayTracingTriangleHitShaderGroup>();

            PSOCreateInfo.pProceduralHitShaders = new List<RayTracingProceduralHitShaderGroup>();

            //Set this to the size of the largest root shader record size
            PSOCreateInfo.RayTracingPipeline.ShaderRecordSize = (ushort)sizeof(HLSL.BlasInstanceData);

            // DirectX 12 only: set attribute and payload size. Values should be as small as possible to minimize the memory usage.
            PSOCreateInfo.MaxAttributeSize = (uint)sizeof(/*BuiltInTriangleIntersectionAttributes*/ Vector2);
            PSOCreateInfo.MaxPayloadSize = (uint)Math.Max(Math.Max(sizeof(HLSL.PrimaryRayPayload), sizeof(HLSL.ShadowRayPayload)), sizeof(HLSL.EmissiveRayPayload));

            // Specify the maximum ray recursion depth.
            // WARNING: the driver does not track the recursion depth and it is the
            //          application's responsibility to not exceed the specified limit.
            //          The value is used to reserve the necessary stack size and
            //          exceeding it will likely result in driver crash.
            PSOCreateInfo.RayTracingPipeline.MaxRecursionDepth = maxRecursionDepth;

            // Define immutable sampler for g_Texture and g_GroundTexture. Immutable samplers should be used whenever possible
            var SamLinearWrapDesc = new SamplerDesc
            {
                MinFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
                MagFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
                MipFilter = FILTER_TYPE.FILTER_TYPE_LINEAR,
                AddressU = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_WRAP,
                AddressV = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_WRAP,
                AddressW = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_WRAP
            };

            var SamPointWrapDesc = new SamplerDesc
            {
                MinFilter = FILTER_TYPE.FILTER_TYPE_POINT,
                MagFilter = FILTER_TYPE.FILTER_TYPE_POINT,
                MipFilter = FILTER_TYPE.FILTER_TYPE_POINT,
                AddressU = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_WRAP,
                AddressV = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_WRAP,
                AddressW = TEXTURE_ADDRESS_MODE.TEXTURE_ADDRESS_WRAP
            };

            var ImmutableSamplers = new List<ImmutableSamplerDesc>
            {
                new ImmutableSamplerDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, SamplerOrTextureName = "g_SamLinearWrap", Desc = SamLinearWrapDesc},
                new ImmutableSamplerDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, SamplerOrTextureName = "g_SamLinearWrap", Desc = SamLinearWrapDesc},

                new ImmutableSamplerDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, SamplerOrTextureName = "g_SamPointWrap", Desc = SamPointWrapDesc},
                new ImmutableSamplerDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, SamplerOrTextureName = "g_SamPointWrap", Desc = SamPointWrapDesc}
            };

            var Variables = new List<ShaderResourceVariableDesc> //
            {
                new ShaderResourceVariableDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_MISS | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, Name = "g_ConstantsCB", Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC},
                new ShaderResourceVariableDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, Name = "g_TLAS", Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC},
                new ShaderResourceVariableDesc{ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN, Name = "g_ColorBuffer", Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC} //This is the buffer where the rays are written
            };

            PSOCreateInfo.PSODesc.ResourceLayout.Variables = Variables;
            PSOCreateInfo.PSODesc.ResourceLayout.ImmutableSamplers = ImmutableSamplers;
            PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_MUTABLE;

            generalShaders.AddToCreateInfo(PSOCreateInfo);
            OnSetupCreateInfo?.Invoke(PSOCreateInfo);

            return PSOCreateInfo;
        }

        unsafe void CreateRayTracingPSO()
        {
            var m_pDevice = graphicsEngine.RenderDevice;

            // Create a buffer with shared constants.
            BufferDesc BuffDesc = new BufferDesc();
            BuffDesc.Name = "Constant buffer";
            BuffDesc.uiSizeInBytes = (uint)sizeof(Constants);
            BuffDesc.Usage = USAGE.USAGE_DEFAULT;
            BuffDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;

            m_ConstantsCB = m_pDevice.CreateBuffer(BuffDesc);
            //VERIFY_EXPR(m_ConstantsCB != nullptr);
            var createInfo = CreatePSOCreateInfo();
            this.m_pRayTracingPSO = m_pDevice.CreateRayTracingPipelineState(createInfo);
            //VERIFY_EXPR(m_pRayTracingPSO != nullptr);

            m_pRayTracingPSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_GEN, "g_ConstantsCB").Set(m_ConstantsCB.Obj);
            m_pRayTracingPSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_MISS, "g_ConstantsCB").Set(m_ConstantsCB.Obj);
            m_pRayTracingPSO.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, "g_ConstantsCB").Set(m_ConstantsCB.Obj);

            m_pRayTracingSRB = m_pRayTracingPSO.Obj.CreateShaderResourceBinding(true);
            //VERIFY_EXPR(m_pRayTracingSRB != nullptr);
        }

        void CreateSBT()
        {
            var m_pDevice = graphicsEngine.RenderDevice;
            // Create shader binding table.

            var SBTDesc = new ShaderBindingTableDesc();
            SBTDesc.Name = "SBT";
            SBTDesc.pPSO = m_pRayTracingPSO.Obj;

            m_pSBT = m_pDevice.CreateSBT(SBTDesc);
            //VERIFY_EXPR(m_pSBT != nullptr);

            m_pSBT.Obj.BindRayGenShader("Main", IntPtr.Zero, 0);

            m_pSBT.Obj.BindMissShader("PrimaryMiss", RtStructures.PRIMARY_RAY_INDEX, IntPtr.Zero, 0);
            m_pSBT.Obj.BindMissShader("ShadowMiss", RtStructures.SHADOW_RAY_INDEX, IntPtr.Zero, 0);
            m_pSBT.Obj.BindMissShader("EmissiveMiss", RtStructures.EMISSIVE_RAY_INDEX, IntPtr.Zero, 0);
        }

        private RTInstances lastInstances;

        AutoPtr<ITopLevelAS> UpdateTLAS(RTInstances activeInstances)
        {
            var m_pDevice = graphicsEngine.RenderDevice;
            var m_pImmediateContext = graphicsEngine.ImmediateContext;
            bool rebuildScratchBuffers = rebuildPipeline;
            bool rebuildSbt = rebuildPipeline;

            if(lastInstances != activeInstances)
            {
                rebuildSbt = true;
                lastInstances = activeInstances;
            }

            if(activeInstances == null)
            {
                return null;
            }

            var instances = activeInstances.Instances;

            uint numInstances = (uint)instances.Count;
            if (numInstances == 0)
            {
                return null;
            }

            if (numInstances != lastNumInstances)
            {
                rebuildSbt = true; //Not 100% sure about this, but its good for now
                rebuildScratchBuffers = true;
                lastNumInstances = numInstances;
            }

            if (rebuildSbt)
            {
                m_pSBT?.Dispose();
            }

            if (rebuildPipeline)
            {
                DestroyPSO();
                //These are disposed now, set them to null so they are recreated below
                m_ScratchBuffer = null;
                m_InstanceBuffer = null;

                CreateRayTracingPSO();
                rebuildPipeline = false;
                rebindShaderResources = true;
            }

            if (rebindShaderResources && m_pRayTracingSRB != null)
            {
                BindShaderResources(m_pRayTracingSRB.Obj);
                rebindShaderResources = false;
            }

            if (rebuildSbt)
            {
                CreateSBT();
            }

            if (rebuildScratchBuffers)
            {
                m_ScratchBuffer?.Dispose();
                m_ScratchBuffer = null;
                m_InstanceBuffer?.Dispose();
                m_InstanceBuffer = null;
            }

            // Create TLAS
            var TLASDesc = new TopLevelASDesc();
            TLASDesc.Name = "TLAS";
            TLASDesc.MaxInstanceCount = numInstances;
            TLASDesc.Flags = RAYTRACING_BUILD_AS_FLAGS.RAYTRACING_BUILD_AS_ALLOW_UPDATE | RAYTRACING_BUILD_AS_FLAGS.RAYTRACING_BUILD_AS_PREFER_FAST_TRACE;

            var m_pTLAS = m_pDevice.CreateTLAS(TLASDesc);
            //VERIFY_EXPR(m_pTLAS != nullptr);

            m_pRayTracingSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_GEN, "g_TLAS").Set(m_pTLAS.Obj);
            m_pRayTracingSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, "g_TLAS").Set(m_pTLAS.Obj);

            // Create scratch buffer
            if (m_ScratchBuffer == null)
            {
                m_ScratchBuffer = m_pDevice.CreateBuffer(new BufferDesc()
                {
                    Name = "TLAS Scratch Buffer",
                    Usage = USAGE.USAGE_DEFAULT,
                    BindFlags = BIND_FLAGS.BIND_RAY_TRACING,
                    uiSizeInBytes = Math.Max(m_pTLAS.Obj.ScratchBufferSizes_Build, m_pTLAS.Obj.ScratchBufferSizes_Update)
                }, new BufferData());
            }

            // Create instance buffer
            if (m_InstanceBuffer == null)
            {
                var BuffDesc = new BufferDesc();
                BuffDesc.Name = "TLAS Instance Buffer";
                BuffDesc.Usage = USAGE.USAGE_DEFAULT;
                BuffDesc.BindFlags = BIND_FLAGS.BIND_RAY_TRACING;
                BuffDesc.uiSizeInBytes = ITopLevelAS.TLAS_INSTANCE_DATA_SIZE * numInstances;

                m_InstanceBuffer = m_pDevice.CreateBuffer(BuffDesc, new BufferData());
                //VERIFY_EXPR(m_InstanceBuffer != nullptr);
            }

            // Build or update TLAS
            var Attribs = new BuildTLASAttribs();
            Attribs.pTLAS = m_pTLAS.Obj;
            Attribs.Update = false;

            // Scratch buffer will be used to store temporary data during TLAS build or update.
            // Previous content in the scratch buffer will be discarded.
            Attribs.pScratchBuffer = m_ScratchBuffer.Obj;

            // Instance buffer will store instance data during TLAS build or update.
            // Previous content in the instance buffer will be discarded.
            Attribs.pInstanceBuffer = m_InstanceBuffer.Obj;

            // Instances will be converted to the format that is required by the graphics driver and copied to the instance buffer.
            Attribs.pInstances = instances;

            // Bind hit shaders per instance, it allows you to change the number of geometries in BLAS without invalidating the shader binding table.
            Attribs.BindingMode = HIT_GROUP_BINDING_MODE.HIT_GROUP_BINDING_MODE_PER_INSTANCE;
            Attribs.HitGroupStride = RtStructures.HIT_GROUP_STRIDE;

            // Allow engine to change resource states.
            Attribs.TLASTransitionMode = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION;
            Attribs.BLASTransitionMode = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION;
            Attribs.InstanceBufferTransitionMode = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION;
            Attribs.ScratchBufferTransitionMode = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION;

            m_pImmediateContext.BuildTLAS(Attribs);

            // Hit groups for primary ray
            activeInstances.BindShaders(m_pSBT.Obj, m_pTLAS.Obj);

            // Hit groups for shadow ray.
            // null means no shaders are bound and hit shader invocation will be skipped.
            m_pSBT.Obj.BindHitGroupForTLAS(m_pTLAS.Obj, RtStructures.SHADOW_RAY_INDEX, null, IntPtr.Zero, 0);

            return m_pTLAS;
        }

        /// <summary>
        /// Render. Returns false if no other rending is needed. True if the rt scene is not ready and rendering should be handled by the caller
        /// </summary>
        public unsafe bool Render(RTInstances activeInstances, Vector3 cameraPos, Quaternion cameraRot)
        {
            var swapChain = graphicsEngine.SwapChain;
            var m_pImmediateContext = graphicsEngine.ImmediateContext;

            //TODO: Might be able to avoid recreating this like the other buffers, this also seems ok
            //So really need more info about behavior to decide here
            using var tlas = UpdateTLAS(activeInstances);
            if (tlas == null)
            {
                //Client needs to render
                return true;
            }

            // Update constants
            {
                var pDSV = swapChain.GetDepthBufferDSV();
                var preTransform = swapChain.GetDesc_PreTransform;

                //= new Vector3(0f, 0f, -15f);
                var preTransformMatrix = CameraHelpers.GetSurfacePretransformMatrix(new Vector3(0, 0, 1), preTransform);
                var cameraProj = CameraHelpers.GetAdjustedProjectionMatrix(YFov, ZNear, ZFar, imageBlitter.Width, imageBlitter.Height, preTransform);
                cameraAndLight.GetCameraPosition(cameraPos, cameraRot, preTransformMatrix, cameraProj, out var CameraWorldPos, out var CameraViewProj);

                var Frustum = new ViewFrustum();
                cameraAndLight.ExtractViewFrustumPlanesFromMatrix(CameraViewProj, Frustum, false);

                // Normalize frustum planes.
                for (ViewFrustum.PLANE_IDX i = 0; i < ViewFrustum.PLANE_IDX.NUM_PLANES; ++i)
                {
                    Plane3D plane = Frustum.GetPlane(i);
                    float invlen = 1.0f / plane.Normal.length();
                    plane.Normal *= invlen;
                    plane.Distance *= invlen;
                }

                // Calculate ray formed by the intersection two planes.
                void GetPlaneIntersection(ViewFrustum.PLANE_IDX lhs, ViewFrustum.PLANE_IDX rhs, out Vector4 result)
                {
                    var lp = Frustum.GetPlane(lhs);
                    var rp = Frustum.GetPlane(rhs);

                    Vector3 dir = lp.Normal.cross(rp.Normal);
                    float len = dir.length2();

                    //VERIFY_EXPR(len > 1.0e-5);

                    var v3result = dir * (1.0f / MathF.Sqrt(len));
                    result = new Vector4(v3result.x, v3result.y, v3result.z, 0);
                };

                GetPlaneIntersection(ViewFrustum.PLANE_IDX.BOTTOM_PLANE_IDX, ViewFrustum.PLANE_IDX.LEFT_PLANE_IDX, out m_Constants.FrustumRayLB);
                GetPlaneIntersection(ViewFrustum.PLANE_IDX.LEFT_PLANE_IDX, ViewFrustum.PLANE_IDX.TOP_PLANE_IDX, out m_Constants.FrustumRayLT);
                GetPlaneIntersection(ViewFrustum.PLANE_IDX.RIGHT_PLANE_IDX, ViewFrustum.PLANE_IDX.BOTTOM_PLANE_IDX, out m_Constants.FrustumRayRB);
                GetPlaneIntersection(ViewFrustum.PLANE_IDX.TOP_PLANE_IDX, ViewFrustum.PLANE_IDX.RIGHT_PLANE_IDX, out m_Constants.FrustumRayRT);

                m_Constants.CameraPos = new Vector4(CameraWorldPos.x, CameraWorldPos.y, CameraWorldPos.z, 1.0f);

                //Need to invert going into the shader
                m_Constants.LightPos_0 = cameraAndLight.LightPos[0] * -1;
                m_Constants.LightPos_1 = cameraAndLight.LightPos[1] * -1;
                m_Constants.LightPos_2 = cameraAndLight.LightPos[2] * -1;
                m_Constants.LightPos_3 = cameraAndLight.LightPos[3] * -1;
                m_Constants.LightPos_4 = cameraAndLight.LightPos[4] * -1;
                m_Constants.LightPos_5 = cameraAndLight.LightPos[5] * -1;
                m_Constants.LightPos_6 = cameraAndLight.LightPos[6] * -1;
                m_Constants.LightPos_7 = cameraAndLight.LightPos[7] * -1;
                m_Constants.LightPos_8 = cameraAndLight.LightPos[8] * -1;
                m_Constants.LightPos_9 = cameraAndLight.LightPos[9] * -1;

                var numLights = m_Constants.NumActiveLights = cameraAndLight.NumActiveLights;
                Color color;
                color = cameraAndLight.LightColor[0];
                m_Constants.LightColor_0 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[0]);
                color = cameraAndLight.LightColor[1];
                m_Constants.LightColor_1 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[1]);
                color = cameraAndLight.LightColor[2];
                m_Constants.LightColor_2 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[2]);
                color = cameraAndLight.LightColor[3];
                m_Constants.LightColor_3 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[3]);
                color = cameraAndLight.LightColor[4];
                m_Constants.LightColor_4 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[4]);
                color = cameraAndLight.LightColor[5];
                m_Constants.LightColor_5 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[5]);
                color = cameraAndLight.LightColor[6];
                m_Constants.LightColor_6 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[6]);
                color = cameraAndLight.LightColor[7];
                m_Constants.LightColor_7 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[7]);
                color = cameraAndLight.LightColor[8];
                m_Constants.LightColor_8 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[8]);
                color = cameraAndLight.LightColor[9];
                m_Constants.LightColor_9 = new Vector4(color.r * numLights, color.g * numLights, color.b * numLights, cameraAndLight.LightLength[9]);

                color = cameraAndLight.MissPallete[0]; m_Constants.Pallete_0 = new Vector4(color.r, color.g, color.b, 0);
                color = cameraAndLight.MissPallete[1]; m_Constants.Pallete_1 = new Vector4(color.r, color.g, color.b, 0);
                color = cameraAndLight.MissPallete[2]; m_Constants.Pallete_2 = new Vector4(color.r, color.g, color.b, 0);
                color = cameraAndLight.MissPallete[3]; m_Constants.Pallete_3 = new Vector4(color.r, color.g, color.b, 0);
                color = cameraAndLight.MissPallete[4]; m_Constants.Pallete_4 = new Vector4(color.r, color.g, color.b, 0);
                color = cameraAndLight.MissPallete[5]; m_Constants.Pallete_5 = new Vector4(color.r, color.g, color.b, 0);

                fixed (Constants* constantsPtr = &m_Constants)
                {
                    m_pImmediateContext.UpdateBuffer(m_ConstantsCB.Obj, 0, (uint)sizeof(Constants), new IntPtr(constantsPtr), RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
                }

                cameraAndLight.ResetLights();
            }

            //Trace rays
            {
                m_pRayTracingSRB.Obj.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_GEN, "g_ColorBuffer").Set(imageBlitter.TextureView);

                m_pImmediateContext.SetPipelineState(m_pRayTracingPSO.Obj);
                m_pImmediateContext.CommitShaderResources(m_pRayTracingSRB.Obj, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

                var Attribs = new TraceRaysAttribs();
                Attribs.DimensionX = imageBlitter.Width;
                Attribs.DimensionY = imageBlitter.Height;
                Attribs.pSBT = m_pSBT.Obj;

                m_pImmediateContext.TraceRays(Attribs);
            }

            // Blit to swapchain image
            imageBlitter.Blit();

            //Client does not need to render
            return false;
        }

        public void RequestRebind()
        {
            rebindShaderResources = true;
        }

        public void AddShaderResourceBinder(ShaderResourceBinder binder)
        {
            rebuildPipeline = true;
            shaderResourceBinders.Add(binder);
        }

        public void RemoveShaderResourceBinder(ShaderResourceBinder binder)
        {
            shaderResourceBinders.Remove(binder);
        }

        private void BindShaderResources(IShaderResourceBinding rayTracingSRB)
        {
            foreach (var i in shaderResourceBinders)
            {
                i(rayTracingSRB);
            }
        }
    }
}
