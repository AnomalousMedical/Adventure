using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class OrcKnightOld : ISpriteAsset
    {
        const float SpriteWidth = 64f;
        const float SpriteHeight = 32f;
        const float SpriteStepX = 32f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        public const uint Tusks = 0xffe0e0e0;//(white)
        public const uint Skin = 0xff156b0b;//(green)
        public const uint Belt = 0xfff2c44d;//(gold)
        public const uint Armor = 0xff404040;//(gray)
        public const uint Eyes = 0xffc00000;//(red)

        private static readonly HslColor TusksHsl = new IntColor(Tusks).ToHsl();
        private static readonly HslColor SkinHsl = new IntColor(Skin).ToHsl();
        private static readonly HslColor BeltHsl = new IntColor(Belt).ToHsl();
        private static readonly HslColor ArmorHsl = new IntColor(Armor).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Crawl/Enemies/orc_knight_old.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Tusks, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),
            new SpriteMaterialTextureItem(Belt, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
            new SpriteMaterialTextureItem(Armor, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
            var baseH = BeltHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Skin, IntColor.FromHslOffset(SkinHsl, h, baseH).ARGB },
                { Armor, IntColor.FromHslOffset(ArmorHsl, h, baseH).ARGB },
                { Belt, IntColor.FromHslOffset(BeltHsl, h, baseH).ARGB },
                { Eyes, IntColor.FromHslOffset(EyesHsl, h, baseH).ARGB }
            };

            defaultMaterial = new SpriteMaterialDescription
            (
                colorMap: colorMap,
                materials: materials,
                palletSwap: palletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite(animations: new Dictionary<string, SpriteAnimation>
            {
                { "default", new SpriteAnimation((long)(1.47f * Clock.SecondsToMicro), new SpriteFrame(0, 0, SpriteStepX, SpriteStepY), new SpriteFrame(SpriteStepX, 0, SpriteStepX * 2, SpriteStepY)) }
            }) { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
