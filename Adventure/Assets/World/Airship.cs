using DiligentEngine.RT.Sprites;
using Engine;
using System.Collections.Generic;

namespace Adventure.Assets.World
{
    static class Airship
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/Airship.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffacadac, "Graphics/Textures/AmbientCG/Fabric045_1K", "jpg"), //Balloon
            new SpriteMaterialTextureItem(0xff727172, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Armor
            new SpriteMaterialTextureItem(0xff6d0a22, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), //Hull
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public static SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        public static Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(76f / 32f * 2.5f, 2.5f, 1) };
        }
    }
}
