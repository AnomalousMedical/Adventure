using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class SnowyEntrance : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/World/SnowyEntrance.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff9de7b9, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"), //trees (greenish)
            new SpriteMaterialTextureItem(0xffa4b5c2, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //arch (grey)
            new SpriteMaterialTextureItem(0xff1a46cc, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //roof (blue)
            new SpriteMaterialTextureItem(0xffe9fff1, "Graphics/Textures/AmbientCG/Snow006_1K", "jpg"), //ground (white)
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

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(3f, 22f / 32f * 3f, 1f) };
        }
    }
}
