using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class Dragon : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Dragon();

        const float SpriteWidth = 131f;
        const float SpriteHeight = 97f;
        const float SpriteStepX = 131f / SpriteWidth;
        const float SpriteStepY = 97f / SpriteHeight;

        public const uint SkinMain = 0xff313346;
        public const uint SkinWing = 0xff292335;
        public const uint SkinBelly = 0xff5b111a;
        public const uint SkinBelly2 = 0xffb4672b;
        public const uint Tongue = 0xff9d0533;
        public const uint TeethClaws = 0xffffffff;
        public const uint Eyes = 0xfff84d5d;

        private static readonly HslColor SkinMainHsl = new IntColor(SkinMain).ToHsl();
        private static readonly HslColor SkinWingHsl = new IntColor(SkinWing).ToHsl();
        private static readonly HslColor SkinBellyHsl = new IntColor(SkinBelly).ToHsl();
        private static readonly HslColor SkinBelly2Hsl = new IntColor(SkinBelly2).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Anomalous/Enemies/Dragon.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(SkinMain, "Graphics/Textures/AmbientCG/MetalPlates006_1K", "jpg"),
            new SpriteMaterialTextureItem(SkinWing, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(SkinBelly, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
            new SpriteMaterialTextureItem(SkinBelly2, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
            new SpriteMaterialTextureItem(Tongue, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
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
                { SkinMain, IntColor.FromHslOffset(SkinMainHsl, h, baseH).ARGB },
                { SkinWing, IntColor.FromHslOffset(SkinWingHsl, h, baseH).ARGB },
                { SkinBelly, IntColor.FromHslOffset(SkinBellyHsl, h, baseH).ARGB },
                { SkinBelly2, IntColor.FromHslOffset(SkinBelly2Hsl, h, baseH).ARGB },
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
