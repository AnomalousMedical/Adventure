using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Spear1 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Spear1();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Spear1.png";
        
        protected const uint Handle = 0xff705446;
        protected const uint Head = 0xffa8bbb0;

        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Handle, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(Head, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
        };

        private readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public Spear1()
        {

        }

        public Spear1(uint handleColor, uint headColor)
        {
            var palletSwap = new Dictionary<uint, uint>
            {
                { Handle, handleColor },
                { Head, headColor }
            };

            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: palletSwap
            );
        }

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
                        SpriteFrameAttachment.FromFramePosition(2f, 25, 0, 5, 33), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(5f / 33f * 1.2f, 1.2f, 1.0f) };
        }
    }

    class SmithedSpear : Spear1
    {
        public SmithedSpear()
            : base(0xff8f0c0c, 0xff757e78)
        {
        }
    }
}
