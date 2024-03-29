﻿using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class ComputerDesk : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new ComputerDesk();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/ComputerDesk.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff979ca0, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), // (grey)
            new SpriteMaterialTextureItem(0xfffe3727, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff46483f, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg")
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
            return new Sprite() { BaseScale = new Vector3(32f / 21f * 1f, 1f, 1f) };
        }
    }
}
