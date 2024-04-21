using DiligentEngine.RT.Sprites;
using Engine;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Book1 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Book1();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Book1.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffc77000, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Highlights
            new SpriteMaterialTextureItem(0xff725a47, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Cover
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        public ISprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(21f / 29f * 0.5f, 0.5f, 0.5f) };
        }
    }
}
