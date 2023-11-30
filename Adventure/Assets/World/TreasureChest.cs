﻿using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World
{
    class TreasureChest : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new TreasureChest();
        private const string colorMap = "Graphics/Sprites/OpenGameArt/Bonsaiheldin/treasure_chests_32x32.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff979ca0, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), // (grey)
            new SpriteMaterialTextureItem(0xff971a1a, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
            new SpriteMaterialTextureItem(0xffbe2121, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
            new SpriteMaterialTextureItem(0xff691212, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
            new SpriteMaterialTextureItem(0xff450c0c, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), // (red)
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 24f / 64f, 24f / 32f)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6, 25, 0, 32, 32), //Center of grip
                    }
                } )
            },
            { "open", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(24f / 64f, 0, 48f / 64f, 28f / 32f)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6, 25, 0, 32, 32), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(0.55f, 0.55f, 0.55f) };
        }
    }
}
