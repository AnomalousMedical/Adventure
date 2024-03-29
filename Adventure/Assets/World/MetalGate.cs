﻿using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class MetalGate : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new MetalGate();

        private const string colorMap = "Graphics/Sprites/OpenGameArt/bee/MetalGate.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff7d7071, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        public ISprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(85f / 26f, 1.0f, 1.0f) };
        }
    }
}
