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

        public Task<SpriteInstance> Checkout(SpriteMaterialDescription desc)
        {
            return pooledResources.Checkout(desc, async () =>
            {
                var instanceName = RTId.CreateId("SpriteInstanceFactory");

                var material = await spriteMaterialManager.Checkout(desc);

                var shader = await primaryHitShaderFactory.Checkout();

                var blas = await spriteBLAS.Checkout(new SpritePlaneBLAS.Desc()); //DO better
                var instance = new SpriteInstance(blas, shader, primaryHitShaderFactory, material, spriteMaterialManager, activeTextures);
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
