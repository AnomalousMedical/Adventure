using DiligentEngine.RT.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Original
{
    internal class MagePlayerSprite : PlayerSprite
    {
        public MagePlayerSprite()
        {
            SpriteMaterialDescription = new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/LastGuardian/Players/amg1_full4.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xffa854ff, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
                    new SpriteMaterialTextureItem(0xff909090, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
                    new SpriteMaterialTextureItem(0xff8c4800, "Graphics/Textures/AmbientCG/Leather026_1K", "jpg"),
                    new SpriteMaterialTextureItem(0xffffe254, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                }
            );
        }
    }
}
