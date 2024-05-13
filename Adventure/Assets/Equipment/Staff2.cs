using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Staff2 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Staff2();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Staff2.png";
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
            { "default", new SpriteAnimation((int)(1.4f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1f / 2f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(8.5f, 44, 0, 18, 57), //Hand position
                        SpriteFrameAttachment.FromFramePosition(8.5f, 8, -0.1f, 18, 57), //Light position
                    }
                },
                new SpriteFrame(1f / 2f, 0, 1f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(8.5f, 44, 0, 18, 57), //Hand position
                        SpriteFrameAttachment.FromFramePosition(8.5f, 9, -0.1f, 18, 57), //Light position
                    }
                }
                )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            {
                BaseScale = new Vector3(18f / 57f * 1.25f, 1.25f, 1.0f),
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
