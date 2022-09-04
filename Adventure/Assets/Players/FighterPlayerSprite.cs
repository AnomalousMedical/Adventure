using DiligentEngine.RT.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Players
{
    internal class FighterPlayerSprite : PlayerSprite
    {
        public FighterPlayerSprite()
        {
            SpriteMaterialDescription = new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Anomalous/Players/Fighter.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xff0054a8, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Helmet (blue)
                    new SpriteMaterialTextureItem(0xff2f37a6, "Graphics/Textures/AmbientCG/Fabric027_1K", "jpg"), //Coat (purple)
                    new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
                    new SpriteMaterialTextureItem(0xff727272, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Armor (grey)
                }
            );
        }
    }
}
