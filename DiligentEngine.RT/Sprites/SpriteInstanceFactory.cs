using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Sprites
{
    public class SpriteInstanceFactory
    {
        private readonly SpritePlaneBLAS.Factory spriteBLAS;
        private readonly ISpriteMaterialManager spriteMaterialManager;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly ActiveTextures activeTextures;

        private readonly PooledResourceManager<SpriteMaterialDescription, SpriteInstance> pooledResources
            = new PooledResourceManager<SpriteMaterialDescription, SpriteInstance>();

        public SpriteInstanceFactory
        (
            SpritePlaneBLAS.Factory spriteBLASFactory, 
            ISpriteMaterialManager spriteMaterialManager, 
            PrimaryHitShader.Factory primaryHitShaderFactory,
            ActiveTextures activeTextures
        )
        {
            this.spriteBLAS = spriteBLASFactory;
            this.spriteMaterialManager = spriteMaterialManager;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.activeTextures = activeTextures;
        }

        public Task<SpriteInstance> Checkout(SpriteMaterialDescription desc, Dictionary<string, SpriteAnimation> animations = null)
        {
            return pooledResources.Checkout(desc, async () =>
            {
                var instanceName = RTId.CreateId("SpriteInstanceFactory");

                var material = await spriteMaterialManager.Checkout(desc);

                var shader = await primaryHitShaderFactory.Checkout();

                var blasLoaders = new Dictionary<String, List<Task<SpritePlaneBLAS>>>();

                if (animations != null)
                {
                    foreach (var animation in animations)
                    {
                        var animFrames = new List<Task<SpritePlaneBLAS>>();
                        blasLoaders[animation.Key] = animFrames;
                        foreach (var frame in animation.Value.frames)
                        {
                            animFrames.Add(LoadBlas(frame));
                        }
                    }
                }
                else
                {
                    blasLoaders["default"] = new List<Task<SpritePlaneBLAS>>(1) { spriteBLAS.Checkout(new SpritePlaneBLAS.Desc()) };
                }

                var blasFrames = new Dictionary<String, List<SpritePlaneBLAS>>(blasLoaders.Count);
                foreach (var loader in blasLoaders)
                {
                    var animFrames = new List<SpritePlaneBLAS>(loader.Value.Count);
                    blasFrames[loader.Key] = animFrames;
                    foreach(var frame in loader.Value)
                    {
                        animFrames.Add(await frame);
                    }
                }

                //You could cache the animations once more if you can make sure each asset only loads them once
                //Then you won't need to even do all this lookup

                var instance = new SpriteInstance(blasFrames, shader, primaryHitShaderFactory, material, spriteMaterialManager, activeTextures);
                return pooledResources.CreateResult(instance);
            });
        }

        private async Task<SpritePlaneBLAS> LoadBlas(SpriteFrame frame)
        {
            var blasDesc = new SpritePlaneBLAS.Desc()
            {
                Left = frame.Left,
                Top = frame.Top,
                Right = frame.Right,
                Bottom = frame.Bottom,
            };
            var blas = await spriteBLAS.Checkout(blasDesc);
            return blas;
        }

        public void TryReturn(SpriteInstance item)
        {
            if (item != null)
            {
                pooledResources.Return(item);
            }
        }
    }
}
