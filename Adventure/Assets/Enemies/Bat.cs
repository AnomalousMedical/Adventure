﻿using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class Bat : ISpriteAsset
    {
        public const uint Fur = 0xff855e49;//(brown)
        public const uint Eyes = 0xffff0000;//(Red)

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/bat.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(Fur, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
                },
                palletSwap: PalletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1, 1, 1) };
        }

        public void SetupSwap(float h, float s, float l, IPallet pallet)
        { 
            PalletSwap = new Dictionary<uint, uint>
            {
                { Fur, IntColor.FromHsl(h, s, l).ClosestRgb(pallet.Colors).ARGB },
                { Eyes, IntColor.FromHsl((h + 180) % 360, s, l).ClosestRgb(pallet.Colors).ARGB }
            };
        }
    }
}
