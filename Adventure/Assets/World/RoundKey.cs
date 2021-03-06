using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class RoundKey : ISpriteAsset
    {
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/OpenGameArt/BizmasterStudios/RoundKey.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xff848e93, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
                },
                palletSwap: new Dictionary<uint, uint>
                {
                    { 0xff848e93, 0xffcb8e17 }
                }
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(.5f, .5f, 1.0f) };
        }
    }
}
