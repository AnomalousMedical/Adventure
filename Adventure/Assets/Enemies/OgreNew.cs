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

        private static readonly HslColor SkinHsl = new IntColor(Skin).ToHsl();
        private static readonly HslColor BeltHsl = new IntColor(Belt).ToHsl();
        private static readonly HslColor RagsHsl = new IntColor(Rags).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

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
            var baseH = BeltHsl.H;

            PalletSwap = new Dictionary<uint, uint>
            {
                //{ Skin, IntColor.FromHsl(h, s, l).ARGB },
                { Belt, IntColor.FromHslOffset(BeltHsl, h, baseH).ARGB },
                { Rags, IntColor.FromHslOffset(RagsHsl, h, baseH).ARGB },
                { Eyes, IntColor.FromHslOffset(EyesHsl, h, baseH).ARGB }
            };
        }
    }
}
