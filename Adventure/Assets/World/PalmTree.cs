using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.World
{
    class PalmTree : ISpriteAsset
    {
        private const int Width = 32;
        private const int Height = 42;
        private const float WidthHeightRatio = (float)Width / (float)Height;

        public ISpriteAsset CreateAnotherInstance() => new PalmTree();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/PalmTree.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff288f31, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xffb8793a, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"),
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

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(16f, 28f, 0, Width, Height)
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations) { BaseScale = new Vector3(WidthHeightRatio * 3f, 3f, 1f) };
        }

        public int? GroundAttachmentChannel => 0;
    }
}
