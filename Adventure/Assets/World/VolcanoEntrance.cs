using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class VolcanoEntrance : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new DesertEntrance();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/VolcanoEntrance.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff3a1c19, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //arch (grey)
            new SpriteMaterialTextureItem(0xfff36201, "Graphics/Textures/AmbientCG/Lava004_1K", "jpg"), //lava (orange)
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
            return new Sprite() { BaseScale = new Vector3(3f, 35f / 32f * 3f, 1f) };
        }
    }
}
