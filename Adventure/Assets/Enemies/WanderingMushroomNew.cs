using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class WanderingMushroomNew : ISpriteAsset
    {
        public const uint Skin = 0xffffff80;//(yellowish)
        public const uint Cap = 0xffffc000;//(orange)
        public const uint CapHighlight = 0xffe20bdf;//(purple)

        private static readonly HslColor SkinHsl = new IntColor(Skin).ToHsl();
        private static readonly HslColor CapHsl = new IntColor(Cap).ToHsl();
        private static readonly HslColor CapHighlightHsl = new IntColor(CapHighlight).ToHsl();

        private const string colorMap = "Graphics/Sprites/Crawl/Enemies/wandering_mushroom_new.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Skin, "Graphics/Textures/AmbientCG/Fabric045_1K", "jpg"),
            new SpriteMaterialTextureItem(Cap, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(CapHighlight, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
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
                { Cap, IntColor.FromHslOffset(CapHsl, h, baseH).ARGB },
                { CapHighlight, IntColor.FromHslOffset(CapHighlightHsl, h, baseH).ARGB },
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
