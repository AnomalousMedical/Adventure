using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class UltimateSword : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/UltimateSword.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffb11e15, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffc0c0bf, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff777777, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xfffda801, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12.5f, 54, 0, 26, 64), //Center of grip
                    }
                } )
            },
        };

        public Sprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(26f / 64f * 1.45f, 1.45f, 1.0f) };
        }
    }
}
