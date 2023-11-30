using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class ForestEntrance : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new ForestEntrance();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/ForestEntrance.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff0e3d32, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff89d860, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"), 
            new SpriteMaterialTextureItem(0xff6c2c17, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"), //tree trunk (brown)
            new SpriteMaterialTextureItem(0xffa4b5c2, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //arch (grey)
            new SpriteMaterialTextureItem(0xffdd3c24, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //roof (red)
            new SpriteMaterialTextureItem(0xff834d36, "Graphics/Textures/AmbientCG/Ground042_1K", "jpg"), //ground (brown)
            new SpriteMaterialTextureItem(0xffedb96c, "Graphics/Textures/AmbientCG/Ground025_1K", "jpg"), //ground path (brown)
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
