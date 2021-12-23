﻿using DiligentEngine;
using Engine;
using Engine.Resources;
using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Resources
{
    /// <summary>
    /// This loader can load the textures from https://cc0textures.com. It will reformat for the rt renderer.
    /// </summary>
    public class CC0TextureLoader
    {
        public const int DefaultNormal = 0x00FF7F7F;
        public const int DefaultPhysical = 0x0000FF00;

        private readonly TextureLoader textureLoader;
        private readonly IResourceProvider<CC0TextureLoader> resourceProvider;
        private readonly GraphicsEngine graphicsEngine;

        public CC0TextureLoader(TextureLoader textureLoader, IResourceProvider<CC0TextureLoader> resourceProvider, GraphicsEngine graphicsEngine)
        {
            this.textureLoader = textureLoader;
            this.resourceProvider = resourceProvider;
            this.graphicsEngine = graphicsEngine;
        }

        public async Task<CC0TextureResult> LoadTextureSet(String basePath, String ext = "jpg", string colorPath = null, string colorExt = null)
        {
            //In this function the auto pointers are handed off to the result, which will be managed by the caller to erase the resources.
            var result = new CC0TextureResult();

            var colorMapPath = $"cc0Textures/{colorPath ?? basePath}_1K_Color.{colorExt ?? ext}";
            var normalMapPath = $"cc0Textures/{basePath}_1K_Normal.{ext}";
            var roughnessMapPath = $"cc0Textures/{basePath}_1K_Roughness.{ext}";
            var metalnessMapPath = $"cc0Textures/{basePath}_1K_Metalness.{ext}";
            var ambientOcclusionMapPath = $"cc0Textures/{basePath}_1K_AmbientOcclusion.{ext}";

            var Barriers = new List<StateTransitionDesc>(5);

            await Task.Run(() =>
            {
                if (resourceProvider.fileExists(colorMapPath))
                {
                    using (var stream = resourceProvider.openFile(colorMapPath))
                    {
                        var baseColorMap = textureLoader.LoadTexture(stream, "baseColorMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, true);
                        result.SetBaseColorMap(baseColorMap);
                        Barriers.Add(new StateTransitionDesc { pResource = baseColorMap.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, UpdateResourceState = true });
                    }
                }

                if (resourceProvider.fileExists(normalMapPath))
                {
                    using (var stream = resourceProvider.openFile(normalMapPath))
                    {
                        using var map = FreeImageBitmap.FromStream(stream);

                        var normalMap = textureLoader.CreateTextureFromImage(map, 0, "normalTexture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        result.SetNormalMap(normalMap);
                        Barriers.Add(new StateTransitionDesc { pResource = normalMap.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, UpdateResourceState = true });
                    }
                }

                {
                    FreeImageBitmap roughnessBmp = null;
                    FreeImageBitmap metalnessBmp = null;
                    try
                    {
                        if (resourceProvider.fileExists(roughnessMapPath))
                        {
                            using var stream = resourceProvider.openFile(roughnessMapPath);
                            roughnessBmp = FreeImageBitmap.FromStream(stream);
                        }

                        if (resourceProvider.fileExists(metalnessMapPath))
                        {
                            using var stream = resourceProvider.openFile(metalnessMapPath);
                            metalnessBmp = FreeImageBitmap.FromStream(stream);
                        }

                        if (roughnessBmp != null || metalnessBmp != null)
                        {
                            int width = 0;
                            int height = 0;

                            if (roughnessBmp != null)
                            {
                                width = roughnessBmp.Width;
                                height = roughnessBmp.Height;
                            }

                            if (metalnessBmp != null)
                            {
                                width = metalnessBmp.Width;
                                height = metalnessBmp.Height;
                            }

                            using var physicalDescriptorBmp = new FreeImageBitmap(width, height, PixelFormat.Format32bppArgb);
                            unsafe
                            {
                                var firstPixel = ((uint*)physicalDescriptorBmp.Scan0.ToPointer()) - ((physicalDescriptorBmp.Height - 1) * physicalDescriptorBmp.Width);
                                var size = physicalDescriptorBmp.Width * physicalDescriptorBmp.Height;
                                var span = new Span<UInt32>(firstPixel, size);
                                span.Fill(DefaultPhysical);
                            }
                            if (metalnessBmp != null)
                            {
                                physicalDescriptorBmp.SetChannel(metalnessBmp, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                            }
                            if (roughnessBmp != null)
                            {
                                physicalDescriptorBmp.SetChannel(roughnessBmp, FREE_IMAGE_COLOR_CHANNEL.FICC_GREEN);
                            }

                            var physicalDescriptorMap = textureLoader.CreateTextureFromImage(physicalDescriptorBmp, 0, "physicalDescriptorMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                            result.SetPhysicalDescriptorMap(physicalDescriptorMap);
                            Barriers.Add(new StateTransitionDesc { pResource = physicalDescriptorMap.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, UpdateResourceState = true });
                        }
                    }
                    finally
                    {
                        roughnessBmp?.Dispose();
                        metalnessBmp?.Dispose();
                    }

                }

                if (resourceProvider.fileExists(ambientOcclusionMapPath))
                {
                    using (var stream = resourceProvider.openFile(normalMapPath))
                    {
                        var map = textureLoader.LoadTexture(stream, "ambientOcclusionMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        result.SetAmbientOcclusionMap(map);
                        Barriers.Add(new StateTransitionDesc { pResource = map.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, UpdateResourceState = true });
                    }
                }
            });

            graphicsEngine.ImmediateContext.TransitionResourceStates(Barriers);

            return result;
        }
    }
}