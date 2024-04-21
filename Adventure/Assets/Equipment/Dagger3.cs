using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Dagger3 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Dagger3();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Dagger3.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff425d5a, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffffeb00, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff1e1e1e, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                SpriteFrameAttachment.FromFramePosition(5, 27, 0, 13, 33), //Center of grip
            };

            var reversedAttachments = new List<SpriteFrameAttachment>()
            {
                SpriteFrameAttachment.FromFramePosition(7, 27, 0, 13, 33), //Center of grip
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
                { "up", defaultOrientation },
                { "stand-right", reversedOrientation },
                { "stand-up", defaultOrientation },
                { "down", reversedOrientation },
                { "stand-down", reversedOrientation },
            };

            return anims;
        });

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(13f / 33f * 1.3f, 1.3f, 1.0f) };
        }
    }
}
