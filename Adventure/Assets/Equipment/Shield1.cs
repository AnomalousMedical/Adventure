using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Shield1 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Shield1();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Shield1.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffbac5d7, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Outer and inner rings (grey)
            new SpriteMaterialTextureItem(0xff80685c, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), //Body
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

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
                        SpriteFrameAttachment.FromFramePosition(8, 8, 0, 16, 16), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations) { BaseScale = new Vector3(0.45f, 0.45f, 1f) };
        }
    }
}
