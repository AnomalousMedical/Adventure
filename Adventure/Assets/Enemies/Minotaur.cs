using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class Minotaur : ISpriteAsset
    {
        public const uint Hair = 0xffb52e26;//(red)
        public const uint FurDark = 0xff5a251f; //(dark brown)
        public const uint Highlight = 0xff9f5d41;//(light brown)
        public const uint Horns = 0xffb6b09b;//(grey)
        public const uint Snout = 0xfff1c760;//(yellow)
        public const uint Eyes = 0xfff98f3a;//(orange)

        private static readonly HslColor HairHsl = new IntColor(Hair).ToHsl();
        private static readonly HslColor FurDarkHsl = new IntColor(FurDark).ToHsl();
        private static readonly HslColor HighlightHsl = new IntColor(Highlight).ToHsl();
        private static readonly HslColor HornsHsl = new IntColor(Horns).ToHsl();
        private static readonly HslColor SnoutHsl = new IntColor(Snout).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Anomalous/Enemies/Minotaur.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Hair, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(FurDark, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(Highlight, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(Horns, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),
            new SpriteMaterialTextureItem(Snout, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
        };

        private SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        public void SetupSwap(float h, float s, float l)
        {
            var baseH = HairHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Hair, IntColor.FromHslOffset(HairHsl, h, baseH).ARGB },
                { FurDark, IntColor.FromHslOffset(FurDarkHsl, h, baseH).ARGB },
                { Highlight, IntColor.FromHslOffset(HighlightHsl, h, baseH).ARGB },
                { Horns, IntColor.FromHslOffset(HornsHsl, h, baseH).ARGB },
                { Snout, IntColor.FromHslOffset(SnoutHsl, h, baseH).ARGB },
                { Eyes, IntColor.FromHslOffset(EyesHsl, h, baseH).ARGB }
            };

            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: palletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
