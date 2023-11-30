using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class Skeleton : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new Skeleton();

        const float SpriteWidth = 64f;
        const float SpriteHeight = 32f;
        const float SpriteStepX = 32f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        public const uint ArmorHighlight = 0xffd0873a;//Armor Highlight (copper)
        public const uint Armor = 0xff453c31;//Armor (brown)
        public const uint Bone = 0xffefefef;//Bone (almost white)
        public const uint Eyes = 0xffbd0000;//(red)

        private static readonly HslColor ArmorHighlightHsl = new IntColor(ArmorHighlight).ToHsl();
        private static readonly HslColor ArmorHsl = new IntColor(Armor).ToHsl();
        private static readonly HslColor BoneHsl = new IntColor(Bone).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Crawl/Enemies/skeletal_warrior_new.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(ArmorHighlight, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg"),
            new SpriteMaterialTextureItem(Armor, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
            new SpriteMaterialTextureItem(Bone, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),
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
            var baseH = ArmorHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Armor, IntColor.FromHslOffset(ArmorHsl, h, baseH).ARGB },
                { ArmorHighlight, IntColor.FromHslOffset(ArmorHighlightHsl, h, baseH).ARGB },
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
                { "default", new SpriteAnimation((long)(1.53f * Clock.SecondsToMicro), new SpriteFrame(0, 0, SpriteStepX, SpriteStepY), new SpriteFrame(SpriteStepX, 0, SpriteStepX * 2, SpriteStepY)) }
            }) { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
