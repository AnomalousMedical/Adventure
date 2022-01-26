using DiligentEngine.RT.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Original
{
    internal class FighterPlayerSprite : PlayerSprite
    {
        public FighterPlayerSprite()
        {
            SpriteMaterialDescription = new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/LastGuardian/Players/avt3_full.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xff0054a8, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Helmet (blue)
                    new SpriteMaterialTextureItem(0xffffff00, "Graphics/Textures/AmbientCG/Fabric027_1K", "jpg"), //Coat (yellow)
                    new SpriteMaterialTextureItem(0xffff8000, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Armor (orange)
                }
            );
        }
    }
}
