using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class TinyDino : ISpriteAsset
    {
        public const uint Skin = 0xff168516;//Skin (green)
        public const uint Spine = 0xffff0000;//Spines (red)
        public const uint Eyes = 0xff9105bd;//Eye (purple)

        private static readonly HslColor SkinHsl = new IntColor(Skin).ToHsl();
        private static readonly HslColor SpineHsl = new IntColor(Spine).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Anomalous/Enemies/TinyDino.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Skin, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
            new SpriteMaterialTextureItem(Spine, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
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
            var baseH = SkinHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Skin, IntColor.FromHslOffset(SkinHsl, h, baseH).ARGB },
                { Spine, IntColor.FromHslOffset(SpineHsl, h, baseH).ARGB },
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
            return new Sprite() { BaseScale = new Vector3(1.466666666666667f, 1, 1) };
        }
    }
}
