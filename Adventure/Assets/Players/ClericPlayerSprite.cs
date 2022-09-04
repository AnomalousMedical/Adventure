using DiligentEngine.RT.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Players
{
    internal class ClericPlayerSprite : PlayerSprite
    {
        public ClericPlayerSprite()
        {
            SpriteMaterialDescription = new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Anomalous/Players/Cleric.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xffa1a1a1, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Grey (robe)
                    new SpriteMaterialTextureItem(0xffffe254, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Highlights (gold)
                    new SpriteMaterialTextureItem(0xff331c00, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Boots (brown)
                    new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
                }
            );
        }
    }
}
