using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class OgreNew : ISpriteAsset
    {
        public const uint Skin = 0xffffb691;
        public const uint Belt = 0xff802000;//(red)
        public const uint Rags = 0xffe0a800;//(yellow)
        public const uint Eyes = 0xff7f21a1;//(purple)

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/ogre_new.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Belt, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
                    new SpriteMaterialTextureItem(Rags, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
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
                //{ Skin, IntColor.FromHsl(h, s, l).ARGB },
                { Belt, IntColor.FromHsl((h + 90) % 360, s, l).ARGB },
                { Rags, IntColor.FromHsl((h + 270) % 360, s, l).ARGB },
                { Eyes, IntColor.FromHsl((h + 180) % 360, s, l).ARGB }
            };
        }
    }
}
