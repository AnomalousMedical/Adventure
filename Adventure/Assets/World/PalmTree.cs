using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class PalmTree : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new PalmTree();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/PalmTree.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff288f31, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffb8793a, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"),
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
            return new Sprite() { BaseScale = new Vector3(3f, 3f, 1f) };
        }
    }
}
