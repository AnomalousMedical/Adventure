using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class CountryEntrance : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new CountryEntrance();
        private const string colorMap = "Graphics/Sprites/Anomalous/World/CountryEntrance.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff406d1e, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"), //leaves and ground (green)
            new SpriteMaterialTextureItem(0xff7b532e, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"), //tree trunk
            new SpriteMaterialTextureItem(0xffa4b5c2, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //arch (grey)
            new SpriteMaterialTextureItem(0xff0a9210, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //roof (green)
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
            return new Sprite() { BaseScale = new Vector3(3f, 26f / 32f * 3f, 1f) };
        }
    }
}
