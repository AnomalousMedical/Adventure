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

namespace DiligentEngine
{
    public enum TEXTURE_VIEW_TYPE :  Uint8
    {
        TEXTURE_VIEW_UNDEFINED = 0,
        TEXTURE_VIEW_SHADER_RESOURCE,
        TEXTURE_VIEW_RENDER_TARGET,
        TEXTURE_VIEW_DEPTH_STENCIL,
        TEXTURE_VIEW_UNORDERED_ACCESS,
        TEXTURE_VIEW_NUM_VIEWS,
    }
}
