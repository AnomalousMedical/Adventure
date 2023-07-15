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
    [StructLayout(LayoutKind.Sequential)]
    public struct CubeAttribVertex
    {
        public float2 uv;
        public float2 globalUv;
        public float4 tangent;
        public float4 binormal;
        public float4 normal;
        public float tex;
        public float tex2;
        public int pad2;
        public int pad3;
    };
}
