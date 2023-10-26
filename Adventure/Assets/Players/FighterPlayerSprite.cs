using DiligentEngine.RT.Sprites;
using System.Collections.Generic;

namespace Adventure.Assets.Players;

class FighterPlayerSprite : PlayerSprite
{
    public FighterPlayerSprite()
    {
        Tier1 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Fighter.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xff8a001f, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Helmet (red)
                new SpriteMaterialTextureItem(0xff2f37a6, "Graphics/Textures/AmbientCG/Fabric027_1K", "jpg"), //Coat (purple)
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
                new SpriteMaterialTextureItem(0xff727272, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Armor (grey)
            }
        );

        Tier2 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Fighter2.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xff8a001f, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Helmet (red)
                new SpriteMaterialTextureItem(0xff2f37a6, "Graphics/Textures/AmbientCG/Fabric027_1K", "jpg"), //Coat (purple)
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
                new SpriteMaterialTextureItem(0xff727272, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Armor (grey)
            }
        );

        Tier3 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Fighter3.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xff8a001f, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Helmet (red)
                new SpriteMaterialTextureItem(0xff2f37a6, "Graphics/Textures/AmbientCG/Fabric027_1K", "jpg"), //Coat (purple)
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
                new SpriteMaterialTextureItem(0xff727272, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Armor (grey)
            }
        );
    }
}