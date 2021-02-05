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
    public partial class TextureSubResData
    {

        public TextureSubResData()
        {
            
        }
        public IntPtr pData { get; set; }
        public IBuffer pSrcBuffer { get; set; }
        public Uint32 SrcOffset { get; set; } = 0;
        public Uint32 Stride { get; set; } = 0;
        public Uint32 DepthStride { get; set; } = 0;


    }
}