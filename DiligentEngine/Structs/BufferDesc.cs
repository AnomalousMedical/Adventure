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
    public partial class BufferDesc : DeviceObjectAttribs
    {
        public BufferDesc()
        {

        }
        public Uint64 Size { get; set; } = 0;
        public BIND_FLAGS BindFlags { get; set; } = BIND_FLAGS.BIND_NONE;
        public USAGE Usage { get; set; } = USAGE.USAGE_DEFAULT;
        public CPU_ACCESS_FLAGS CPUAccessFlags { get; set; } = CPU_ACCESS_FLAGS.CPU_ACCESS_NONE;
        public BUFFER_MODE Mode { get; set; } = BUFFER_MODE.BUFFER_MODE_UNDEFINED;
        public MISC_BUFFER_FLAGS MiscFlags { get; set; } = MISC_BUFFER_FLAGS.MISC_BUFFER_FLAG_NONE;
        public Uint32 ElementByteStride { get; set; } = 0;
        public Uint64 ImmediateContextMask { get; set; } = 1;


    }
}
