using DiligentEngine;
using DiligentEngine.RT.Resources;
using Engine;
using Engine.Resources;
using FreeImageAPI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Sprites
{
    public class SpriteMaterialTextureDescription
    {
        public SpriteMaterialTextureDescription(string baseMap, HashSet<SpriteMaterialTextureItem> materials)
        {
            BaseMap = baseMap;
            Materials = materials;
        }

        String BaseMap { get; }

        public HashSet<SpriteMaterialTextureItem> Materials { get; }

        public override bool Equals(object obj)
        {
            return obj is SpriteMaterialTextureDescription description &&
                   BaseMap == description.BaseMap &&
                    (
                        (Materials == null && description.Materials == null) ||
                        (Materials?.SetEquals(description.Materials) == true)
                    );
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(BaseMap);
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
            return hashCode.ToHashCode();
        }
    }

    public class SpriteMaterialTextures : IDisposable
    {
        AutoPtr<ITexture> normalTexture;
        AutoPtr<ITexture> physicalTexture;
        AutoPtr<ITexture> aoTexture;

        public SpriteMaterialTextures(AutoPtr<ITexture> normalTexture, AutoPtr<ITexture> physicalTexture, AutoPtr<ITexture> aoTexture, bool reflective)
        {
            this.normalTexture = normalTexture;
            this.physicalTexture = physicalTexture;
            this.aoTexture = aoTexture;
            this.Reflective = reflective;
        }

        public void Dispose()
        {
            normalTexture?.Dispose();
            physicalTexture?.Dispose();
            aoTexture?.Dispose();
        }


        public ITexture NormalTexture => normalTexture?.Obj;
        public ITexture PhysicalTexture => physicalTexture?.Obj;
        public ITexture AoTexture => aoTexture?.Obj;
        public bool Reflective { get; internal set; }
    }

    class SpriteMaterialTextureManager : ISpriteMaterialTextureManager
    {
        private readonly PooledResourceManager<SpriteMaterialTextureDescription, SpriteMaterialTextures> pooledResources
            = new PooledResourceManager<SpriteMaterialTextureDescription, SpriteMaterialTextures>();

        private readonly CC0MaterialTextureBuilder cc0MaterialTextureBuilder;
        private readonly TextureLoader textureLoader;
        private readonly ILogger<SpriteMaterialTextureManager> logger;
        private readonly GraphicsEngine graphicsEngine;

        public SpriteMaterialTextureManager(
             CC0MaterialTextureBuilder cc0MaterialTextureBuilder,
             TextureLoader textureLoader,
             ILogger<SpriteMaterialTextureManager> logger,
             GraphicsEngine graphicsEngine
            )
        {
            this.cc0MaterialTextureBuilder = cc0MaterialTextureBuilder;
            this.textureLoader = textureLoader;
            this.logger = logger;
            this.graphicsEngine = graphicsEngine;
        }

        public Task<SpriteMaterialTextures> Checkout(FreeImageBitmap image, SpriteMaterialTextureDescription desc)
        {
            return pooledResources.Checkout(desc, async () =>
            {
                var barriers = new List<StateTransitionDesc>(3);

                var fromPoolResult = await Task.Run(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    var scale = Math.Min(1024 / image.Width, 1024 / image.Height); //This needs to become configurable

                    using var ccoTextures = cc0MaterialTextureBuilder.CreateMaterialSet(image, scale, desc.Materials?.ToDictionary(k => k.Color, e => new CC0MaterialDesc(e.BasePath, e.Ext, e.Reflective)));

                    AutoPtr<ITexture> normalTexture = null;
                    if (ccoTextures.NormalMap != null)
                    {
                        normalTexture = textureLoader.CreateTextureFromImage(ccoTextures.NormalMap, 1, "normalTextureSprite", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        barriers.Add(new StateTransitionDesc { pResource = normalTexture.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }

                    AutoPtr<ITexture> physicalTexture = null;
                    if (ccoTextures.PhysicalDescriptorMap != null)
                    {
                        physicalTexture = textureLoader.CreateTextureFromImage(ccoTextures.PhysicalDescriptorMap, 1, "physicalTextureSprite", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        barriers.Add(new StateTransitionDesc { pResource = physicalTexture.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }

                    AutoPtr<ITexture> aoTexture = null;
                    if (ccoTextures.AmbientOcclusionMap != null)
                    {
                        aoTexture = textureLoader.CreateTextureFromImage(ccoTextures.AmbientOcclusionMap, 1, "aoTextureSprite", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
                        barriers.Add(new StateTransitionDesc { pResource = aoTexture.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }

                    var result = new SpriteMaterialTextures(normalTexture, physicalTexture, aoTexture, desc.Materials.Any(i => i.Reflective));

                    sw.Stop();
                    logger.LogInformation($"Loaded sprite texture in {sw.ElapsedMilliseconds} ms.");

                    return pooledResources.CreateResult(result);
                });

                //This must be on the main thread
                graphicsEngine.ImmediateContext.TransitionResourceStates(barriers);

                return fromPoolResult;
            });
        }

        public void Return(SpriteMaterialTextures binding)
        {
            pooledResources.Return(binding);
        }
    }
}
