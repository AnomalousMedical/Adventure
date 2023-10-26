using DiligentEngine.RT.Sprites;
using System.Collections.Generic;

namespace Adventure.Assets
{
    public interface IPlayerSprite
    {
        Dictionary<string, SpriteAnimation> Animations { get; }

        SpriteMaterialDescription Tier1 { get; }

        SpriteMaterialDescription Tier2 { get; }

        SpriteMaterialDescription Tier3 { get; }
    }
}