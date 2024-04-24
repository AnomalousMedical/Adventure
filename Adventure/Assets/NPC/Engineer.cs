using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.NPC
{
    class Engineer : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Engineer();

        private const string colorMap = "Graphics/Sprites/Anomalous/NPC/Engineer.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff2d25c6, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffa4b2b4, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff84817e, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff606060, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff787777, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xff5f3500, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
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
