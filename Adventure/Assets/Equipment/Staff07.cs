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

        public Sprite CreateSprite()
        {
            return new Sprite(new Dictionary<string, SpriteAnimation>()
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
                })
            { BaseScale = new Vector3(1f, 1f, 1f) };
        }
    }

    class FireStaff07 : Staff07
    {
        public FireStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xffde4509 }
            };
        }
    }

    class IceStaff07 : Staff07
    {
        public IceStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xff0962de }
            };
        }
    }

    class ZapStaff07 : Staff07
    {
        public ZapStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xffe3c923 }
            };
        }
    }

    class AcidStaff07 : Staff07
    {
        public AcidStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xff18c81b }
            };
        }
    }

    class LightStaff07 : Staff07
    {
        public LightStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xffffff }
            };
        }
    }

    class DarknessStaff07 : Staff07
    {
        public DarknessStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xff0c0c0c }
            };
        }
    }

    class WaterStaff07 : Staff07
    {
        public WaterStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xff1633b9 }
            };
        }
    }

    class PoisonStaff07 : Staff07
    {
        public PoisonStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xff16b933 }
            };
        }
    }

    class AirStaff07 : Staff07
    {
        public AirStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xff5fa7d7 }
            };
        }
    }

    class EarthStaff07 : Staff07
    {
        public EarthStaff07()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Staff07.Crystal, 0xffcd880b }
            };
        }
    }
}
