﻿using System;
using System.Collections.Generic;
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
using FreeImageAPI;
using System.IO;
using System.Runtime.InteropServices;

namespace DiligentEngine
{
    public class TextureLoader
    {
        private readonly GraphicsEngine graphicsEngine;

        public TextureLoader(GraphicsEngine graphicsEngine)
        {
            this.graphicsEngine = graphicsEngine;
        }

        TEXTURE_FORMAT GetFormat(FreeImageBitmap bitmap, bool isSRGB)
        {
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return isSRGB ? TEXTURE_FORMAT.TEX_FORMAT_BGRA8_UNORM_SRGB : TEXTURE_FORMAT.TEX_FORMAT_BGRA8_UNORM;

                default:
                    return TEXTURE_FORMAT.TEX_FORMAT_UNKNOWN;
            }
        }

        public AutoPtr<ITexture> LoadTexture(Stream stream, String name, RESOURCE_DIMENSION resouceDimension, bool isSRGB)
        {
            using (var bmp = FreeImageBitmap.FromStream(stream))
            {
                return CreateTextureFromImage(bmp, 0, name, resouceDimension, isSRGB);
            }
        }

        /// <summary>
        /// This function loads stuff, but its really incomplete.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="MipLevels"></param>
        /// <param name="pDevice"></param>
        /// <returns></returns>
        public AutoPtr<ITexture> CreateTextureFromImage(FreeImageBitmap bitmap, int MipLevels, String name, RESOURCE_DIMENSION resouceDimension, bool isSRGB)
        {
            var mips = new List<FreeImageBitmap>(MipLevels);

            try
            {
                FixBitmap(bitmap);

                uint width = (uint)bitmap.Width;
                uint height = (uint)bitmap.Height;

                TextureDesc TexDesc = new TextureDesc();
                TexDesc.Name = name;
                TexDesc.Type = resouceDimension;
                TexDesc.Width = width;
                TexDesc.Height = height;
                TexDesc.MipLevels = ComputeMipLevelsCount(TexDesc.Width, TexDesc.Height);
                if (MipLevels > 0)
                {
                    TexDesc.MipLevels = (uint)Math.Min(TexDesc.MipLevels, MipLevels);
                }
                TexDesc.Usage = USAGE.USAGE_IMMUTABLE;
                TexDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
                TexDesc.Format = GetFormat(bitmap, isSRGB);
                TexDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_NONE;

                var pSubResources = new List<TextureSubResData>(MipLevels);

                AddBitmapToResources(bitmap, pSubResources);

                //Mip maps
                var MipWidth = TexDesc.Width;
                var MipHeight = TexDesc.Height;
                for (Uint32 m = 1; m < TexDesc.MipLevels; ++m)
                {
                    var CoarseMipWidth = Math.Max(MipWidth / 2u, 1u);
                    var CoarseMipHeight = Math.Max(MipHeight / 2u, 1u);
                    var mip = bitmap.Copy(0, 0, bitmap.Width, bitmap.Height);
                    mip.Rescale(new Size((int)CoarseMipWidth, (int)CoarseMipHeight), FREE_IMAGE_FILTER.FILTER_BILINEAR);
                    mips.Add(mip);
                    AddBitmapToResources(mip, pSubResources);

                    MipWidth = CoarseMipWidth;
                    MipHeight = CoarseMipHeight;
                }

                TextureData TexData = new TextureData();
                TexData.pSubResources = pSubResources;

                return graphicsEngine.RenderDevice.CreateTexture(TexDesc, TexData); //This does not do anything with this pointer, just pass it along and let the caller handle it
            }
            finally
            {
                foreach(var mip in mips)
                {
                    mip.Dispose();
                }
            }
        }

        public AutoPtr<ITexture> CreateTextureFromFloatSpan(Span<float> floats, String name, RESOURCE_DIMENSION resouceDimension, uint width, uint height)
        {
            TextureDesc TexDesc = new TextureDesc();
            TexDesc.Name = name;
            TexDesc.Type = resouceDimension;
            TexDesc.Width = width;
            TexDesc.Height = height;
            TexDesc.MipLevels = 1;
            TexDesc.Usage = USAGE.USAGE_IMMUTABLE;
            TexDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
            TexDesc.Format = TEXTURE_FORMAT.TEX_FORMAT_R32_FLOAT;
            TexDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_NONE;

            var pSubResources = new List<TextureSubResData>(1);

            unsafe
            {
                fixed (float* texData = floats)
                {
                    pSubResources.Add(new TextureSubResData()
                    {
                        pData = new IntPtr(texData),
                        Stride = sizeof(float) * width,
                    });

                    TextureData TexData = new TextureData();
                    TexData.pSubResources = pSubResources;

                    return graphicsEngine.RenderDevice.CreateTexture(TexDesc, TexData); //This does not do anything with this pointer, just pass it along and let the caller handle it
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HalfRgTexturePixel
        {
            public Half r;
            public Half g;
        };

        public unsafe AutoPtr<ITexture> CreateTextureFromFloatSpan(Span<HalfRgTexturePixel> pixels, String name, RESOURCE_DIMENSION resouceDimension, uint width, uint height)
        {
            TextureDesc TexDesc = new TextureDesc();
            TexDesc.Name = name;
            TexDesc.Type = resouceDimension;
            TexDesc.Width = width;
            TexDesc.Height = height;
            TexDesc.MipLevels = 1;
            TexDesc.Usage = USAGE.USAGE_IMMUTABLE;
            TexDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
            TexDesc.Format = TEXTURE_FORMAT.TEX_FORMAT_RG16_FLOAT;
            TexDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_NONE;

            var pSubResources = new List<TextureSubResData>(1);

            unsafe
            {
                fixed (HalfRgTexturePixel* texData = pixels)
                {
                    pSubResources.Add(new TextureSubResData()
                    {
                        pData = new IntPtr(texData),
                        Stride = (ulong)sizeof(HalfRgTexturePixel) * width,
                    });

                    TextureData TexData = new TextureData();
                    TexData.pSubResources = pSubResources;

                    return graphicsEngine.RenderDevice.CreateTexture(TexDesc, TexData); //This does not do anything with this pointer, just pass it along and let the caller handle it
                }
            }
        }

        private static void AddBitmapToResources(FreeImageBitmap bitmap, List<TextureSubResData> pSubResources)
        {
            if (bitmap.Stride > 0)
            {
                pSubResources.Add(new TextureSubResData()
                {
                    pData = bitmap.Scan0,
                    Stride = (Uint32)(bitmap.Stride),
                });
            }
            else
            {
                //Freeimage scan0 gives the last line for some reason, this gives the first to allow the negative scan to become positive
                var stride = -bitmap.Stride;
                pSubResources.Add(new TextureSubResData()
                {
                    pData = bitmap.Scan0 - (stride * (bitmap.Height - 1)),
                    Stride = (Uint32)stride,
                });
            }
        }

        /// <summary>
        /// Do the changes required to be able to load the bitmap as a texture.
        /// </summary>
        /// <param name="bitmap"></param>
        public void FixBitmap(FreeImageBitmap bitmap)
        {
            //THIS SUCKS - Rotating in memory, but only way for now, need to figure out how to read backward.
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                bitmap.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
            }
        }

        Uint32 ComputeMipLevelsCount(Uint32 Width, Uint32 Height)
        {
            return ComputeMipLevelsCount(Math.Max(Width, Height));
        }

        Uint32 ComputeMipLevelsCount(Uint32 Width)
        {
            if (Width == 0)
                return 0;

            int MipLevels = 0; //Was Uint32, but c# cannot do that
            while ((Width >> MipLevels) > 0)
            {
                ++MipLevels;
            }
            //VERIFY(Width >= (1U << (MipLevels - 1)) && Width < (1U << MipLevels), "Incorrect number of Mip levels");
            return (Uint32)MipLevels;
        }
    }
}
