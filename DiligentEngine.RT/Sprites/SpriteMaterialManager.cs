﻿using DiligentEngine;
using Engine;
using Engine.Resources;
using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Sprites
{
    public class SpriteMaterialTextureItem
    {
        public uint Color { get; }
        public String BasePath { get; }
        public String Ext { get; }
        public bool Reflective { get; set; }

        public SpriteMaterialTextureItem(uint color, string basePath, string ext, bool reflective = false)
        {
            this.Color = color;
            this.BasePath = basePath;
            this.Ext = ext;
            this.Reflective = reflective;
        }

        public override bool Equals(object obj)
        {
            return obj is SpriteMaterialTextureItem description &&
                   Color == description.Color &&
                   BasePath == description.BasePath &&
                   Ext == description.Ext &&
                   Reflective == description.Reflective;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Color, BasePath, Ext, Reflective);
        }
    }

    public class SpriteMaterialDescription
    {
        public SpriteMaterialDescription(string colorMap, HashSet<SpriteMaterialTextureItem> materials, Dictionary<uint, uint> palletSwap = null)
        {
            this.ColorMap = colorMap;
            this.Materials = materials;
            this.PalletSwap = palletSwap;
        }

        public String ColorMap { get; set; }

        public HashSet<SpriteMaterialTextureItem> Materials { get; set; }

        public Dictionary<uint, uint> PalletSwap { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SpriteMaterialDescription description &&
                   ColorMap == description.ColorMap &&
                   (
                       (Materials == null && description.Materials == null) ||
                       (Materials?.SetEquals(description.Materials) == true)
                   )
                   &&
                   (
                       (PalletSwap == null && description.PalletSwap == null) ||
                       (description.PalletSwap != null && PalletSwap?.OrderBy(i => i.Key).SequenceEqual(description.PalletSwap.OrderBy(i => i.Key)) == true)
                   );
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ColorMap);
            if (Materials != null && Materials.Count > 0) //Null and empty considered the same
            {
                foreach (var mat in Materials.OrderBy(i => i.Color))
                {
                    hashCode.Add(mat);
                }
            }
            else
            {
                hashCode.Add<Object>(null);
            }
            if(PalletSwap != null && PalletSwap.Count > 0)
            {
                foreach(var swap in PalletSwap.OrderBy(i => i.Key))
                {
                    hashCode.Add(swap);
                }
            }
            else
            {
                hashCode.Add<Object>(null);
            }
            return hashCode.ToHashCode();
        }
    }

    public class SpriteMaterialManager : ISpriteMaterialManager
    {
        private readonly PooledResourceManager<SpriteMaterialDescription, SpriteMaterial> pooledResources
            = new PooledResourceManager<SpriteMaterialDescription, SpriteMaterial>();

        private readonly IResourceProvider<SpriteMaterialManager> resourceProvider;
        private readonly TextureLoader textureLoader;
        private readonly ISpriteMaterialTextureManager spriteMaterialTextureManager;
        private readonly GraphicsEngine graphicsEngine;

        public SpriteMaterialManager(
            IResourceProvider<SpriteMaterialManager> resourceProvider,
            TextureLoader textureLoader,
            ISpriteMaterialTextureManager spriteMaterialTextureManager,
            GraphicsEngine graphicsEngine
        )
        {
            this.resourceProvider = resourceProvider;
            this.textureLoader = textureLoader;
            this.spriteMaterialTextureManager = spriteMaterialTextureManager;
            this.graphicsEngine = graphicsEngine;
        }

        public Task<SpriteMaterial> Checkout(SpriteMaterialDescription desc)
        {
            return pooledResources.Checkout(desc, async () =>
            {
                List<StateTransitionDesc> barriers = new List<StateTransitionDesc>(1);

                using var image = await Task.Run(() =>
                {
                    using var stream = resourceProvider.openFile(desc.ColorMap);
                    return FreeImageBitmap.FromStream(stream);
                });

                //Order is important here, image is flipped when the texture is created below
                //Also need to get back onto the main thread to lookup the textures in the other manager
                //This makes it take multiple frames, but living with that for now
                //Ideally this should go back into 1 task.run to run at full speed, but this manager only
                //has main thread sync support
                var spriteMatTextures = await spriteMaterialTextureManager.Checkout(image, new SpriteMaterialTextureDescription(desc.ColorMap, desc.Materials));

                var pooledResult = await Task.Run(() =>
                {
                    var swaps = desc.PalletSwap;
                    var width = image.Width;
                    var height = image.Height;
                    if (swaps != null)
                    {
                        unsafe
                        {
                            var indexScan0 = (UInt32*)image.Scan0.ToPointer();
                            for (var y = 0; y < height; ++y)
                            {
                                var scanline = indexScan0 - y * width;
                                for (var x = 0; x < width; ++x)
                                {
                                    if (swaps.TryGetValue(scanline[x], out var remap))
                                    {
                                        scanline[x] = remap;
                                    }
                                }
                            }
                        }
                    }

                    using var colorTexture = textureLoader.CreateTextureFromImage(image, 1, "colorTexture", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, true)
                       ?? throw new InvalidOperationException("Could not create sprite color texture");
                    barriers.Add(new StateTransitionDesc { pResource = colorTexture.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });

                    var result = new SpriteMaterial(width, height, spriteMaterialTextureManager, spriteMatTextures, colorTexture.Obj);
                    return pooledResources.CreateResult(result);
                });

                //This needs to stay on the main thread
                graphicsEngine.ImmediateContext.TransitionResourceStates(barriers);

                return pooledResult;
            });
        }

        public void TryReturn(SpriteMaterial item)
        {
            if (item != null)
            {
                Return(item);
            }
        }

        public void Return(SpriteMaterial item)
        {
            pooledResources.Return(item);
        }
    }
}
