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
using float4x4 = Engine.Matrix4x4;
using BOOL = System.Boolean;

namespace DiligentEngine
{
    public partial class StencilOpDesc
    {

        public StencilOpDesc()
        {
            
        }
        public STENCIL_OP StencilFailOp { get; set; } = STENCIL_OP.STENCIL_OP_KEEP;
        public STENCIL_OP StencilDepthFailOp { get; set; } = STENCIL_OP.STENCIL_OP_KEEP;
        public STENCIL_OP StencilPassOp { get; set; } = STENCIL_OP.STENCIL_OP_KEEP;
        public COMPARISON_FUNCTION StencilFunc { get; set; } = COMPARISON_FUNCTION.COMPARISON_FUNC_ALWAYS;


    }
}
