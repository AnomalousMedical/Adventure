using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Spear2Old : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Crawl/Weapons/spear_2_old.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffa00000, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), //Staff (red)
            new SpriteMaterialTextureItem(0xffe0ba4a, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Blade (gold)
            new SpriteMaterialTextureItem(0xffe0e0e0, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Blade (silver)
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(7, 24, 0, 32, 32), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(0.75f, 0.75f, 0.75f) };
        }
    }
}
