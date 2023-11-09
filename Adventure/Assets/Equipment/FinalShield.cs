using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class FinalShield : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/FinalShield.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff9c9c9e, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public Quaternion GetOrientation()
        {
            return Quaternion.Identity;
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        const float width = 25;
        const float height = 42;

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12, 28.5f, 0, width, height), //Center of grip
                    }
                },
                new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12, 28.5f, 0, width, height), //Center of grip
                    }
                },
                new SpriteFrame(2f / 3f, 0f, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12, 28.5f, 0, width, height), //Center of grip
                    }
                },
                new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12, 28.5f, 0, width, height), //Center of grip
                    }
                })
            },
        };

        public ISprite CreateSprite()
        {
            return new KeepTimeSprite(animations)
            { BaseScale = new Vector3(width / height * 0.95f, 0.95f, 1.0f) };
        }
    }
}
