using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class Portal : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/World/Portal.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xfffdf9e0, "Graphics/Textures/AmbientCG/Chip005_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xffbb73be, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff898a89, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),
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
            return new Sprite() { BaseScale = new Vector3(3f, 3f, 1f) };
        }
    }
}
