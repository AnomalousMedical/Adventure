using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class ThornHunter : ISpriteAsset
    {
        public const uint Rose = 0xffcc0202;//(red)
        public const uint Vines = 0xff3f5800;//(green)
        public const uint Thorns = 0xffa7bf49;//(light green)

        private static readonly HslColor RoseHsl = new IntColor(Rose).ToHsl();
        private static readonly HslColor VinesHsl = new IntColor(Vines).ToHsl();
        private static readonly HslColor ThornsHsl = new IntColor(Thorns).ToHsl();
        
        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/thorn_hunter.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Rose, "Graphics/Textures/AmbientCG/Fabric045_1K", "jpg"),
                    new SpriteMaterialTextureItem(Vines, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
                    new SpriteMaterialTextureItem(Thorns, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
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
            PalletSwap = new Dictionary<uint, uint>
            {
                { Rose, IntColor.FromHsl(h, RoseHsl.S, RoseHsl.L).ARGB },
                { Vines, IntColor.FromHsl((h + 90) % 360, VinesHsl.S, VinesHsl.L).ARGB },
                { Thorns, IntColor.FromHsl((h + 120) % 360, ThornsHsl.S, ThornsHsl.L).ARGB },
            };
        }
    }
}
