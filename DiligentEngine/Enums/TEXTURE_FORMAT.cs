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
    public enum TEXTURE_FORMAT :  Uint16
    {
        TEX_FORMAT_UNKNOWN = 0,
        TEX_FORMAT_RGBA32_TYPELESS,
        TEX_FORMAT_RGBA32_FLOAT,
        TEX_FORMAT_RGBA32_UINT,
        TEX_FORMAT_RGBA32_SINT,
        TEX_FORMAT_RGB32_TYPELESS,
        TEX_FORMAT_RGB32_FLOAT,
        TEX_FORMAT_RGB32_UINT,
        TEX_FORMAT_RGB32_SINT,
        TEX_FORMAT_RGBA16_TYPELESS,
        TEX_FORMAT_RGBA16_FLOAT,
        TEX_FORMAT_RGBA16_UNORM,
        TEX_FORMAT_RGBA16_UINT,
        TEX_FORMAT_RGBA16_SNORM,
        TEX_FORMAT_RGBA16_SINT,
        TEX_FORMAT_RG32_TYPELESS,
        TEX_FORMAT_RG32_FLOAT,
        TEX_FORMAT_RG32_UINT,
        TEX_FORMAT_RG32_SINT,
        TEX_FORMAT_R32G8X24_TYPELESS,
        TEX_FORMAT_D32_FLOAT_S8X24_UINT,
        TEX_FORMAT_R32_FLOAT_X8X24_TYPELESS,
        TEX_FORMAT_X32_TYPELESS_G8X24_UINT,
        TEX_FORMAT_RGB10A2_TYPELESS,
        TEX_FORMAT_RGB10A2_UNORM,
        TEX_FORMAT_RGB10A2_UINT,
        TEX_FORMAT_R11G11B10_FLOAT,
        TEX_FORMAT_RGBA8_TYPELESS,
        TEX_FORMAT_RGBA8_UNORM,
        TEX_FORMAT_RGBA8_UNORM_SRGB,
        TEX_FORMAT_RGBA8_UINT,
        TEX_FORMAT_RGBA8_SNORM,
        TEX_FORMAT_RGBA8_SINT,
        TEX_FORMAT_RG16_TYPELESS,
        TEX_FORMAT_RG16_FLOAT,
        TEX_FORMAT_RG16_UNORM,
        TEX_FORMAT_RG16_UINT,
        TEX_FORMAT_RG16_SNORM,
        TEX_FORMAT_RG16_SINT,
        TEX_FORMAT_R32_TYPELESS,
        TEX_FORMAT_D32_FLOAT,
        TEX_FORMAT_R32_FLOAT,
        TEX_FORMAT_R32_UINT,
        TEX_FORMAT_R32_SINT,
        TEX_FORMAT_R24G8_TYPELESS,
        TEX_FORMAT_D24_UNORM_S8_UINT,
        TEX_FORMAT_R24_UNORM_X8_TYPELESS,
        TEX_FORMAT_X24_TYPELESS_G8_UINT,
        TEX_FORMAT_RG8_TYPELESS,
        TEX_FORMAT_RG8_UNORM,
        TEX_FORMAT_RG8_UINT,
        TEX_FORMAT_RG8_SNORM,
        TEX_FORMAT_RG8_SINT,
        TEX_FORMAT_R16_TYPELESS,
        TEX_FORMAT_R16_FLOAT,
        TEX_FORMAT_D16_UNORM,
        TEX_FORMAT_R16_UNORM,
        TEX_FORMAT_R16_UINT,
        TEX_FORMAT_R16_SNORM,
        TEX_FORMAT_R16_SINT,
        TEX_FORMAT_R8_TYPELESS,
        TEX_FORMAT_R8_UNORM,
        TEX_FORMAT_R8_UINT,
        TEX_FORMAT_R8_SNORM,
        TEX_FORMAT_R8_SINT,
        TEX_FORMAT_A8_UNORM,
        TEX_FORMAT_R1_UNORM,
        TEX_FORMAT_RGB9E5_SHAREDEXP,
        TEX_FORMAT_RG8_B8G8_UNORM,
        TEX_FORMAT_G8R8_G8B8_UNORM,
        TEX_FORMAT_BC1_TYPELESS,
        TEX_FORMAT_BC1_UNORM,
        TEX_FORMAT_BC1_UNORM_SRGB,
        TEX_FORMAT_BC2_TYPELESS,
        TEX_FORMAT_BC2_UNORM,
        TEX_FORMAT_BC2_UNORM_SRGB,
        TEX_FORMAT_BC3_TYPELESS,
        TEX_FORMAT_BC3_UNORM,
        TEX_FORMAT_BC3_UNORM_SRGB,
        TEX_FORMAT_BC4_TYPELESS,
        TEX_FORMAT_BC4_UNORM,
        TEX_FORMAT_BC4_SNORM,
        TEX_FORMAT_BC5_TYPELESS,
        TEX_FORMAT_BC5_UNORM,
        TEX_FORMAT_BC5_SNORM,
        TEX_FORMAT_B5G6R5_UNORM,
        TEX_FORMAT_B5G5R5A1_UNORM,
        TEX_FORMAT_BGRA8_UNORM,
        TEX_FORMAT_BGRX8_UNORM,
        TEX_FORMAT_R10G10B10_XR_BIAS_A2_UNORM,
        TEX_FORMAT_BGRA8_TYPELESS,
        TEX_FORMAT_BGRA8_UNORM_SRGB,
        TEX_FORMAT_BGRX8_TYPELESS,
        TEX_FORMAT_BGRX8_UNORM_SRGB,
        TEX_FORMAT_BC6H_TYPELESS,
        TEX_FORMAT_BC6H_UF16,
        TEX_FORMAT_BC6H_SF16,
        TEX_FORMAT_BC7_TYPELESS,
        TEX_FORMAT_BC7_UNORM,
        TEX_FORMAT_BC7_UNORM_SRGB,
        TEX_FORMAT_NUM_FORMATS,
    }
}
