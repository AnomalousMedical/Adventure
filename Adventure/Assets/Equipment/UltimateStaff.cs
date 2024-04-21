using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class UltimateStaff : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new UltimateStaff();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/UltimateStaff.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff5b3c18, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
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

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1f / 4f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Center of grip
                    }
                },
                new SpriteFrame(1f / 4f, 0, 2f / 4f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Center of grip
                    }
                },
                new SpriteFrame(2f / 4f, 0, 3f / 4f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Center of grip
                    }
                },
                new SpriteFrame(3f / 4f, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Center of grip
                    }
                }
                )
            },
        };

        public ISprite CreateSprite()
        {
            return new KeepTimeSprite(animations)
            { BaseScale = new Vector3(23f / 62f * 1.45f, 1.45f, 1.0f) };
        }
    }
}
