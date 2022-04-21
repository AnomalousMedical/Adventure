﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    public class RTCameraAndLight
    {
        public Matrix4x4 CurrentViewProj { get; private set; }

        private const int NUM_LIGHTS = 10;

        public readonly int NumLights = NUM_LIGHTS;

        private int currentLight = 0;

        public Vector4[] LightPos { get; } = new Vector4[NUM_LIGHTS];

        public Color[] LightColor { get; } = new Color[NUM_LIGHTS];

        public float[] LightLength { get; } = new float[NUM_LIGHTS];

        public int NumActiveLights => currentLight;

        public RTCameraAndLight()
        {
            LightColor[0] = new Color(1.00f, +0.8f, +0.80f);
            LightColor[1] = new Color(0.2f, 0.2f, 0.4f);

            for(int i = 0; i < NUM_LIGHTS; i++)
            {
                LightLength[i] = float.MaxValue;
            }
        }


        public Color[] MissPallete = new Color[6]
        {
            new Color(0.32f, 0.00f, 0.92f),
            new Color(0.00f, 0.22f, 0.90f),
            new Color(0.02f, 0.67f, 0.98f),
            new Color(0.41f, 0.79f, 1.00f),
            new Color(0.78f, 1.00f, 1.00f),
            new Color(1.00f, 1.00f, 1.00f),

        };

        /// <summary>
        /// Checkout a light index for this frame. Do not store the value. Returns true if you can use a light and
        /// false if none are left.
        /// </summary>
        /// <param name="lightIndex"></param>
        /// <returns></returns>
        public bool CheckoutLight(out int lightIndex)
        {
            if (currentLight < NUM_LIGHTS)
            {
                lightIndex = currentLight;
                ++currentLight;
                return true;
            }
            else
            {
                lightIndex = -1;
                return false;
            }
        }

        /// <summary>
        /// Reset the lights for the next frame.
        /// </summary>
        internal void ResetLights()
        {
            currentLight = 0;
        }

        public void GetCameraPosition(Vector3 position, Quaternion rotation, in Matrix4x4 preTransformMatrix, in Matrix4x4 CameraProj, out Vector3 CameraWorldPos, out Matrix4x4 CameraViewProj)
        {
            var CameraView = Matrix4x4.Translation(position) * rotation.toRotationMatrix4x4();

            // Apply pretransform matrix that rotates the scene according the surface orientation
            CameraView *= preTransformMatrix;

            var CameraWorld = CameraView.inverse();

            // Get projection matrix adjusted to the current screen orientation
            CameraViewProj = CameraView * CameraProj;
            this.CurrentViewProj = CameraViewProj;
            CameraWorldPos = CameraWorld.GetTranslation();
        }

        public void ExtractViewFrustumPlanesFromMatrix(in Matrix4x4 Matrix, ViewFrustum Frustum, bool bIsOpenGL)
        {
            // For more details, see Gribb G., Hartmann K., "Fast Extraction of Viewing Frustum Planes from the
            // World-View-Projection Matrix" (the paper is available at
            // http://gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf)

            // Left clipping plane
            Frustum.LeftPlane.Normal.x = Matrix.m03 + Matrix.m00;
            Frustum.LeftPlane.Normal.y = Matrix.m13 + Matrix.m10;
            Frustum.LeftPlane.Normal.z = Matrix.m23 + Matrix.m20;
            Frustum.LeftPlane.Distance = Matrix.m33 + Matrix.m30;

            // Right clipping plane
            Frustum.RightPlane.Normal.x = Matrix.m03 - Matrix.m00;
            Frustum.RightPlane.Normal.y = Matrix.m13 - Matrix.m10;
            Frustum.RightPlane.Normal.z = Matrix.m23 - Matrix.m20;
            Frustum.RightPlane.Distance = Matrix.m33 - Matrix.m30;

            // Top clipping plane
            Frustum.TopPlane.Normal.x = Matrix.m03 - Matrix.m01;
            Frustum.TopPlane.Normal.y = Matrix.m13 - Matrix.m11;
            Frustum.TopPlane.Normal.z = Matrix.m23 - Matrix.m21;
            Frustum.TopPlane.Distance = Matrix.m33 - Matrix.m31;

            // Bottom clipping plane
            Frustum.BottomPlane.Normal.x = Matrix.m03 + Matrix.m01;
            Frustum.BottomPlane.Normal.y = Matrix.m13 + Matrix.m11;
            Frustum.BottomPlane.Normal.z = Matrix.m23 + Matrix.m21;
            Frustum.BottomPlane.Distance = Matrix.m33 + Matrix.m31;

            // Near clipping plane
            if (bIsOpenGL)
            {
                // -w <= z <= w
                Frustum.NearPlane.Normal.x = Matrix.m03 + Matrix.m02;
                Frustum.NearPlane.Normal.y = Matrix.m13 + Matrix.m12;
                Frustum.NearPlane.Normal.z = Matrix.m23 + Matrix.m22;
                Frustum.NearPlane.Distance = Matrix.m33 + Matrix.m32;
            }
            else
            {
                // 0 <= z <= w
                Frustum.NearPlane.Normal.x = Matrix.m02;
                Frustum.NearPlane.Normal.y = Matrix.m12;
                Frustum.NearPlane.Normal.z = Matrix.m22;
                Frustum.NearPlane.Distance = Matrix.m32;
            }

            // Far clipping plane
            Frustum.FarPlane.Normal.x = Matrix.m03 - Matrix.m02;
            Frustum.FarPlane.Normal.y = Matrix.m13 - Matrix.m12;
            Frustum.FarPlane.Normal.z = Matrix.m23 - Matrix.m22;
            Frustum.FarPlane.Distance = Matrix.m33 - Matrix.m32;
        }
    }
}
