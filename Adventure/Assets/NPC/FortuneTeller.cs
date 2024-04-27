using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.NPC
{
    class FortuneTeller : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new FortuneTeller();

        private const string colorMap = "Graphics/Sprites/Anomalous/NPC/FortuneTeller.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff7f0d0d, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff64677d, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff71778a, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xffffe254, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff0934c0, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
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
            return new Sprite() { BaseScale = new Vector3(32f / 32f, 1, 1) };
        }
    }
}
