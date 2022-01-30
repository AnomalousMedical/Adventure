using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Original
{
    class Gargoyle : ISpriteAsset
    {
        private const uint WingDarkGray = 0xff606060;
        private const uint WingLightGray = 0xffc0c0c0;
        private const uint FaceArms = 0xffa0a0a0;
        private const uint Legs = 0xff808080;
        private const uint Belly = 0xff404040;
        private const uint Horn = 0xffffffff;
        private const uint Eyes = 0xffc00000;

        public string WingMaterial { get; set; } = "Graphics/Textures/AmbientCG/Leather008_1K";
        public string Skin1Material { get; set; } = "Graphics/Textures/AmbientCG/Rock022_1K";
        public string Skin2Material { get; set; } = "Graphics/Textures/AmbientCG/Rock022_1K";
        public string Skin3Material { get; set; } = "Graphics/Textures/AmbientCG/Leather008_1K";
        public string HornMaterial { get; set; } = "Graphics/Textures/AmbientCG/Rock022_1K";

        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/NPC/gargoyle.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(WingDarkGray, WingMaterial, "jpg"),//wing (dark gray)
                    new SpriteMaterialTextureItem(WingLightGray, WingMaterial, "jpg"),//wing (light gray)
                    new SpriteMaterialTextureItem(FaceArms, Skin1Material, "jpg"), //face, arms
                    new SpriteMaterialTextureItem(Legs, Skin2Material, "jpg"), //legs
                    new SpriteMaterialTextureItem(Belly, Skin3Material, "jpg"), //belly
                    new SpriteMaterialTextureItem(Horn, HornMaterial, "jpg"), //horn
                }
            );
        }

        public SpriteMaterialDescription CreateSwappedMaterial()
        {
            var mat = CreateMaterial();
            mat.PalletSwap = new Dictionary<uint, uint>()
            {
                { WingDarkGray, 0xff802000 },
                { WingLightGray, 0xffc03000 },
                { FaceArms, 0xffe00000 },
                { Legs, 0xffe00000 },
                { Belly, 0xffe00000 },
                { Eyes, 0xffffc000 },
                { Horn, 0xffc0c0c0 }

            };
            return mat;
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(34f / 32f, 1, 1) };
        }
    }
}
