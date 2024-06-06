using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class Inn : ISpriteAsset
    {
        const int Width = 100;
        const int Height = 100;

        public ISpriteAsset CreateAnotherInstance() => new Inn();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/Inn.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff643028, "Graphics/Textures/AmbientCG/RoofingTiles012A_1K-JPG", "jpg"),
            new SpriteMaterialTextureItem(0xfff8b514, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff6e4530, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff463328, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff2c1f1c, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffdfc59c, "Graphics/Textures/AmbientCG/Bricks056_1K-JPG", "jpg"),
            new SpriteMaterialTextureItem(0xff81797a, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff7e6a5a, "Graphics/Textures/AmbientCG/Bricks056_1K-JPG", "jpg"),
            new SpriteMaterialTextureItem(0xff7d787a, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
