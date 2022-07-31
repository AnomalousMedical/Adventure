﻿using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Equipment
{
    class Greatsword01 : ISpriteAsset
    {
        public Quaternion GetOrientation()
        {
            return new Quaternion(0, MathFloat.PI / 4f, 0);
        }

        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
                (
                    colorMap: "Graphics/Sprites/Crawl/Weapons/greatsword_01.png",
                    //colorMap: "opengameart/Dungeon Crawl Stone Soup Full/misc/cursor_red.png",
                    materials: new HashSet<SpriteMaterialTextureItem>
                    {
                        new SpriteMaterialTextureItem(0xff802000, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"), //Hilt (brown)
                        new SpriteMaterialTextureItem(0xffadadad, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Blade (grey)
                        new SpriteMaterialTextureItem(0xff5e5e5f, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Blade (grey)
                        new SpriteMaterialTextureItem(0xffe4ac26, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true), //Blade (grey)
                    }
                );
        }

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation((int)(0.7f * Clock.SecondsToMicro),
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(6, 25, 0, 32, 32), //Center of grip
                    }
                } )
            },
        };

        public Sprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(0.75f, 0.75f, 0.75f) };
        }
    }
}
