using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Staff07 : ISpriteAsset
    {
        public const uint Staff = 0xff9f7f66;//Staff (brown)
        public const uint Crystal = 0xff3722af;//Crystal (purple)

        protected Dictionary<uint, uint> PalletSwap { get; set; }

        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
                (
                    colorMap: "Graphics/Sprites/Crawl/Weapons/staff_7.png",
                    materials: new HashSet<SpriteMaterialTextureItem>
                    {
                        new SpriteMaterialTextureItem(Staff, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
                        new SpriteMaterialTextureItem(Crystal, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg"),
                    },
                    palletSwap: PalletSwap
                );
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6, 25, 0, 32, 32), //Center of grip
                    }
                } )
            },
        };

        public Sprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(1f, 1f, 1f) };
        }
    }

    class FireStaff07 : Staff07
    {
        public FireStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, ElementColors.Fire }
            };
        }
    }

    class IceStaff07 : Staff07
    {
        public IceStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, ElementColors.Ice }
            };
        }
    }

    class ZapStaff07 : Staff07
    {
        public ZapStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, ElementColors.Electricity }
            };
        }
    }

    class AcidStaff07 : Staff07
    {
        public AcidStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, ElementColors.Acid }
            };
        }
    }

    class GravityStaff07 : Staff07
    {
        public GravityStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, ElementColors.Gravity }
            };
        }
    }



    class EarthStaff07 : Staff07
    {
        public EarthStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, ElementColors.Earth }
            };
        }
    }
}
