﻿using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.PixelEffects
{
    class BuffMagicEffect : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new BuffMagicEffect();

        private const string colorMap = "Graphics/Sprites/vfx-free-pack/magicbuff_281x213.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>();
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
            { "default", new SpriteAnimation((int)(16f * Clock.MilliToMicroseconds), SpriteBuilder.CreateAnimatedSprite(281, 213, 9, 9 * 7)) },
        };

        public ISprite CreateSprite()
        {
            return new Sprite(animations)
            { BaseScale = new Vector3(281f / 213f, 1, 1) };
        }
    }
}
