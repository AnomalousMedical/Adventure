using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class MountainLion : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new MountainLion();

        const float SpriteWidth = 61f;
        const float SpriteHeight = 33f;
        const float SpriteStepX = 61f / SpriteWidth;
        const float SpriteStepY = 33f / SpriteHeight;

        public const uint FurMain = 0xffcd9150;
        public const uint FurBelly = 0xfffef8d9;
        public const uint Nose = 0xff680a0a;
        public const uint TeethClaws = 0xffffffff;
        public const uint Eyes = 0xffcc00fb;

        private static readonly HslColor SkinMainHsl = new IntColor(FurMain).ToHsl();
        private static readonly HslColor SkinBellyHsl = new IntColor(FurBelly).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Anomalous/Enemies/MountainLion.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(FurMain, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(FurBelly, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(Nose, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
            new SpriteMaterialTextureItem(TeethClaws, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),
        };

        private SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
        (
            colorMap: colorMap,
            materials: materials
        );

        public SpriteMaterialDescription CreateMaterial()
        {
            return defaultMaterial;
        }

        public void SetupSwap(float h, float s, float l)
        {
            var baseH = SkinMainHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { FurMain, IntColor.FromHslOffset(SkinMainHsl, h, baseH).ARGB },
                { FurBelly, IntColor.FromHslOffset(SkinBellyHsl, h, baseH).ARGB },
                { Eyes, IntColor.FromHslOffset(EyesHsl, h, baseH).ARGB }
            };

            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: palletSwap
            );
        }

        public ISprite CreateSprite()
        {
            return new Sprite(animations: new Dictionary<string, SpriteAnimation>
            {
                { "default", new SpriteAnimation((long)(0.67f * Clock.SecondsToMicro), 
                new SpriteFrame(0, 0, SpriteStepX, SpriteStepY)) }
            }) { BaseScale = new Vector3(SpriteWidth / SpriteHeight, 1, 1) };
        }
    }
}
