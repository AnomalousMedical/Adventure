using Adventure.Services;
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
    class TallTree : ISpriteAsset
    {
        private const int Width = 77;
        private const int Height = 892;
        private const float WidthHeightRatio = (float)Width / (float)Height;

        public ISpriteAsset CreateAnotherInstance() => new TallTree();
        private const string colorMap = "Graphics/Sprites/Anomalous/World/TallTree.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(0xff008d00, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(0xff834d36, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"),
        };

        private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials,
            textureScale: 16
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
                        new SpriteFrameAttachment(new Vector3((Width / 2f) / Width, (842f - Height) / Height, 0))
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations) { BaseScale = new Vector3(WidthHeightRatio * 11.5f, 11.5f, 1f) };
        }

        public int? GroundAttachmentChannel => 0;
    }
}
