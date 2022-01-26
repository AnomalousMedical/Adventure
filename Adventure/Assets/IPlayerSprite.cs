using DiligentEngine.RT.Sprites;
using System.Collections.Generic;

namespace Adventure.Assets
{
    public interface IPlayerSprite
    {
        Dictionary<string, SpriteAnimation> Animations { get; }
        SpriteMaterialDescription SpriteMaterialDescription { get; }
    }
}