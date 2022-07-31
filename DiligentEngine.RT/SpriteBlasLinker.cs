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
        private readonly TLASInstanceData instanceBuildData;
        private readonly SpriteInstance spriteInstance;

        public SpriteBlasLinker(ISprite wrappedSprite, TLASInstanceData instanceData, SpriteInstance spriteInstance)
        {
            this.wrappedSprite = wrappedSprite;
            this.instanceBuildData = instanceData;
            this.spriteInstance = spriteInstance;
            spriteInstance.UpdateBlas(instanceData);
        }

        public ISprite WrappedSprite => wrappedSprite;

        public void Update(Clock clock)
        {
            wrappedSprite.Update(clock);
        }
    }
}
