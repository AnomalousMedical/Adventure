using DiligentEngine.RT.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Players
{
    internal class MagePlayerSprite : PlayerSprite
    {
        public MagePlayerSprite()
        {
            SpriteMaterialDescription = new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Anomalous/Players/Mage.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xffb30cb9, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
                    new SpriteMaterialTextureItem(0xffb21829, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
                    new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather026_1K", "jpg"),
                    new SpriteMaterialTextureItem(0xff5f3500, "Graphics/Textures/AmbientCG/Leather026_1K", "jpg"),
                }
            );
        }
    }
}
