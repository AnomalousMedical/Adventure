using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.Enemies
{
    class ThornHunter : ISpriteAsset
    {
        const float SpriteWidth = 64f;
        const float SpriteHeight = 32f;
        const float SpriteStepX = 32f / SpriteWidth;
        const float SpriteStepY = 32f / SpriteHeight;

        public const uint Rose = 0xffcc0202;//(red)
        public const uint Vines = 0xff3f5800;//(green)
        public const uint Thorns = 0xffa7bf49;//(light green)

        private static readonly HslColor RoseHsl = new IntColor(Rose).ToHsl();
        private static readonly HslColor VinesHsl = new IntColor(Vines).ToHsl();
        private static readonly HslColor ThornsHsl = new IntColor(Thorns).ToHsl();

        private const string colorMap = "Graphics/Sprites/Crawl/Enemies/thorn_hunter.png";
        private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
        {
            new SpriteMaterialTextureItem(Rose, "Graphics/Textures/AmbientCG/Fabric045_1K", "jpg"),
            new SpriteMaterialTextureItem(Vines, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
            new SpriteMaterialTextureItem(Thorns, "Graphics/Textures/AmbientCG/Fabric020_1K", "jpg"),
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
            var baseH = RoseHsl.H;

            var palletSwap = new Dictionary<uint, uint>
            {
                { Rose, IntColor.FromHslOffset(RoseHsl, h, baseH).ARGB },
                { Vines, IntColor.FromHslOffset(VinesHsl, h, baseH).ARGB },
                { Thorns, IntColor.FromHslOffset(ThornsHsl, h, baseH).ARGB },
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
                { "default", new SpriteAnimation((long)(1.37f * Clock.SecondsToMicro), new SpriteFrame(0, 0, SpriteStepX, SpriteStepY), new SpriteFrame(SpriteStepX, 0, SpriteStepX * 2, SpriteStepY)) }
            }) { BaseScale = new Vector3(1, 1, 1) };
        }
    }
}
