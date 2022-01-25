using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneTest.Assets.Original
{
    class Skeleton : ISpriteAsset
    {
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/skeletal_warrior_new.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xffd0873a, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg"),//Armor Highlight (copper)
                    new SpriteMaterialTextureItem(0xff453c31, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),//Armor (brown)
                    new SpriteMaterialTextureItem(0xffefefef, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),//Bone (almost white)
                }
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
