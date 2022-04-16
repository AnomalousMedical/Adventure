﻿using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class SirenNew : ISpriteAsset
    {
        public const uint Hair = 0xff612d11;//(brown)
        public const uint Skin = 0xffb84f16;
        public const uint Highlight = 0xff798991;//(gray)
        public const uint Shirt = 0xff678e52;//(light green)
        public const uint Pants = 0xff156b0b;//(dark green)
        public const uint Eyes = 0xffffffff;//(white)

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/siren_new.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Hair, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
                    new SpriteMaterialTextureItem(Highlight, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                    new SpriteMaterialTextureItem(Shirt, "Graphics/Textures/AmbientCG/Fabric027_1K", "jpg"),
                    new SpriteMaterialTextureItem(Pants, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
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
                { Hair, IntColor.FromHsl(h, s, l).ARGB },
                { Highlight, IntColor.FromHsl((h + 90) % 360, s, l).ARGB },
                { Shirt, IntColor.FromHsl((h + 270) % 360, s, l).ARGB },
                { Pants, IntColor.FromHsl((h + 300) % 360, s, l).ARGB },
                { Eyes, IntColor.FromHsl((h + 180) % 360, s, l).ARGB }
            };
        }
    }
}