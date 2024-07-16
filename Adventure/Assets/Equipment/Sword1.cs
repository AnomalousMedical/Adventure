using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Sword1 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Sword1();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Sword1.png";
        
        protected const uint Handle = 0xff692c0c;
        protected const uint Blade = 0xffc0c0bf;
        protected const uint Guard = 0xff5e5858;

        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Handle, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(Blade, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(Guard, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
        };

        private readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public Sword1()
        {

        }

        public Sword1(uint handleColor, uint bladeColor, uint guardColor)
        {
            var palletSwap = new Dictionary<uint, uint>
            {
                { Handle, handleColor },
                { Blade, bladeColor },
                { Guard, guardColor }
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
                        SpriteFrameAttachment.FromFramePosition(4.5f, 24.5f, 0, 9, 31), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(9f / 31f * 1f, 1f, 1.0f) };
        }
    }

    class SmithedSword : Sword1
    {
        public SmithedSword()
            : base(0xff8f0c0c, 0xff818181, 0xff888f00)
        {
        }
    }
}
