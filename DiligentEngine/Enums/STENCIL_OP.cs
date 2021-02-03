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

namespace DiligentEngine
{
    public enum STENCIL_OP :  Int8
    {
        STENCIL_OP_UNDEFINED = 0,
        STENCIL_OP_KEEP = 1,
        STENCIL_OP_ZERO = 2,
        STENCIL_OP_REPLACE = 3,
        STENCIL_OP_INCR_SAT = 4,
        STENCIL_OP_DECR_SAT = 5,
        STENCIL_OP_INVERT = 6,
        STENCIL_OP_INCR_WRAP = 7,
        STENCIL_OP_DECR_WRAP = 8,
        STENCIL_OP_NUM_OPS,
    }
}
