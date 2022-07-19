using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class FancyBook : ISpriteAsset
    {
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Anomalous/Equipment/FancyBook.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(0xffe3fc02, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Highlights (gold)
                    new SpriteMaterialTextureItem(0xff8a0f83, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Cover (purple)
                    //new SpriteMaterialTextureItem(0xff32e6f9, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Pages (blue)
                }
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(21f / 29f * 0.5f, 0.5f, 0.5f) };
        }
    }
}
