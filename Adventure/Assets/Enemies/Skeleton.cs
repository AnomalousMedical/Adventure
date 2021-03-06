using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class Skeleton : ISpriteAsset
    {
        public const uint ArmorHighlight = 0xffd0873a;//Armor Highlight (copper)
        public const uint Armor = 0xff453c31;//Armor (brown)
        public const uint Bone = 0xffefefef;//Bone (almost white)
        public const uint Eyes = 0xffbd0000;//(red)

        private static readonly HslColor ArmorHighlightHsl = new IntColor(ArmorHighlight).ToHsl();
        private static readonly HslColor ArmorHsl = new IntColor(Armor).ToHsl();
        private static readonly HslColor BoneHsl = new IntColor(Bone).ToHsl();
        private static readonly HslColor EyesHsl = new IntColor(Eyes).ToHsl();

        public Dictionary<uint, uint> PalletSwap { get; set; }
        public SpriteMaterialDescription CreateMaterial()
        {
            return new SpriteMaterialDescription
            (
                colorMap: "Graphics/Sprites/Crawl/Enemies/skeletal_warrior_new.png",
                materials: new HashSet<SpriteMaterialTextureItem>
                {
                    new SpriteMaterialTextureItem(ArmorHighlight, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg"),
                    new SpriteMaterialTextureItem(Armor, "Graphics/Textures/AmbientCG/Leather001_1K", "jpg"),
                    new SpriteMaterialTextureItem(Bone, "Graphics/Textures/AmbientCG/Rock022_1K", "jpg"),
                },
                palletSwap: PalletSwap
            );
        }

        public Sprite CreateSprite()
        {
            return new Sprite() { BaseScale = new Vector3(1, 1, 1) };
        }

        public void SetupSwap(float h, float s, float l)
        {
            var baseH = ArmorHsl.H;

            PalletSwap = new Dictionary<uint, uint>
            {
                { Armor, IntColor.FromHslOffset(ArmorHsl, h, baseH).ARGB },
                { ArmorHighlight, IntColor.FromHslOffset(ArmorHighlightHsl, h, baseH).ARGB },
            };
        }
    }
}
