using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class SalamanderFirebrand : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new SalamanderFirebrand();

        const float SpriteWidth = 64f;
        const float SpriteHeight = 32f;
        const float SpriteStepX = 32f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        public const uint Horns = 0xffffd400;//(yellow)
        public const uint Skin = 0xff7f4415;//(brown)
        public const uint Chest = 0xff760a1a;//(red)
        public const uint Eyes = 0xff66ffef;//(blue)

        private static readonly HslColor HornsHsl = new IntColor(Horns).ToHsl();
        private static readonly HslColor SkinHsl = new IntColor(Skin).ToHsl();
        private static readonly HslColor ChestHsl = new IntColor(Chest).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Crawl/Enemies/salamander_firebrand.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Horns, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(Skin, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
            new SpriteMaterialTextureItem(Chest, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
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
            var baseH = SkinHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Skin, IntColor.FromHslOffset(SkinHsl, h, baseH).ARGB },
                { Horns, IntColor.FromHslOffset(HornsHsl, h, baseH).ARGB },
                { Chest, IntColor.FromHslOffset(ChestHsl, h, baseH).ARGB },
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
                { "default", new SpriteAnimation((long)(1.27f * Clock.SecondsToMicro), new SpriteFrame(0, 0, SpriteStepX, SpriteStepY), new SpriteFrame(SpriteStepX, 0, SpriteStepX * 2, SpriteStepY)) }
            }) { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
