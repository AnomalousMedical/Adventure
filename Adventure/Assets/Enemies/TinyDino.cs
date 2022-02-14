using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Original
{
    class TinyDino : ISpriteAsset
    {
        public const uint Skin = 0xff168516;//Skin (green)
        public const uint Spine = 0xffff0000;//Spines (red)

        //Threax drew this one

        public string SkinMaterial { get; set; } = "Graphics/Textures/AmbientCG/Leather008_1K";
        public string SpineMaterial { get; set; } = "Graphics/Textures/AmbientCG/Leather008_1K";
        public Dictionary<uint, uint> PalletSwap { get; set; }

        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Anomalous/Enemies/TinyDino.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Skin, SkinMaterial, "jpg"),
                    new SpriteMaterialTextureItem(Spine, SpineMaterial, "jpg"),
                },
                palletSwap: PalletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1.466666666666667f, 1, 1) };
        }
    }
}
