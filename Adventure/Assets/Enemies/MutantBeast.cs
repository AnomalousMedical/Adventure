using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class MutantBeast : ISpriteAsset
    {
        public const uint Skin = 0xff0a5c0a;//(green)
        public const uint Horns = 0xfffa7e00;//(orange)
        public const uint Eyes = 0xff8f00fa;//(Purple)

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/mutant_beast.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Skin, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
                    new SpriteMaterialTextureItem(Horns, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                { Skin, IntColor.FromHsl(h, s, l).ARGB },
                { Horns, IntColor.FromHsl((h + 90) % 360, s, l).ARGB },
                { Eyes, IntColor.FromHsl((h + 180) % 360, s, l).ARGB }
            };
        }
    }
}
