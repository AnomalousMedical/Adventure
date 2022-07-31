using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class Cactus : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/World/Cactus.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff027304, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg")
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

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1f, 1f, 1f) };
        }
    }
}
