using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class BlacksmithShop : ISpriteAsset
    {
        const int Width = 100;
        const int Height = 100;

        public ISpriteAsset CreateAnotherInstance() => new BlacksmithShop();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/BlacksmithShop.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff714023, "Graphics/Textures/AmbientCG/RoofingTiles012A_1K-JPG", "jpg"),
            new SpriteMaterialTextureItem(0xffa0a0a0, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff472519, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff746557, "Graphics/Textures/AmbientCG/Bricks056_1K-JPG", "jpg"),
            new SpriteMaterialTextureItem(0xffb37d4a, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xfff8b514, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff304400, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
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
