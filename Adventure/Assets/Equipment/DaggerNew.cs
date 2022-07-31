using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class DaggerNew : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Crawl/Weapons/dagger_new.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff692c0c, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Hilt (brown)
            new SpriteMaterialTextureItem(0xffa3a3a3, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Blade (grey)
            new SpriteMaterialTextureItem(0xff535353, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Other Metal (grey)
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
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
                        SpriteFrameAttachment.FromFramePosition(11, 20, 0, 32, 32), //Center of grip
                    }
                } )
            },
        };

        public Sprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(0.65f, 0.65f, 0.65f) };
        }
    }
}
