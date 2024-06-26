﻿using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Spear3 : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Spear3();

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/Spear3.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff3a2922, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff675e61, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                new SpriteFrame(0, 0, 1f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(5.5f, 43f, 0, 11, 55), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(5, 17, -0.1f, 11, 55), //Light position
                    }
                },
                new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(5.5f, 43f, 0, 11, 55), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(5, 17, -0.1f, 11, 55), //Light position
                    }
                },
                new SpriteFrame(2f / 3f, 0f, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(5.5f, 43f, 0, 11, 55), //Center of grip
                        SpriteFrameAttachment.FromFramePosition(5, 17, -0.1f, 11, 55), //Light position
                    }
                })
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            {
                BaseScale = new Vector3(11f / 55f * 1.45f, 1.45f, 1.0f),
                KeepTime = true
            };
        }

        public Light CreateLight()
        {
            return new Light()
            {
                Color = Color.FromRGB(0xffd800),
                Length = 0.7f,
            };
        }

        public int? LightAttachmentChannel => 1;
    }
}
