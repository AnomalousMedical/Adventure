using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Scimitar : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Scimitar();

        private const float Width = 19f;
        private const float Height = 50f;

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Scimitar.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff201309, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff8d8f92, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xffae8a52, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
            var normalAttachments = new List<SpriteFrameAttachment>()
            {
                SpriteFrameAttachment.FromFramePosition(4, 43, 0, Width, Height), //Center of grip
            };

            var reversedAttachments = new List<SpriteFrameAttachment>()
            {
                SpriteFrameAttachment.FromFramePosition(15, 43, 0, Width, Height), //Center of grip
            };

            var animSpeed = 5000000;

            var defaultOrientation = new SpriteAnimation(animSpeed,
                    new SpriteFrame(0, 0, 1, 1)
                    {
                        Attachments = normalAttachments
                    });

            var reversedOrientation = new SpriteAnimation(animSpeed,
                    new SpriteFrame(1f, 0f, 0f, 1f)
                    {
                        Attachments = reversedAttachments
                    });

            var anims = new Dictionary<string, SpriteAnimation>()
            {
                { "default", defaultOrientation },
                { "right", reversedOrientation },
                { "up", reversedOrientation },
                { "stand-right", reversedOrientation },
                { "stand-up", reversedOrientation },
                { "down", defaultOrientation },
                { "stand-down", defaultOrientation  },
            };

            return anims;
        });

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(Width / Height * 1.3f, 1.3f, 1.0f) };
        }
    }
}
