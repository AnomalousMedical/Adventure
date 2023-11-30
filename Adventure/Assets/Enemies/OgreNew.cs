using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class OgreNew : ISpriteAsset
    {
        public ISpriteAsset CreateAnotherInstance() => new OgreNew();

        const float SpriteWidth = 64f;
        const float SpriteHeight = 32f;
        const float SpriteStepX = 32f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        public const uint Skin = 0xffffb691;
        public const uint Belt = 0xff802000;//(red)
        public const uint Rags = 0xffe0a800;//(yellow)
        public const uint Eyes = 0xff7f21a1;//(purple)

        private static readonly HslColor SkinHsl = new IntColor(Skin).ToHsl();
        private static readonly HslColor BeltHsl = new IntColor(Belt).ToHsl();
        private static readonly HslColor RagsHsl = new IntColor(Rags).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        private const string colorMap = "Graphics/Sprites/Crawl/Enemies/ogre_new.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Belt, "Graphics/Textures/AmbientCG/Carpet008_1K", "jpg"),
            new SpriteMaterialTextureItem(Rags, "Graphics/Textures/AmbientCG/Fabric012_1K", "jpg"),
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
                //{ Skin, IntColor.FromHsl(h, s, l).ARGB },
                { Belt, IntColor.FromHslOffset(BeltHsl, h, baseH).ARGB },
                { Rags, IntColor.FromHslOffset(RagsHsl, h, baseH).ARGB },
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
                { "default", new SpriteAnimation((long)(1.47f * Clock.SecondsToMicro), new SpriteFrame(0, 0, SpriteStepX, SpriteStepY), new SpriteFrame(SpriteStepX, 0, SpriteStepX * 2, SpriteStepY)) }
            }) { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
