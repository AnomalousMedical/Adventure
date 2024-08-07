﻿using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World
{
    class HelpBookAsset : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new HelpBookAsset();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/HelpBook.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff842755, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xfff9d354, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(0xffef3109, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public Quaternion GetOrientation()
        {
            return Quaternion.Identity;
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(12, 18, 0, 23, 32), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(23f / 32f * 0.55f, 0.55f, 1.0f) };
        }
    }
}
