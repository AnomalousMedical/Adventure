using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class ShieldOfReflection : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Crawl/Shields/shield_of_reflection.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffa0a0a0, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Blade (grey)
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

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(0.75f, 0.75f, 0.75f) };
        }
    }
}
