using DiligentEngine.RT.Sprites;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    internal class SpriteBlasLinker
    {
        private ISprite wrappedSprite;
        private readonly TLASBuildInstanceData instanceBuildData;
        private readonly SpriteInstance spriteInstance;

        public SpriteBlasLinker(ISprite wrappedSprite, TLASBuildInstanceData instanceBuildData, SpriteInstance spriteInstance)
        {
            this.wrappedSprite = wrappedSprite;
            this.instanceBuildData = instanceBuildData;
            this.spriteInstance = spriteInstance;
            spriteInstance.UpdateBlas(instanceBuildData, wrappedSprite); //This one isn't really needed, but it ensures we seed with something since the user calls update
        }

        public ISprite WrappedSprite => wrappedSprite;

        public void Update(Clock clock)
        {
            wrappedSprite.Update(clock);
            spriteInstance.UpdateBlas(instanceBuildData, wrappedSprite);
        }
    }
}
