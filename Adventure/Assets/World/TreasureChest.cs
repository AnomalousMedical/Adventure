using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World
{
    class TreasureChest : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new TreasureChest();
        private const string colorMap = "Graphics/Sprites/OpenGameArt/Bonsaiheldin/treasure_chests_32x32.png";
        protected const uint MetalColor = 0xff979ca0;
        protected const uint WoodColor1 = 0xff971a1a;
        protected const uint WoodColor2 = 0xffbe2121;
        protected const uint WoodColor3 = 0xff691212;
        protected const uint WoodColor4 = 0xff450c0c;

        private static readonly HslColor WoodHsl = new IntColor(WoodColor1).ToHsl();

        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(MetalColor, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), // (grey)
            new SpriteMaterialTextureItem(WoodColor1, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
            new SpriteMaterialTextureItem(WoodColor2, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
            new SpriteMaterialTextureItem(WoodColor3, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
            new SpriteMaterialTextureItem(WoodColor4, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
        };

        private readonly SpriteMaterialDescription defaultMaterial;

        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
        }

        public TreasureChest()
        {
            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials
            );
        }

        public TreasureChest(uint woodColor1, uint woodColor2, uint woodColor3, uint woodColor4, uint metalTrimColor)
        {
            var palletSwap = new Dictionary<uint, uint>
            {
                { WoodColor1, woodColor1 },
                { WoodColor2, woodColor2 },
                { WoodColor3, woodColor3 },
                { WoodColor4, woodColor4 },
                { MetalColor, metalTrimColor }
            };

            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: palletSwap
            );
        }

        public TreasureChest(float woodH, uint metalTrimColor)
        {
            var baseH = WoodHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { WoodColor1, IntColor.FromHslOffset(WoodHsl, woodH, baseH).ARGB },
                { WoodColor2, IntColor.FromHslOffset(WoodHsl, woodH, baseH).ARGB },
                { WoodColor3, IntColor.FromHslOffset(WoodHsl, woodH, baseH).ARGB },
                { WoodColor4, IntColor.FromHslOffset(WoodHsl, woodH, baseH).ARGB },
                { MetalColor, metalTrimColor }
            };

            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: palletSwap
            );
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 24f / 64f, 24f / 32f)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6, 25, 0, 32, 32), //Center of grip
                    }
                } )
            },
            { "open", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(24f / 64f, 0, 48f / 64f, 28f / 32f)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6, 25, 0, 32, 32), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(0.55f, 0.55f, 0.55f) };
        }
    }

    class WeaponTreasureChest : TreasureChest
    {
        public WeaponTreasureChest()
            :base(-70, TreasureChest.MetalColor) //Purple
        {

        }
    }

    class OffHandTreasureChest : TreasureChest
    {
        public OffHandTreasureChest()
            : base(-70, TreasureChest.MetalColor) //Purple
        {

        }
    }

    class AccessoryTreasureChest : TreasureChest
    {
        public AccessoryTreasureChest()
            : base(-143, TreasureChest.MetalColor) //Blue
        {

        }
    }

    class ArmorTreasureChest : TreasureChest
    {
        public ArmorTreasureChest()
            : base(120, TreasureChest.MetalColor) //Green
        {

        }
    }

    class PlotItemTreasureChest : TreasureChest
    {
        public PlotItemTreasureChest() //Default (red)
            : base()
        {

        }
    }

    class PotionTreasureChest : TreasureChest
    {
        public PotionTreasureChest() //Default (red)
            : base()
        {

        }
    }

    class StatBoostTreasureChest : TreasureChest
    {
        public StatBoostTreasureChest()
            : base(17, TreasureChest.MetalColor)
        {

        }
    }
}
