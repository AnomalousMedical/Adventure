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
    }
}
