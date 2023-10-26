using DiligentEngine.RT.Sprites;
using System.Collections.Generic;

namespace Adventure.Assets.Players;

class ThiefPlayerSprite : PlayerSprite
{
    public ThiefPlayerSprite()
    {
        Tier1 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Thief.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xff5f3500, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Brown (armor)
                new SpriteMaterialTextureItem(0xff0e463f, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Green (coat)
                new SpriteMaterialTextureItem(0xffab1a21, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"), //Bandanna (red)
                new SpriteMaterialTextureItem(0xff727272, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Goggles (grey)
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
            }
        );

        Tier2 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Thief2.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xff5f3500, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Brown (armor)
                new SpriteMaterialTextureItem(0xff0e463f, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Green (coat)
                new SpriteMaterialTextureItem(0xffab1a21, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"), //Bandanna (red)
                new SpriteMaterialTextureItem(0xff727272, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Goggles (grey)
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
            }
        );

        Tier3 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Thief3.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xff5f3500, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Brown (armor)
                new SpriteMaterialTextureItem(0xff0e463f, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"), //Green (coat)
                new SpriteMaterialTextureItem(0xffab1a21, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"), //Bandanna (red)
                new SpriteMaterialTextureItem(0xff727272, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Goggles (grey)
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Belt (brown)
            }
        );
    }
}