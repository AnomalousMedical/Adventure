using Engine.Platform;
using System;
using System.Collections.Generic;

namespace DiligentEngine.RT.Sprites
{
    public interface ISprite
    {
        SpriteFrame GetCurrentFrame();
        void SetAnimation(string animationName);
        void Update(Clock clock);
        int FrameIndex { get; }
        String CurrentAnimationName { get; }
        IReadOnlyDictionary<String, SpriteAnimation> Animations { get; }
    }
}