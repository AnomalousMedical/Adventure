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
    public enum BUFFER_MODE :  Uint8
    {
        BUFFER_MODE_UNDEFINED = 0,
        BUFFER_MODE_FORMATTED,
        BUFFER_MODE_STRUCTURED,
        BUFFER_MODE_RAW,
        BUFFER_MODE_NUM_MODES,
    }
}
