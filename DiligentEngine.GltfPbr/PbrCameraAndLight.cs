﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.GltfPbr
{
    public class PbrCameraAndLight : IPbrCameraAndLight, IDisposable
    {
        private AutoPtr<IBuffer> m_CameraAttribsCB;
        private AutoPtr<IBuffer> m_LightAttribsCB;
        private AutoPtr<IBuffer> m_EnvMapRenderAttribsCB;

        IDeviceContext m_pImmediateContext;

        public unsafe PbrCameraAndLight(GraphicsEngine graphicsEngine)
        {
            var m_pDevice = graphicsEngine.RenderDevice;
            m_pImmediateContext = graphicsEngine.ImmediateContext;

            {
                BufferDesc CBDesc = new BufferDesc();
                CBDesc.Name = "Camera attribs buffer";
                CBDesc.Size = (uint)sizeof(CameraAttribs);
                CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                m_CameraAttribsCB = m_pDevice.CreateBuffer(CBDesc);
            }

            {
                BufferDesc CBDesc = new BufferDesc();
                CBDesc.Name = "Light attribs buffer";
                CBDesc.Size = (uint)sizeof(LightAttribs);
                CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                m_LightAttribsCB = m_pDevice.CreateBuffer(CBDesc);
            }

            {
                BufferDesc CBDesc = new BufferDesc();
                CBDesc.Name = "Env map render attribs buffer";
                CBDesc.Size = (uint)sizeof(EnvMapRenderAttribs);
                CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;

                m_EnvMapRenderAttribsCB = m_pDevice.CreateBuffer(CBDesc);
            }

            var Barriers = new List<StateTransitionDesc>
            {
                new StateTransitionDesc{pResource = m_CameraAttribsCB.Obj,        OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                new StateTransitionDesc{pResource = m_LightAttribsCB.Obj,         OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
                new StateTransitionDesc{pResource = m_EnvMapRenderAttribsCB.Obj,  OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_CONSTANT_BUFFER, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE},
            };
            m_pImmediateContext.TransitionResourceStates(Barriers);
        }

        public void Dispose()
        {
            m_CameraAttribsCB.Dispose();
            m_LightAttribsCB.Dispose();
            m_EnvMapRenderAttribsCB.Dispose();
        }

        public IBuffer CameraAttribs => m_CameraAttribsCB.Obj;
        public IBuffer LightAttribs => m_LightAttribsCB.Obj;
        public IBuffer EnvMapRenderAttribs => m_EnvMapRenderAttribsCB.Obj;

        public Matrix4x4 CurrentViewProj { get; private set; }

        public unsafe void SetCameraMatrices(in Matrix4x4 CameraProj, in Matrix4x4 CameraViewProj, in Vector3 CameraWorldPos)
        {
            this.CurrentViewProj = CameraViewProj;

            IntPtr data = m_pImmediateContext.MapBuffer(m_CameraAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);

            var CamAttribs = (CameraAttribs*)data.ToPointer();
            CamAttribs->mProjT = CameraProj.Transpose();
            CamAttribs->mViewProjT = CameraViewProj.Transpose();
            CamAttribs->mViewProjInvT = CameraViewProj.inverse().Transpose();
            CamAttribs->f4Position = new Vector4(CameraWorldPos.x, CameraWorldPos.y, CameraWorldPos.z, 1);

            m_pImmediateContext.UnmapBuffer(m_CameraAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
        }

        public unsafe void SetLight(in Vector3 direction, in Vector4 lightColor, float intensity)
        {
            IntPtr data = m_pImmediateContext.MapBuffer(m_LightAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);

            //Looks like only direction and intensity matter here, setting more did not help
            var lightAttribs = (LightAttribs*)data.ToPointer();
            lightAttribs->f4Direction = direction.ToVector4();
            lightAttribs->f4Intensity = lightColor * intensity;

            m_pImmediateContext.UnmapBuffer(m_LightAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
        }

        public unsafe void SetLightAndShadow(in Vector3 direction, in Vector4 lightColor, float intensity, in Matrix4x4 WorldToShadowMapUVDepth)
        {
            IntPtr data = m_pImmediateContext.MapBuffer(m_LightAttribsCB.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);

            var lightAttribs = (LightAttribs*)data.ToPointer();
            lightAttribs->f4Direction = direction.ToVector4();
            lightAttribs->f4Intensity = lightColor * intensity;
            var shadowAttribs = &lightAttribs->ShadowAttribs;
            shadowAttribs->mWorldToShadowMapUVDepthT_0 = WorldToShadowMapUVDepth.Transpose();

            m_pImmediateContext.UnmapBuffer(m_LightAttribsCB.Obj, MAP_TYPE.MAP_WRITE);
        }

        public void SetCameraPosition(Vector3 position, Quaternion rotation, in Matrix4x4 preTransformMatrix, in Matrix4x4 CameraProj)
        {
            //For some reason camera defined backward, so take -position
            var CameraView = Matrix4x4.Translation(-position) * rotation.toRotationMatrix4x4();

            // Apply pretransform matrix that rotates the scene according the surface orientation
            CameraView *= preTransformMatrix;

            var CameraWorld = CameraView.inverse();

            // Get projection matrix adjusted to the current screen orientation
            var CameraViewProj = CameraView * CameraProj;
            var CameraWorldPos = CameraWorld.GetTranslation();

            this.SetCameraMatrices(CameraProj, CameraViewProj, CameraWorldPos);
        }
    }
}
