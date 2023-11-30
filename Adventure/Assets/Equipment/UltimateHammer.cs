using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class UltimateHammer : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new UltimateHammer();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/UltimateHammer.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffb11e15, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff8e90a4, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff5c716f, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xfff0b640, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                        SpriteFrameAttachment.FromFramePosition(9.5f, 26, 0, 20f, 32f), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(20f / 32f * 1.45f, 1.45f, 1.0f) };
        }
    }
}
