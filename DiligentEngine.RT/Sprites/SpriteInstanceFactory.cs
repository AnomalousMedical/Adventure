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
        private readonly ISpriteMaterialManager spriteMaterialManager;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly ActiveTextures activeTextures;
        private readonly SpritePlaneBLAS spritePlaneBLAS;

        private readonly PooledResourceManager<SpriteMaterialDescription, SpriteInstance> pooledResources
            = new PooledResourceManager<SpriteMaterialDescription, SpriteInstance>();

        public SpriteInstanceFactory
        (
            ISpriteMaterialManager spriteMaterialManager, 
            PrimaryHitShader.Factory primaryHitShaderFactory,
            ActiveTextures activeTextures,
            SpritePlaneBLAS spritePlaneBLAS
        )
        {
            this.spriteMaterialManager = spriteMaterialManager;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.activeTextures = activeTextures;
            this.spritePlaneBLAS = spritePlaneBLAS;
        }

        public Task<SpriteInstance> Checkout(SpriteMaterialDescription desc, ISprite sprite)
        {
            return pooledResources.Checkout(desc, async () =>
            {
                var instanceName = RTId.CreateId("SpriteInstanceFactory");

                var material = await spriteMaterialManager.Checkout(desc);

                var shader = await primaryHitShaderFactory.Checkout();

                var instance = new SpriteInstance(shader, primaryHitShaderFactory, material, spriteMaterialManager, activeTextures, spritePlaneBLAS);
                return pooledResources.CreateResult(instance);
            });
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
