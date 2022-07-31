using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class TallTree : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/World/TallTree.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff008d00, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff834d36, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"),
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials,
            textureScale: 16
        );

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1f, 1024f / 32f * 1f, 1f) };
        }
    }
}
