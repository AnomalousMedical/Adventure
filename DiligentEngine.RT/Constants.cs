﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using System.Runtime.InteropServices;
using Engine;

namespace DiligentEngine.RT
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct Constants
    {
        // Camera world position
        public float4 CameraPos;

        // Near and far clip plane distances
        public float2 ClipPlanes;
        public float2 Padding0;

        // Camera view frustum corner rays
        public float4 FrustumRayLT;
        public float4 FrustumRayLB;
        public float4 FrustumRayRT;
        public float4 FrustumRayRB;

        // The number of shadow PCF samples
        public int ShadowPCF;
        // Maximum ray recursion depth
        public uint MaxRecursion;
        public float Darkness;
        public int NumActiveLights;

        // Light properties
        public float4 AmbientColor;
        public float4 LightPos_0;
        public float4 LightPos_1;
        public float4 LightPos_2;
        public float4 LightPos_3;
        public float4 LightPos_4;
        public float4 LightPos_5;
        public float4 LightPos_6;
        public float4 LightPos_7;
        public float4 LightPos_8;
        public float4 LightPos_9;
        public float4 LightPos_10;
        public float4 LightPos_11;
        public float4 LightPos_12;
        public float4 LightPos_13;
        public float4 LightPos_14;
        public float4 LightPos_15;
        public float4 LightPos_16;
        public float4 LightPos_17;
        public float4 LightPos_18;
        public float4 LightPos_19;

        //Light color stores length of the light in w
        public float4 LightColor_0;
        public float4 LightColor_1;
        public float4 LightColor_2;
        public float4 LightColor_3;
        public float4 LightColor_4;
        public float4 LightColor_5;
        public float4 LightColor_6;
        public float4 LightColor_7;
        public float4 LightColor_8;
        public float4 LightColor_9;
        public float4 LightColor_10;
        public float4 LightColor_11;
        public float4 LightColor_12;
        public float4 LightColor_13;
        public float4 LightColor_14;
        public float4 LightColor_15;
        public float4 LightColor_16;
        public float4 LightColor_17;
        public float4 LightColor_18;
        public float4 LightColor_19;

        public float4 Pallete_0;
        public float4 Pallete_1;
        public float4 Pallete_2;
        public float4 Pallete_3;
        public float4 Pallete_4;
        public float4 Pallete_5;

        //Mip info
        public float eyeToPixelConeSpreadAngle;
        /// <summary>
        /// Bias the mip map. Negative values use larger mip maps.
        /// </summary>
        public float mipBias;
        public int missTextureSet;
        public int missTextureSet2;
        public float missTextureBlend;

        public float padding1;
        public float padding2;
        public float padding3;

        public static Constants CreateDefault(uint maxRecursionDepth)
        {
            return new Constants
            {
                ClipPlanes = new Vector2(0.1f, 150.0f),
                ShadowPCF = 1,
                MaxRecursion = Math.Min(6, maxRecursionDepth),

                AmbientColor = new Vector4(1f, 1f, 1f, 0f) * 0f,
                Darkness = 0.125f,
                LightPos_0 = new Vector4(8.00f, -8.0f, +0.00f, 0f),
                LightColor_0 = new Vector4(1.00f, +0.8f, +0.80f, float.MaxValue),
                LightPos_1 = new Vector4(0.00f, -4.0f, -5.00f, 0f),
                LightColor_1 = new Vector4(0.4f, +0.4f, +0.6f, float.MaxValue),
                NumActiveLights = 2,

                Pallete_0 = new float4(0.32f, 0.00f, 0.92f, 0f),
                Pallete_1 = new float4(0.00f, 0.22f, 0.90f, 0f),
                Pallete_2 = new float4(0.02f, 0.67f, 0.98f, 0f),
                Pallete_3 = new float4(0.41f, 0.79f, 1.00f, 0f),
                Pallete_4 = new float4(0.78f, 1.00f, 1.00f, 0f),
                Pallete_5 = new float4(1.00f, 1.00f, 1.00f, 0f),

                missTextureSet = -1,
                missTextureSet2 = -1,
                missTextureBlend = 0
            };
        }
    }
}
