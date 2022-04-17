using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class SalamanderFirebrand : ISpriteAsset
    {
        public const uint Horns = 0xffffd400;//(yellow)
        public const uint Skin = 0xff7f4415;//(brown)
        public const uint Chest = 0xff760a1a;//(red)
        public const uint Eyes = 0xff66ffef;//(blue)

        private static readonly HslColor HornsHsl = new IntColor(Horns).ToHsl();
        private static readonly HslColor SkinHsl = new IntColor(Skin).ToHsl();
        private static readonly HslColor ChestHsl = new IntColor(Chest).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/salamander_firebrand.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Horns, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                    new SpriteMaterialTextureItem(Skin, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
                    new SpriteMaterialTextureItem(Chest, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
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
            var baseH = SkinHsl.H;

            PalletSwap = new Dictionary<uint, uint>
            {
                { Skin, IntColor.FromHslOffset(SkinHsl, h, baseH).ARGB },
                { Horns, IntColor.FromHslOffset(HornsHsl, h, baseH).ARGB },
                { Chest, IntColor.FromHslOffset(ChestHsl, h, baseH).ARGB },
                { Eyes, IntColor.FromHslOffset(EyesHsl, h, baseH).ARGB }
            };
        }
    }
}
