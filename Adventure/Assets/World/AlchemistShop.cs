using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class AlchemistShop : ISpriteAsset
    {
        const int Width = 102;
        const int Height = 103;

        public ISpriteAsset CreateAnotherInstance() => new AlchemistShop();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/AlchemistShop.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff593a22, "Graphics/Textures/AmbientCG/RoofingTiles012A_1K-JPG", "jpg"),
            new SpriteMaterialTextureItem(0xff7f6a61, "Graphics/Textures/AmbientCG/Bricks056_1K-JPG", "jpg"),
            new SpriteMaterialTextureItem(0xfff8b514, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff89531e, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffbf9f70, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffebaa36, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff7aa73c, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffc7c3c1, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff155e9f, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff71d5ff, "Graphics/Textures/AmbientCG/Chip005_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xffa411c8, "Graphics/Textures/AmbientCG/Fabric045_1K", "jpg"),
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
            return new Sprite() { BaseScale = new Vector3((float)Width / Height, 1, 1) };
        }
    }
}
