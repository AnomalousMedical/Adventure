using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class MaceLarge2New : ISpriteAsset
    {
        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
        }

        private const string colorMap = "Graphics/Sprites/Crawl/Weapons/mace_large_2_new.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff9e0000, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), //Handle (red)
            new SpriteMaterialTextureItem(0xffffc000, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Highlight (gold)
            new SpriteMaterialTextureItem(0xffbfcfde, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Bludgeon (silver)
        };

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
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 22, 0, 32, 32), //Center of grip
                    }
                } )
            },
        };

        public Sprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(0.75f, 0.75f, 0.75f) };
        }
    }
}
