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

        public const uint Leaves = 0xff288f31;
        public const uint Trunk = 0xffb8793a;

        private static readonly HslColor LeavesHsl = new IntColor(Leaves).ToHsl();

        public ISpriteAsset CreateAnotherInstance() => new PalmTree();

        private const string colorMap = "Graphics/Sprites/Anomalous/World/PalmTree.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Leaves, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(Trunk, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"),
        };

        private readonly SpriteMaterialDescription defaultMaterial;

        public PalmTree()
        {
            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials
            );
        }

        public PalmTree(float h, float s, float l)
        {
            var baseH = LeavesHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Leaves, IntColor.FromHslOffset(LeavesHsl, h, baseH).ARGB }
            };

            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: palletSwap
            );
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

        public class Swap1 : PalmTree
        {
            static float h = new IntColor(0xff288f3d).ToHsl().H;

            public Swap1()
                : base(h, 100, 50)
            {

            }
        }

        public class Swap2 : PalmTree
        {
            static float h = new IntColor(0xff498d00).ToHsl().H;

            public Swap2()
                : base(h, 100, 50)
            {

            }
        }
    }
}
