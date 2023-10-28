using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class FinalSpear : ISpriteAsset
    {
        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/FinalSpear.png";
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
                    }
                },
                new SpriteFrame(1f / 3f, 0, 2f / 3f, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(5.5f, 43f, 0, 11, 55), //Center of grip
                    }
                },
                new SpriteFrame(2f / 3f, 0f, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(5.5f, 43f, 0, 11, 55), //Center of grip
                    }
                })
            },
        };

        public ISprite CreateSprite()
        {
            return new KeepTimeSprite(animations)
            { BaseScale = new Vector3(11f / 55f * 1.45f, 1.45f, 1.0f) };
        }
    }
}
