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
using float4 = Engine.Vector4;
using float3 = Engine.Vector3;
using float2 = Engine.Vector2;
using float4x4 = Engine.Matrix4x4;
using BOOL = System.Boolean;

namespace DiligentEngine
{
    /// <summary>
    /// Texture inteface
    /// </summary>
    public partial class ITexture :  IDeviceObject
    {
        public Uint32 GetDesc_MipLevels => ITexture_GetDesc_MipLevels(objPtr);


        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Uint32 ITexture_GetDesc_MipLevels(
            IntPtr objPtr
        );
        public Uint32 GetDesc_Width => ITexture_GetDesc_Width(objPtr);


        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Uint32 ITexture_GetDesc_Width(
            IntPtr objPtr
        );
        public Uint32 GetDesc_Height => ITexture_GetDesc_Height(objPtr);


        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Uint32 ITexture_GetDesc_Height(
            IntPtr objPtr
        );
        public RESOURCE_DIMENSION GetDesc_Type => ITexture_GetDesc_Type(objPtr);


        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern RESOURCE_DIMENSION ITexture_GetDesc_Type(
            IntPtr objPtr
        );
    }
}
