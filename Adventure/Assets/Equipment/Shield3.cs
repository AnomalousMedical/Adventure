using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Shield3 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Shield3();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Shield3.png";
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

        const float width = 19;
        const float height = 27;

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 18, 0, width, height), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(4, 9, -0.1f, width, height), //light
                    }
                },
                new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 18, 0, width, height), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(9, 9, -0.1f, width, height), //light
                    }
                },
                new SpriteFrame(2f / 3f, 0f, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 18, 0, width, height), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(14, 9, -0.1f, width, height), //light
                    }
                },
                new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(9, 18, 0, width, height), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(9, 9, -0.1f, width, height), //light
                    }
                })
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            {
                BaseScale = new Vector3(width / height * 0.85f, 0.85f, 1.0f),
                KeepTime = true
            };
        }

        public Light CreateLight()
        {
            return new Light()
            {
                Color = Color.FromRGB(0xef00ff),
                Length = 0.6f,
            };
        }

        public int? LightAttachmentChannel => 1;
    }
}
