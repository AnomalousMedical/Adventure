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
            new SpriteMaterialTextureItem(0xff525252, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
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
                        SpriteFrameAttachment.FromFramePosition(9, 52, 0, 18, 64), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(18f / 64f * 1.45f, 1.45f, 1.0f) };
        }
    }
}
