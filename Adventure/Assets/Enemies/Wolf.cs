using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class Wolf : ISpriteAsset
    {
        public const uint Fur1 = 0xff5e7096;//Fur 1 (blue)
        public const uint Fur2 = 0xffe3e4e5;//Fur 2 (white)
        public const uint Fur3 = 0xffaaa8a3;//Fur 2 (grey)
        public const uint Eyes = 0xffd31033;//Eye (red)

        private static readonly HslColor Fur1Hsl = new IntColor(Fur1).ToHsl();
        private static readonly HslColor Fur2Hsl = new IntColor(Fur2).ToHsl();
        private static readonly HslColor Fur3Hsl = new IntColor(Fur3).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        public string Fur1Material { get; set; } = "Graphics/Textures/AmbientCG/Carpet008_1K";
        public string Fur2Material { get; set; } = "Graphics/Textures/AmbientCG/Carpet008_1K";
        public string Fur3Material { get; set; } = "Graphics/Textures/AmbientCG/Carpet008_1K";
        public Dictionary<uint, uint> PalletSwap { get; set; }

        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Anomalous/Enemies/wolf.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Fur1, Fur1Material, "jpg"),
                    new SpriteMaterialTextureItem(Fur2, Fur2Material, "jpg"),
                    new SpriteMaterialTextureItem(Fur3, Fur3Material, "jpg"),
                },
                palletSwap: PalletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(24f / 29f, 1, 1) };
        }

        public void SetupSwap(float h, float s, float l)
        {
            var baseH = Fur1Hsl.H;

            PalletSwap = new Dictionary<uint, uint>
            {
                { Fur1, IntColor.FromHslOffset(Fur1Hsl, h, baseH).ARGB },
                { Fur2, IntColor.FromHslOffset(Fur2Hsl, h, baseH).ARGB },
                { Fur3, IntColor.FromHslOffset(Fur3Hsl, h, baseH).ARGB },
                { Eyes, IntColor.FromHslOffset(EyesHsl, h, baseH).ARGB }
            };
        }
    }
}
