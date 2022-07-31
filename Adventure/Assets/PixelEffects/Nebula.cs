using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.PixelEffects
{
    class Nebula : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/FreePixelEffectsPack/12_nebula_spritesheet.png";
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
            { BaseScale = new Vector3(0.4f, 0.4f, 0.4f) };
        }
    }
}
