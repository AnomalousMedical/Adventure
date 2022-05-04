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
    public enum BIND_FLAGS :  Uint32
    {
        BIND_NONE = 0,
        BIND_VERTEX_BUFFER = 1 << 0,
        BIND_INDEX_BUFFER = 1 << 1,
        BIND_UNIFORM_BUFFER = 1 << 2,
        BIND_SHADER_RESOURCE = 1 << 3,
        BIND_STREAM_OUTPUT = 1 << 4,
        BIND_RENDER_TARGET = 1 << 5,
        BIND_DEPTH_STENCIL = 1 << 6,
        BIND_UNORDERED_ACCESS = 1 << 7,
        BIND_INDIRECT_DRAW_ARGS = 1 << 8,
        BIND_INPUT_ATTACHMENT = 1 << 9,
        BIND_RAY_TRACING = 1 << 10,
        BIND_SHADING_RATE = 1 << 11,
        BIND_FLAGS_LAST = BIND_SHADING_RATE,
    }
}
