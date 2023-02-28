using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class Wolf : ISpriteAsset
    {
        const float SpriteWidth = 102f;
        const float SpriteHeight = 54f;
        const float SpriteStepX = 51f / SpriteWidth;
        const float SpriteStepY = 54f / SpriteHeight;

        public const uint Fur1 = 0xff5e7096;//Fur 1 (blue)
        public const uint Fur2 = 0xffe3e4e5;//Fur 2 (white)
        public const uint Fur3 = 0xffaaa8a3;//Fur 2 (grey)
        public const uint Eyes = 0xffd31033;//Eye (red)

        private static readonly HslColor Fur1Hsl = new IntColor(Fur1).ToHsl();
        private static readonly HslColor Fur2Hsl = new IntColor(Fur2).ToHsl();
        private static readonly HslColor Fur3Hsl = new IntColor(Fur3).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Anomalous/Enemies/wolf.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Fur1, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(Fur2, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(Fur3, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
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
            var baseH = Fur1Hsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Fur1, IntColor.FromHslOffset(Fur1Hsl, h, baseH).ARGB },
                { Fur2, IntColor.FromHslOffset(Fur2Hsl, h, baseH).ARGB },
                { Fur3, IntColor.FromHslOffset(Fur3Hsl, h, baseH).ARGB },
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
                { "default", new SpriteAnimation((long)(0.87f * Clock.SecondsToMicro), new SpriteFrame(0, 0, SpriteStepX, SpriteStepY), new SpriteFrame(SpriteStepX, 0, SpriteStepX * 2, SpriteStepY)) }
            }) 
            { BaseScale = new Vector3(24f / 29f, 1, 1) };
        }
    }
}
