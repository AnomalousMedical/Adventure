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
                    new SpriteMaterialTextureItem(0xff606060, WingMaterial, "jpg"),//wing (dark gray)
                    new SpriteMaterialTextureItem(0xffc0c0c0, WingMaterial, "jpg"),//wing (light gray)
                    new SpriteMaterialTextureItem(0xffa0a0a0, Skin1Material, "jpg"), //face, arms
                    new SpriteMaterialTextureItem(0xff808080, Skin2Material, "jpg"), //legs
                    new SpriteMaterialTextureItem(0xff404040, Skin3Material, "jpg"), //belly
                    new SpriteMaterialTextureItem(0xffffffff, HornMaterial, "jpg"), //horn
                }
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(34f / 32f, 1, 1) };
        }
    }
}
