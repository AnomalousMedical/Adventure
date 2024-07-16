using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Hammer1 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Hammer1();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Hammer1.png";

        protected const uint Handle = 0xff692c0c;
        protected const uint Head = 0xff5c716f;

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

        public Quaternion GetOrientation()
        {
            return Quaternion.Identity;
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        public Hammer1()
        {

        }

        public Hammer1(uint handleColor, uint headColor)
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

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(7, 14, 0, 14f, 22f), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(14f / 22f * 0.75f, 0.75f, 1.0f) };
        }
    }

    class SmithedHammer : Hammer1
    {
        public SmithedHammer()
            :base(0xff8f0c0c, 0xff3d4a48)
        {
        }
    }
}
