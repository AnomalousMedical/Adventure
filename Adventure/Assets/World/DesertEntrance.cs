using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class DesertEntrance : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/World/DesertEntrance.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff145f15, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"), //cactus (green)
            new SpriteMaterialTextureItem(0xffa4b5c2, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //arch (grey)
            new SpriteMaterialTextureItem(0xffdd7324, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //roof (orange)
            new SpriteMaterialTextureItem(0xffedb96c, "Graphics/Textures/AmbientCG/Ground025_1K", "jpg"), //ground (brown)
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
            return new Sprite() { BaseScale = new Vector3(3f, 22f / 32f * 3f, 1f) };
        }
    }
}
