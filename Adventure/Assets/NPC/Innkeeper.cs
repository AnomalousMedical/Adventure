using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.NPC
{
    class Innkeeper : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Innkeeper();

        private const string colorMap = "Graphics/Sprites/Anomalous/NPC/Innkeeper.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff1c2a91, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff64677d, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffc3c4d1, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffaa8b63, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff7f0d0d, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
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
