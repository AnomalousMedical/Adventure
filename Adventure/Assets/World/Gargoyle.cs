using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class Gargoyle : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Gargoyle();

        private const uint WingDarkGray = 0xff606060;
        private const uint WingLightGray = 0xffc0c0c0;
        private const uint FaceArms = 0xffa0a0a0;
        private const uint Legs = 0xff808080;
        private const uint Belly = 0xff404040;
        private const uint Horn = 0xffffffff;
        private const uint Eyes = 0xffc00000;

        private const string colorMap = "Graphics/Sprites/Crawl/NPC/gargoyle.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(WingDarkGray, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),//wing (dark gray)
            new SpriteMaterialTextureItem(WingLightGray, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),//wing (light gray)
            new SpriteMaterialTextureItem(FaceArms, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //face, arms
            new SpriteMaterialTextureItem(Legs, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //legs
            new SpriteMaterialTextureItem(Belly, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"), //belly
            new SpriteMaterialTextureItem(Horn, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"), //horn
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
            return new Sprite() { BaseScale = new Vector3(34f / 32f, 1, 1) };
        }
    }
}
