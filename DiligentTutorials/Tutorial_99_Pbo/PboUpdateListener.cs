﻿using Anomalous.OSPlatform;
using DiligentEngine;
using Engine;
using Engine.Platform;
using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


using Uint8 = System.Byte;
using Int8 = System.SByte;
using Bool = System.Boolean;
using Uint32 = System.UInt32;
using Uint64 = System.UInt64;
using Float32 = System.Single;
using Uint16 = System.UInt16;
using float3 = Engine.Vector3;
using float4 = Engine.Vector4;
using float4x4 = Engine.Matrix4x4;
using System.IO;
using Tutorial_99_Pbo.Shapes;
using DiligentEngine.GltfPbr;

namespace Tutorial_99_Pbo
{
    class PboUpdateListener : UpdateListener, IDisposable
    {
        private readonly NativeOSWindow window;
        private readonly Cube shape;
        private readonly TextureLoader textureLoader;
        private readonly CC0TextureLoader cc0TextureLoader;
        private ISwapChain m_pSwapChain;
        private IRenderDevice m_pDevice;
        private IDeviceContext m_pImmediateContext;
        private PbrRenderAttribs pbrRenderAttribs = PbrRenderAttribs.CreateDefault();

        private PbrRenderer m_GLTFRenderer;
        private AutoPtr<IBuffer> m_CameraAttribsCB;
        private AutoPtr<IBuffer> m_LightAttribsCB;
        private AutoPtr<IBuffer> m_EnvMapRenderAttribsCB;
        private AutoPtr<ITextureView> m_EnvironmentMapSRV;
        private AutoPtr<IShaderResourceBinding> pboMatBinding;

        public unsafe PboUpdateListener(
            GraphicsEngine graphicsEngine, 
            NativeOSWindow window, 
            PbrRenderer m_GLTFRenderer, 
            Cube shape, 
            TextureLoader textureLoader, 
            CC0TextureLoader cc0TextureLoader)
        {
            this.m_GLTFRenderer = m_GLTFRenderer;
            this.m_pSwapChain = graphicsEngine.SwapChain;
            this.m_pDevice = graphicsEngine.RenderDevice;
            this.m_pImmediateContext = graphicsEngine.ImmediateContext;
            this.window = window;
            this.shape = shape;
            this.textureLoader = textureLoader;
            this.cc0TextureLoader = cc0TextureLoader;
            Initialize();
        }

        public void Dispose()
        {
            pboMatBinding.Dispose();
            m_CameraAttribsCB.Dispose();
            m_LightAttribsCB.Dispose();
            m_EnvMapRenderAttribsCB.Dispose();
            m_EnvironmentMapSRV.Dispose();
        }

        unsafe void Initialize()
        {
            AutoPtr<ITexture> EnvironmentMap = null;
            try
            {
                //var bytes = File.ReadAllBytes("textures/papermill.ktx");
                ////Create environment map texture
                //var loadInfo = new TextureLoadInfo()
                //{
                //    Name = "Environment map",
                //};
                //fixed (byte* data = bytes)
                //{
                //    EnvironmentMap = KtxLoader.CreateTextureFromKTX(new IntPtr(data), new UIntPtr((uint)bytes.Length), loadInfo, m_pDevice);
                //}

                //Create env map manually, need to make this something actually cool.
                {
                    var TexDesc = new TextureDesc();
                    TexDesc.Type = RESOURCE_DIMENSION.RESOURCE_DIM_TEX_CUBE;
                    TexDesc.Usage = USAGE.USAGE_IMMUTABLE;
                    TexDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
                    TexDesc.Width = 512;
                    TexDesc.Height = 512;
                    TexDesc.Depth = 6;
                    TexDesc.Format = TEXTURE_FORMAT.TEX_FORMAT_BGRA8_UNORM;
                    TexDesc.MipLevels = 1;

                    var size = TexDesc.Width * TexDesc.Height;
                    var envMapArray = stackalloc uint[(int)size];
                    var span = new Span<uint>(envMapArray, (int)size);
                    span.Fill(0xff323232);

                    var Level0Data = new TextureSubResData { pData = new IntPtr(envMapArray), Stride = TexDesc.Width * 4 };
                    var InitData = new TextureData { pSubResources = new List<TextureSubResData> { Level0Data, Level0Data, Level0Data, Level0Data, Level0Data, Level0Data } };

                    EnvironmentMap = m_pDevice.CreateTexture(TexDesc, InitData);

                    m_EnvironmentMapSRV = new AutoPtr<ITextureView>(EnvironmentMap.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE));
                }

                unsafe
                {
                    BufferDesc CBDesc = new BufferDesc();
                    CBDesc.Name = "Camera attribs buffer";
                    CBDesc.uiSizeInBytes = (uint)sizeof(CameraAttribs);
                    CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                    CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                    CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                    m_CameraAttribsCB = m_pDevice.CreateBuffer(CBDesc);
                }

                {
                    BufferDesc CBDesc = new BufferDesc();
                    CBDesc.Name = "Light attribs buffer";
                    CBDesc.uiSizeInBytes = (uint)sizeof(LightAttribs);
                    CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                    CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                    CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                    m_LightAttribsCB = m_pDevice.CreateBuffer(CBDesc);
                }

                {
                    BufferDesc CBDesc = new BufferDesc();
                    CBDesc.Name = "Env map render attribs buffer";
                    CBDesc.uiSizeInBytes = (uint)sizeof(EnvMapRenderAttribs);
                    CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                    CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                    CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                    m_EnvMapRenderAttribsCB = m_pDevice.CreateBuffer(CBDesc);
                }

                var Barriers = new List<StateTransitionDesc>
                {
                    new StateTransitionDesc{pResource = m_CameraAttribsCB.Obj,        OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, UpdateResourceState = true},
                    new StateTransitionDesc{pResource = m_LightAttribsCB.Obj,         OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, UpdateResourceState = true},
                    new StateTransitionDesc{pResource = m_EnvMapRenderAttribsCB.Obj,  OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, UpdateResourceState = true},
                    new StateTransitionDesc{pResource = EnvironmentMap.Obj,           OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, UpdateResourceState = true}
                };
                m_pImmediateContext.TransitionResourceStates(Barriers);

                m_GLTFRenderer.PrecomputeCubemaps(m_pDevice, m_pImmediateContext, m_EnvironmentMapSRV.Obj);

                //Spheres
                //using (var baseColorStream = File.Open("textures/Spheres_BaseColor.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                //using (var physicalDescriptorStream = File.Open("textures/Spheres_MetalRough.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                //{
                //    using var baseColorMap = textureLoader.LoadTexture(baseColorStream, "baseColorMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D_ARRAY, false);
                //    using var physicalDescriptorMap = textureLoader.LoadTexture(physicalDescriptorStream, "physicalDescriptorMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D_ARRAY, false);

                //    pboMatBinding = m_GLTFRenderer.CreateMaterialSRB(
                //        pCameraAttribs: m_CameraAttribsCB.Obj,
                //        pLightAttribs: m_LightAttribsCB.Obj,
                //        baseColorMap: baseColorMap.Obj,
                //        physicalDescriptorMap: physicalDescriptorMap.Obj
                //    );
                //}


                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Wood049_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Chainmail001_1K");
                using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Chainmail004_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/SheetMetal001_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/SheetMetal002_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/SheetMetal004_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/ChristmasTreeOrnament007_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Carpet008_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Fabric026_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Fabric020_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Metal032_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Leather026_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Leather011_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Ground037_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Chip005_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Pipe002_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/AcousticFoam003_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/AsphaltDamage001_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Rope001_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/SolarPanel003_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Ice004_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Snow006_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Snow005_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/Snow004_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/RoofingTiles003_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/RoofingTiles006_1K");
                //using var ccoTextures = cc0TextureLoader.LoadTextureSet("cc0Textures/ManholeCover004_1K");
                pboMatBinding = m_GLTFRenderer.CreateMaterialSRB(
                    pCameraAttribs: m_CameraAttribsCB.Obj,
                    pLightAttribs: m_LightAttribsCB.Obj,
                    baseColorMap: ccoTextures.BaseColorMap,
                    normalMap: ccoTextures.NormalMap,
                    physicalDescriptorMap: ccoTextures.PhysicalDescriptorMap,
                    aoMap: ccoTextures.AmbientOcclusionMap
                );
            }
            finally
            {
                EnvironmentMap?.Dispose();
            }
        }

        public void exceededMaxDelta()
        {

        }

        public void loopStarting()
        {

        }

        static readonly float PI_F = (float)Math.PI;

        Matrix4x4 GetAdjustedProjectionMatrix(float FOV, float NearPlane, float FarPlane, float Width, float Height, SURFACE_TRANSFORM PreTransform = SURFACE_TRANSFORM.SURFACE_TRANSFORM_IDENTITY)
        {
            if (Height == 0.0f)
            {
                Height = 1.0f;
            }

            float AspectRatio = Width / Height;
            float XScale, YScale;
            if (PreTransform == SURFACE_TRANSFORM.SURFACE_TRANSFORM_ROTATE_90 ||
                PreTransform == SURFACE_TRANSFORM.SURFACE_TRANSFORM_ROTATE_270 ||
                PreTransform == SURFACE_TRANSFORM.SURFACE_TRANSFORM_HORIZONTAL_MIRROR_ROTATE_90 ||
                PreTransform == SURFACE_TRANSFORM.SURFACE_TRANSFORM_HORIZONTAL_MIRROR_ROTATE_270)
            {
                // When the screen is rotated, vertical FOV becomes horizontal FOV
                XScale = 1f / (float)Math.Tan(FOV / 2f);
                // Aspect ratio is inversed
                YScale = XScale * AspectRatio;
            }
            else
            {
                YScale = 1f / (float)Math.Tan(FOV / 2f);
                XScale = YScale / AspectRatio;
            }

            Matrix4x4 Proj = new Matrix4x4();
            Proj.m00 = XScale;
            Proj.m11 = YScale;
            Proj.SetNearFarClipPlanes(NearPlane, FarPlane, false);// genericEngineFactory.RenderDevice.GetDeviceCaps().IsGLDevice());
            return Proj;
        }

        public unsafe void sendUpdate(Clock clock)
        {
            var pRTV = m_pSwapChain.GetCurrentBackBufferRTV();
            var pDSV = m_pSwapChain.GetDepthBufferDSV();
            m_pImmediateContext.SetRenderTarget(pRTV, pDSV, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            // Clear the back buffer
            var ClearColor = new Engine.Color(0.032f, 0.032f, 0.032f, 1.0f);
            m_pImmediateContext.ClearRenderTarget(pRTV, ClearColor, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            m_pImmediateContext.ClearDepthStencil(pDSV, CLEAR_DEPTH_STENCIL_FLAGS.CLEAR_DEPTH_FLAG, 1f, 0, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            float YFov = PI_F / 4.0f;
            float ZNear = 0.1f;
            float ZFar = 100f;

            float4x4 CameraView = float4x4.Identity * float4x4.Translation(0.0f, 0.0f, 5.0f);

            // Apply pretransform matrix that rotates the scene according the surface orientation
            //Skip for now
            //CameraView *= GetSurfacePretransformMatrix(float3{0, 0, 1});

            float4x4 CameraWorld = CameraView.inverse();

            // Get projection matrix adjusted to the current screen orientation
            var CameraProj = GetAdjustedProjectionMatrix(YFov, ZNear, ZFar, window.WindowWidth, window.WindowHeight);
            var CameraViewProj = CameraView * CameraProj;

            var CameraWorldPos = CameraWorld.GetTranslation();

            {
                IntPtr data = m_pImmediateContext.MapBuffer(m_CameraAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);

                var CamAttribs = (CameraAttribs*)data.ToPointer();
                CamAttribs->mProjT = CameraProj.Transpose();
                CamAttribs->mViewProjT = CameraViewProj.Transpose();
                CamAttribs->mViewProjInvT = CameraViewProj.inverse().Transpose();
                CamAttribs->f4Position = new Vector4(CameraWorldPos.x, CameraWorldPos.y, CameraWorldPos.z, 1);

                m_pImmediateContext.UnmapBuffer(m_CameraAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
            }

            {
                float3 m_LightDirection = Vector3.Forward;// (new float3(0.5f, -0.6f, -0.2f)).normalized();
                float4 m_LightColor = new float4(1, 1, 1, 1);
                float m_LightIntensity = 3.0f;

                IntPtr data = m_pImmediateContext.MapBuffer(m_LightAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);

                //Looks like only direction and intensity matter here, setting more did not help
                var lightAttribs = (LightAttribs*)data.ToPointer();
                lightAttribs->f4Direction = m_LightDirection.ToVector4();
                lightAttribs->f4Intensity = m_LightColor * m_LightIntensity;

                m_pImmediateContext.UnmapBuffer(m_LightAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
            }

            var trans = Vector3.Zero;
            //var rot = Quaternion.Identity;
            var rotAmount = (clock.CurrentTimeMicro * Clock.MicroToSeconds) / 2 % (2 * (float)Math.PI);
            var rot = new Quaternion(rotAmount, 0f, 0f);

            var CubeModelTransform = rot.toRotationMatrix4x4(trans);

            m_GLTFRenderer.Begin(m_pImmediateContext);
            m_GLTFRenderer.Render(m_pImmediateContext, pboMatBinding.Obj, shape.VertexBuffer, shape.SkinVertexBuffer, shape.IndexBuffer, shape.NumIndices, PbrAlphaMode.ALPHA_MODE_OPAQUE, ref CubeModelTransform, pbrRenderAttribs);

            this.m_pSwapChain.Present(1);
        }
    }
}
