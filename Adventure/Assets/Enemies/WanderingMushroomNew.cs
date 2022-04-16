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
        public const uint CapHighlight = 0xffff4000;//(red)

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/wandering_mushroom_new.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Skin, "Graphics/Textures/AmbientCG/Fabric045_1K", "jpg"),
                    new SpriteMaterialTextureItem(Cap, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
                    new SpriteMaterialTextureItem(CapHighlight, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
                },
                palletSwap: PalletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1, 1, 1) };
        }

        public void SetupSwap(float h, float s, float l, IPallet pallet)
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Skin, IntColor.FromHsl(h, s, l).ClosestRgb(pallet.Colors).ARGB },
                { Cap, IntColor.FromHsl((h + 90) % 360, s, l).ClosestRgb(pallet.Colors).ARGB },
                { CapHighlight, IntColor.FromHsl((h + 270) % 360, s, l).ClosestRgb(pallet.Colors).ARGB },
            };
        }
    }
}
