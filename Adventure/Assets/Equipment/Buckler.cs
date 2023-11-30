using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Buckler : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Buckler();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Buckler.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffbac5d7, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Outer and inner rings (grey)
            new SpriteMaterialTextureItem(0xff80685c, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), //Body
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
            return new Sprite() { BaseScale = new Vector3(0.45f, 0.45f, 1f) };
        }
    }
}
