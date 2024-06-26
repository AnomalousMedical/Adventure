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
        public const uint DefaultNormal = 0x00FF7F7F;
        private const uint DefaultPhysicalReflective = 0xFF00FF00; //Reflective loads the alpha channel
        private const uint DefaultPhysicalNoReflect = 0x0000FF00;

        public static uint GetDefaultPhysicalPixel(bool reflective)
        {
            if (reflective)
            {
                return DefaultPhysicalReflective;
            }
            return DefaultPhysicalNoReflect;
        }

        private readonly TextureLoader textureLoader;
        private readonly IResourceProvider<CC0TextureLoader> resourceProvider;
        private readonly GraphicsEngine graphicsEngine;

        public CC0TextureLoader(TextureLoader textureLoader, IResourceProvider<CC0TextureLoader> resourceProvider, GraphicsEngine graphicsEngine)
        {
            this.textureLoader = textureLoader;
            this.resourceProvider = resourceProvider;
            this.graphicsEngine = graphicsEngine;
        }

        public async Task<CC0TextureResult> LoadTextureSet(CCOTextureBindingDescription desc)
        {
            //In this function the auto pointers are handed off to the result, which will be managed by the caller to erase the resources.
            var result = new CC0TextureResult();

            var colorMapPath = $"{desc.BaseName}_Color.{desc.Ext}";
            var normalMapPath = $"{desc.BaseName}_Normal.{desc.Ext}";
            var roughnessMapPath = $"{desc.BaseName}_Roughness.{desc.Ext}";
            var metalnessMapPath = $"{desc.BaseName}_Metalness.{desc.Ext}";
            var ambientOcclusionMapPath = $"{desc.BaseName}_AmbientOcclusion.{desc.Ext}";
            var opacityFile = $"{desc.BaseName}_Opacity.jpg";
            var emissiveMapPath = $"{desc.BaseName}_Emission.jpg";

            var Barriers = new List<StateTransitionDesc>(5);

            await Task.Run(() =>
            {
                if (resourceProvider.fileExists(colorMapPath))
                {
                    using (var stream = resourceProvider.openFile(colorMapPath))
                    {
                        using var bmp = FreeImageBitmap.FromStream(stream);
                        bool hasOpacity = false;
                        if (desc.AllowOpacityMapLoad && resourceProvider.exists(opacityFile))
                        {
                            //Jam opacity map into color alpha channel if it exists
                            bmp.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_32_BPP);
                            using var opacityStream = resourceProvider.openFile(opacityFile);
                            using var opacityBmp = FreeImageBitmap.FromStream(opacityStream);
                            opacityBmp.ConvertColorDepth(FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP);
                            bmp.SetChannel(opacityBmp, FREE_IMAGE_COLOR_CHANNEL.FICC_ALPHA);
                            hasOpacity = true;
                        }
                        var baseColorMap = textureLoader.CreateTextureFromImage(bmp, desc.MipLevels, "baseColorMap from cc0", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, true);
                        result.SetBaseColorMap(baseColorMap, hasOpacity, desc.Reflective);
                        Barriers.Add(new StateTransitionDesc { pResource = baseColorMap.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }
                }

                if (resourceProvider.fileExists(normalMapPath))
                {
                    using (var stream = resourceProvider.openFile(normalMapPath))
                    {
                        using var map = FreeImageBitmap.FromStream(stream);

                        var normalMap = textureLoader.CreateTextureFromImage(map, desc.MipLevels, "normalTexture from cc0", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        result.SetNormalMap(normalMap);
                        Barriers.Add(new StateTransitionDesc { pResource = normalMap.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
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
                                var fillColor = DefaultPhysicalNoReflect;
                                if (desc.Reflective)
                                {
                                    fillColor = DefaultPhysicalReflective;
                                }
                                span.Fill(fillColor);
                            }
                            if (metalnessBmp != null)
                            {
                                physicalDescriptorBmp.SetChannel(metalnessBmp, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
                            }
                            if (roughnessBmp != null)
                            {
                                physicalDescriptorBmp.SetChannel(roughnessBmp, FREE_IMAGE_COLOR_CHANNEL.FICC_GREEN);
                            }

                            var physicalDescriptorMap = textureLoader.CreateTextureFromImage(physicalDescriptorBmp, desc.MipLevels, "physicalDescriptorMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                            result.SetPhysicalDescriptorMap(physicalDescriptorMap);
                            Barriers.Add(new StateTransitionDesc { pResource = physicalDescriptorMap.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
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
                    using (var stream = resourceProvider.openFile(ambientOcclusionMapPath))
                    {
                        var map = textureLoader.LoadTexture(stream, "ambientOcclusionMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        result.SetAmbientOcclusionMap(map);
                        Barriers.Add(new StateTransitionDesc { pResource = map.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }
                }

                if (resourceProvider.fileExists(emissiveMapPath))
                {
                    using (var stream = resourceProvider.openFile(emissiveMapPath))
                    {
                        var map = textureLoader.LoadTexture(stream, "emissiveMap", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        result.SetEmissiveMap(map);
                        Barriers.Add(new StateTransitionDesc { pResource = map.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }
                }
            });

            graphicsEngine.ImmediateContext.TransitionResourceStates(Barriers);

            return result;
        }
    }
}
