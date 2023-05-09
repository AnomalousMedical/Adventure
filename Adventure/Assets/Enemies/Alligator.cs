using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class Alligator : ISpriteAsset
    {
        const float SpriteWidth = 40f;
        const float SpriteHeight = 20f;
        const float SpriteStepX = 40f / SpriteWidth;
        const float SpriteStepY = 20f / SpriteHeight;

        public const uint SkinMain = 0xff547b2d;
        public const uint SkinBelly = 0xffced882;
        public const uint Tongue = 0xffd96a35;
        public const uint MouthRoof = 0xff9f8740;
        public const uint TeethClaws = 0xfffcfdec;
        public const uint Eyes = 0xffdf4c06;

        private static readonly HslColor SkinMainHsl = new IntColor(SkinMain).ToHsl();
        private static readonly HslColor SkinBellyHsl = new IntColor(SkinBelly).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Anomalous/Enemies/Alligator.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(SkinMain, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
            new SpriteMaterialTextureItem(SkinBelly, "Graphics/Textures/AmbientCG/Leather008_1K", "jpg"),
            new SpriteMaterialTextureItem(Tongue, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
            new SpriteMaterialTextureItem(MouthRoof, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
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
                { SkinBelly, IntColor.FromHslOffset(SkinBellyHsl, h, baseH).ARGB },
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
                { "default", new SpriteAnimation((long)(0.67f * Clock.SecondsToMicro), 
                new SpriteFrame(0, 0, SpriteStepX, SpriteStepY)) }
            }) { BaseScale = new Vector3(2, 1, 1) };
        }
    }
}
