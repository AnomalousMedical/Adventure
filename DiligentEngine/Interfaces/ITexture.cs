using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using Engine;

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
    public partial class ITexture :  IDeviceObject
    {
        public ITexture(IntPtr objPtr)
            : base(objPtr)
        {

        }
        public ITextureView GetDefaultView(TEXTURE_VIEW_TYPE ViewType)
        {
            return new ITextureView(ITexture_GetDefaultView(
                this.objPtr
                , ViewType
            ));
        }


        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr ITexture_GetDefaultView(
            IntPtr objPtr
            , TEXTURE_VIEW_TYPE ViewType
        );
    }
}