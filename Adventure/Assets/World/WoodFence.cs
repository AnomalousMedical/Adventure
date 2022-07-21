using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class WoodFence : ISpriteAsset
    {
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Anomalous/World/WoodFence.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xff89612e, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
                    new SpriteMaterialTextureItem(0xffe8f1f5, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                }
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(36f / 23f * .5f, .5f, 1.0f) };
        }
    }
}
