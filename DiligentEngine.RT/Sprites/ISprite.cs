using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;

namespace DiligentEngine.RT.Sprites
{
    public interface ISprite
    {
        Vector3 BaseScale { get; }
        SpriteFrame GetCurrentFrame();
        void SetAnimation(string animationName);
        void Update(Clock clock);
        void RandomizeFrameTime();

        int FrameIndex { get; }
        String CurrentAnimationName { get; }
        IReadOnlyDictionary<String, SpriteAnimation> Animations { get; }
    }
}