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
    public partial class BottomLevelASDesc : DeviceObjectAttribs
    {
        public BottomLevelASDesc()
        {

        }
        public List<BLASTriangleDesc> pTriangles { get; set; }
        public List<BLASBoundingBoxDesc> pBoxes { get; set; }
        public RAYTRACING_BUILD_AS_FLAGS Flags { get; set; } = RAYTRACING_BUILD_AS_FLAGS.RAYTRACING_BUILD_AS_NONE;
        public Uint64 CompactedSize { get; set; } = 0;
        public Uint64 ImmediateContextMask { get; set; } = 1;


    }
}
