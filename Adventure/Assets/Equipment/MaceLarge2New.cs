using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class MaceLarge2New : ISpriteAsset
    {
        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
                (
                    colorMap: "Graphics/Sprites/Crawl/Weapons/mace_large_2_new.png",
                    materials: new HashSet<SpriteMaterialTextureItem>
                    {
                        new SpriteMaterialTextureItem(0xff9e0000, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"), //Handle (red)
                        new SpriteMaterialTextureItem(0xffffc000, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Highlight (gold)
                        new SpriteMaterialTextureItem(0xffbfcfde, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Bludgeon (silver)
                    }
                );
        }

        public Sprite CreateSprite()
        {
            return new Sprite(new Dictionary<string, SpriteAnimation>()
                {
                    { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                        new SpriteFrame(0, 0, 1, 1)
                        {
                            Attachments = new List<SpriteFrameAttachment>()
                            {
                                SpriteFrameAttachment.FromFramePosition(7, 24, 0, 32, 32), //Center of grip
                            }
                        } )
                    },
                })
            { BaseScale = new Vector3(0.75f, 0.75f, 0.75f) };
        }
    }
}
