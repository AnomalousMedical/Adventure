using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World
{
    class GoldPile : ISpriteAsset
    {
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
                (
                     colorMap: "Graphics/Sprites/Crawl/World/gold_pile_16.png",
                    materials: new HashSet<SpriteMaterialTextureItem>
                    {
                        new SpriteMaterialTextureItem(0xffffe254, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                    }
                );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(0.75f, 0.75f, 0.75f) };
        }
    }
}
