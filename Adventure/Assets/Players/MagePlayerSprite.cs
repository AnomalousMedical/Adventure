using DiligentEngine.RT.Sprites;
using System.Collections.Generic;

namespace Adventure.Assets.Players;

class MagePlayerSprite : PlayerSprite
{
    public MagePlayerSprite()
    {
        Tier1 = new SpriteMaterialDescription
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

        Tier2 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Mage2.png",
            materials: new HashSet<SpriteMaterialTextureItem>
            {
                new SpriteMaterialTextureItem(0xffb30cb9, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
                new SpriteMaterialTextureItem(0xffb21829, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
                new SpriteMaterialTextureItem(0xff2a1903, "Graphics/Textures/AmbientCG/Leather026_1K", "jpg"),
                new SpriteMaterialTextureItem(0xff5f3500, "Graphics/Textures/AmbientCG/Leather026_1K", "jpg"),
            }
        );

        Tier3 = new SpriteMaterialDescription
        (
            colorMap: "Graphics/Sprites/Anomalous/Players/Mage3.png",
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
