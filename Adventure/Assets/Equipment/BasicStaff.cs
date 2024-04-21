using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class BasicStaff : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new BasicStaff();

        public const uint Staff = 0xff5b3c18;//Staff (brown)

        private const string colorMap = "Graphics/Sprites/Anomalous/Equipment/BasicStaff.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Staff, "Graphics/Textures/AmbientCG/Bark007_1K", "jpg"),
        };

        protected Dictionary<uint, uint> PalletSwap { get; set; }

        public Quaternion GetOrientation()
        {
            return Quaternion.Identity;
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: PalletSwap
            );
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(7, 32, 0, 13, 43), //Center of grip
                    }
                } )
            },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(13f / 43f, 1f, 1f) };
        }
    }
}
