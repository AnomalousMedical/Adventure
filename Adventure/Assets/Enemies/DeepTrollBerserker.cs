﻿using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class DeepTrollBerserker : ISpriteAsset
    {
        public const uint Skin = 0xff0080ff;//(blue)
        public const uint SkinHighlight = 0xff006030;//(green)
        public const uint Rags = 0xffdc4416;//(orange)
        public const uint Eyes = 0xffac1010;//(red)

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/deep_troll_berserker.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Skin, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
                    new SpriteMaterialTextureItem(SkinHighlight, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
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
                { Skin, IntColor.FromHsl(h, s, l).ARGB },
                { SkinHighlight, IntColor.FromHsl((h + 30) % 360, s, l).ARGB },
                { Rags, IntColor.FromHsl((h + 270) % 360, s, l).ARGB },
                { Eyes, IntColor.FromHsl((h + 180) % 360, s, l).ARGB }
            };
        }
    }
}