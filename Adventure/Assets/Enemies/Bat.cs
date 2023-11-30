using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class Bat : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Bat();

        const float SpriteWidth = 64f;
        const float SpriteHeight = 32f;
        const float SpriteStepX = 32f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        public const uint Fur = 0xff855e49;//(brown)
        public const uint Eyes = 0xffff0000;//(Red)

        private static readonly HslColor FurHsl = new IntColor(Fur).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Crawl/Enemies/bat.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Fur, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
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
            var baseH = FurHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Fur, IntColor.FromHslOffset(FurHsl, h, baseH).ARGB },
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
                { "default", new SpriteAnimation((long)(0.67f * Clock.SecondsToMicro), new SpriteFrame(0, 0, SpriteStepX, SpriteStepY), new SpriteFrame(SpriteStepX, 0, SpriteStepX * 2, SpriteStepY)) }
            }) { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
