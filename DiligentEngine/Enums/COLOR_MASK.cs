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
    public enum COLOR_MASK :  Uint8
    {
        COLOR_MASK_NONE = 0,
        COLOR_MASK_RED = 1 << 0,
        COLOR_MASK_GREEN = 1 << 1,
        COLOR_MASK_BLUE = 1 << 2,
        COLOR_MASK_ALPHA = 1 << 3,
        COLOR_MASK_ALL = (((COLOR_MASK_RED | COLOR_MASK_GREEN) | COLOR_MASK_BLUE) | COLOR_MASK_ALPHA),
    }
}
