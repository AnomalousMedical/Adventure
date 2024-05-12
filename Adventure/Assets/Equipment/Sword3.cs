using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Sword3 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Sword3();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Sword3.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff201309, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff5e6166, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                new SpriteFrame(0, 0, 1f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6.5f, 38.5f, 0, 14, 45), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(6.5f, 23f, -0.1f, 14, 45), //Light
                    }
                },
                new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6.5f, 38.5f, 0, 14, 45), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(6.5f, 23f, -0.1f, 14, 45), //Light
                    }
                },
                new SpriteFrame(2f / 3f, 0f, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6.5f, 38.5f, 0, 14, 45), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(6.5f, 23f, -0.1f, 14, 45), //Light
                    }
                })
            },
        };

        public ISprite CreateSprite()
        {
            return new KeepTimeSprite(animations)
            { BaseScale = new Vector3(14f / 45f * 1.45f, 1.45f, 1.0f) };
        }

        public Light CreateLight()
        {
            return new Light()
            {
                Color = Color.FromRGB(0x2fe38c),
                Length = 0.7f,
            };
        }

        public int? LightAttachmentChannel => 1;
    }
}
