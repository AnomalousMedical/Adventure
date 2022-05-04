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
    public enum RESOURCE_STATE :  Uint32
    {
        RESOURCE_STATE_UNKNOWN = 0,
        RESOURCE_STATE_UNDEFINED = 1u << 0,
        RESOURCE_STATE_VERTEX_BUFFER = 1u << 1,
        RESOURCE_STATE_CONSTANT_BUFFER = 1u << 2,
        RESOURCE_STATE_INDEX_BUFFER = 1u << 3,
        RESOURCE_STATE_RENDER_TARGET = 1u << 4,
        RESOURCE_STATE_UNORDERED_ACCESS = 1u << 5,
        RESOURCE_STATE_DEPTH_WRITE = 1u << 6,
        RESOURCE_STATE_DEPTH_READ = 1u << 7,
        RESOURCE_STATE_SHADER_RESOURCE = 1u << 8,
        RESOURCE_STATE_STREAM_OUT = 1u << 9,
        RESOURCE_STATE_INDIRECT_ARGUMENT = 1u << 10,
        RESOURCE_STATE_COPY_DEST = 1u << 11,
        RESOURCE_STATE_COPY_SOURCE = 1u << 12,
        RESOURCE_STATE_RESOLVE_DEST = 1u << 13,
        RESOURCE_STATE_RESOLVE_SOURCE = 1u << 14,
        RESOURCE_STATE_INPUT_ATTACHMENT = 1u << 15,
        RESOURCE_STATE_PRESENT = 1u << 16,
        RESOURCE_STATE_BUILD_AS_READ = 1u << 17,
        RESOURCE_STATE_BUILD_AS_WRITE = 1u << 18,
        RESOURCE_STATE_RAY_TRACING = 1u << 19,
        RESOURCE_STATE_COMMON = 1u << 20,
        RESOURCE_STATE_SHADING_RATE = 1u << 21,
        RESOURCE_STATE_MAX_BIT = RESOURCE_STATE_SHADING_RATE,
    }
}
