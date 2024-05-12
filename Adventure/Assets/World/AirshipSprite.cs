using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World
{
    static class AirshipSprite
    {
        const float SpriteWidth = 76f;
        const float SpriteHeight = 99f;

        private const string colorMap = "Graphics/Sprites/Anomalous/Airship.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xffacadac, "Graphics/Textures/AmbientCG/Fabric049_1K", "jpg"), //Balloon
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

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 33f / SpriteHeight))
            },
            { "left", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 33f / SpriteHeight, 1, 66f / SpriteHeight))
            },
            { "down", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(1f / SpriteWidth, 66f / SpriteHeight, 32f / SpriteWidth, 99f / SpriteHeight))
            },
            { "up", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(44f / SpriteWidth, 66f / SpriteHeight, 75f / SpriteWidth, 99f / SpriteHeight))
            },
        };

        public static ISprite CreateSprite()
        {
            return new Sprite(animations) { BaseScale = new Vector3(76f / 33f * 2.5f, 2.5f, 1) };
        }

        public static Vector3 GetScale(string animation)
        {
            switch (animation)
            {
                case "up":
                case "down":
                    return new Vector3(31f / 33f * 2.5f, 2.5f, 1);
                default:
                    return new Vector3(76f / 33f * 2.5f, 2.5f, 1);
            }
        }
    }
}
