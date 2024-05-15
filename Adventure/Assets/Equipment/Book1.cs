using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Book1 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Book1();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Book1.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffc77000, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Highlights
            new SpriteMaterialTextureItem(0xff725a47, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Cover
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
                        SpriteFrameAttachment.FromFramePosition(12, 18, 0, 23, 32), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations) { BaseScale = new Vector3(21f / 29f * 0.5f, 0.5f, 0.5f) };
        }
    }
}
