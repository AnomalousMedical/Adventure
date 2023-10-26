using DiligentEngine.RT.Sprites;
using System.Collections.Generic;

namespace Adventure.Assets.Players;

class ClericPlayerSprite : PlayerSprite
{
    public ClericPlayerSprite()
    {
        Tier1 = new SpriteMaterialDescription
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

        Tier2 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Cleric2.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xffa1a1a1, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Grey (robe)
                new SpriteMaterialTextureItem(0xffffe254, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Highlights (gold)
                new SpriteMaterialTextureItem(0xff331c00, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Boots (brown)
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
            }
        );

        Tier3 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Cleric3.png",
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