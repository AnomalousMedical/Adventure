using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Staff3 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Staff3();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Staff3.png";
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
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Hand position
                        SpriteFrameAttachment.FromFramePosition(11, 15, -0.1f, 23, 62), //Light position
                    }
                },
                new SpriteFrame(1f / 4f, 0, 2f / 4f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Hand position
                        SpriteFrameAttachment.FromFramePosition(11, 16, -0.1f, 23, 62), //Light position
                    }
                },
                new SpriteFrame(2f / 4f, 0, 3f / 4f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Hand position
                        SpriteFrameAttachment.FromFramePosition(11, 16, -0.1f, 23, 62), //Light position
                    }
                },
                new SpriteFrame(3f / 4f, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(11, 51, 0, 23, 62), //Hand position
                        SpriteFrameAttachment.FromFramePosition(11, 15, -0.1f, 23, 62), //Light position
                    }
                }
                )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            {
                BaseScale = new Vector3(23f / 62f * 1.45f, 1.45f, 1.0f),
                KeepTime = true
            };
        }

        public Light CreateLight()
        {
            return new Light()
            {
                Color = Color.FromRGB(0x1b97c3),
                Length = 0.7f,
            };
        }

        public int? LightAttachmentChannel => 1;
    }
}
