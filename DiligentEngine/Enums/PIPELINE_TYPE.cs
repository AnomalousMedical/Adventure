using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

namespace DiligentEngine
{
    public enum PIPELINE_TYPE :  Uint8
    {
        PIPELINE_TYPE_GRAPHICS,
        PIPELINE_TYPE_COMPUTE,
        PIPELINE_TYPE_MESH,
        PIPELINE_TYPE_RAY_TRACING,
        PIPELINE_TYPE_TILE,
        PIPELINE_TYPE_LAST = PIPELINE_TYPE_TILE,
        PIPELINE_TYPE_COUNT,
        PIPELINE_TYPE_INVALID = 0xFF,
    }
}
