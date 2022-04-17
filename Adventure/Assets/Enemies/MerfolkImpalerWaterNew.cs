using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class MerfolkImpalerWaterNew : ISpriteAsset
    {
        public const uint Tail = 0xff940700;//(red)
        public const uint Armor = 0xfff6d4a4;//(yellow)
        public const uint Shoulders = 0xffb84f16;//(orange)
        public const uint HelmetHighlight = 0xff852bc8;//(purple)

        private static readonly HslColor TailHsl = new IntColor(Tail).ToHsl();
        private static readonly HslColor ArmorHsl = new IntColor(Armor).ToHsl();
        private static readonly HslColor ShouldersHsl = new IntColor(Shoulders).ToHsl();
        private static readonly HslColor HelmetHighlightHsl = new IntColor(HelmetHighlight).ToHsl();

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/merfolk_impaler_water_new.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Tail, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
                    new SpriteMaterialTextureItem(Armor, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                    new SpriteMaterialTextureItem(Shoulders, "Graphics/Textures/AmbientCG/Fabric027_1K", "jpg"),
                    new SpriteMaterialTextureItem(HelmetHighlight, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
                },
                palletSwap: PalletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1, 1, 1) };
        }

        public void SetupSwap(float h, float s, float l)
        {
            var baseH = TailHsl.H;

            PalletSwap = new Dictionary<uint, uint>
            {
                { Tail, IntColor.FromHslOffset(TailHsl, h, baseH).ARGB },
                { Armor, IntColor.FromHslOffset(ArmorHsl, h, baseH).ARGB },
                { Shoulders, IntColor.FromHslOffset(ShouldersHsl, h, baseH).ARGB },
                { HelmetHighlight, IntColor.FromHslOffset(HelmetHighlightHsl, h, baseH).ARGB }
            };
        }
    }
}
