using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Hammer3 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Hammer3();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Hammer3.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff705446, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff5d5d5d, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff060606, "Graphics/Textures/AmbientCG/Metal009_1K", "jpg", reflective: true),
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

        private static readonly Dictionary<string, SpriteAnimation> animations = FuncOp.Create(() =>
        {
            var attachments = new List<SpriteFrameAttachment>()
            {
                SpriteFrameAttachment.FromFramePosition(13.5f, 42f, 0, 27, 54), //Center of grip
                SpriteFrameAttachment.FromFramePosition(4f, 7f, -0.1f, 27, 54), //Light
            };

            var reverseAttachments = new List<SpriteFrameAttachment>()
            {
                SpriteFrameAttachment.FromFramePosition(13.5f, 42f, 0, 27, 54), //Center of grip
                SpriteFrameAttachment.FromFramePosition(23f, 7f, -0.1f, 27, 54), //Light
            };

            var animSpeed = (int)(0.7f * Clock.SecondsToMicro);

            var defaultOrientation = new SpriteAnimation(animSpeed,
                    new SpriteFrame(0, 0, 1f / 3f, 1)
                    {
                        Attachments = attachments
                    },
                    new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                    {
                        Attachments = attachments
                    },
                    new SpriteFrame(2f / 3f, 0f, 1, 1)
                    {
                        Attachments = attachments
                    });

            var reversedOrientation = new SpriteAnimation(animSpeed,
                    new SpriteFrame(1f / 3f, 0, 0f, 1)
                    {
                        Attachments = reverseAttachments
                    },
                    new SpriteFrame(2f / 3f, 0, 1f / 3f, 1)
                    {
                        Attachments = reverseAttachments
                    },
                    new SpriteFrame(1, 0f, 2f / 3f, 1)
                    {
                        Attachments = reverseAttachments
                    });

            var anims = new Dictionary<string, SpriteAnimation>()
            {
                { "default", defaultOrientation },
                { "right", reversedOrientation },
                { "up", reversedOrientation },
                { "stand-right", reversedOrientation },
                { "stand-up", reversedOrientation },
            };

            return anims;
        });

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            {
                BaseScale = new Vector3(27f / 54f * 1.45f, 1.45f, 1.0f),
                KeepTime = true
            };
        }

        public Light CreateLight()
        {
            return new Light()
            {
                Color = Color.FromRGB(0xffeb00),
                Length = 0.7f,
            };
        }

        public int? LightAttachmentChannel => 1;
    }
}
