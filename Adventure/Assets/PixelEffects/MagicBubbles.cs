using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.PixelEffects
{
    class MagicBubbles : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/FreePixelEffectsPack/20_magicbubbles_spritesheet.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>();
        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.1f * Clock.SecondsToMicro), SpriteBuilder.CreateAnimatedSprite(100, 100, 8, 60)) },
        };

        public Sprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
