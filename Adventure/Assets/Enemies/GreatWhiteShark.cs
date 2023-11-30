using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class GreatWhiteShark : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new GreatWhiteShark();

        const float SpriteWidth = 63f;
        const float SpriteHeight = 32f;
        const float SpriteStepX = 63f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        public const uint SkinMain = 0xff166989;
        public const uint SkinBelly = 0xfffcfcfd;
        public const uint MouthRoof = 0xff781f19;
        public const uint TeethClaws = 0xfff3e7c5;
        public const uint Eyes = 0xff5e0048;

        private static readonly HslColor SkinMainHsl = new IntColor(SkinMain).ToHsl();
        private static readonly HslColor SkinBellyHsl = new IntColor(SkinBelly).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Anomalous/Enemies/GreatWhiteShark.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(SkinMain, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(SkinBelly, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
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

        public ISprite CreateSprite()
        {
            return new Sprite(animations: new Dictionary<string, SpriteAnimation>
            {
                { "default", new SpriteAnimation((long)(0.67f * Clock.SecondsToMicro), 
                new SpriteFrame(0, 0, SpriteStepX, SpriteStepY)) }
            }) { BaseScale = new Vector3(2, 1, 1) };
        }
    }
}
