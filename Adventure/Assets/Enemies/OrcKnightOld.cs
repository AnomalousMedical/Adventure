using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class OrcKnightOld : ISpriteAsset
    {
        public const uint Tusks = 0xffe0e0e0;//(white)
        public const uint Skin = 0xff156b0b;//(green)
        public const uint Belt = 0xfff2c44d;//(gold)
        public const uint Armor = 0xff404040;//(gray)
        public const uint Eyes = 0xffc00000;//(red)

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/orc_knight_old.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Tusks, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),
                    new SpriteMaterialTextureItem(Belt, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                    new SpriteMaterialTextureItem(Armor, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                { Armor, IntColor.FromHsl((h + 90) % 360, s, l).ARGB },
                { Belt, IntColor.FromHsl((h + 270) % 360, s, l).ARGB },
                { Eyes, IntColor.FromHsl((h + 180) % 360, s, l).ARGB }
            };
        }
    }
}
