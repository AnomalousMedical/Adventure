using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface IBiomeManager
    {
        int Count { get; }

        IBiome GetBiome(int index);
        Biome MakeChip();
    }

    class BiomeEnemy
    {
        public ISpriteAsset Asset { get; set; }

        public IEnemyCurve EnemyCurve { get; set; }

        public Dictionary<Element, Resistance> Resistances { get; set; }
    }

    class BiomeTreasure
    {
        public ISpriteAsset Asset { get; set; }
    }

    class BiomeManager : IBiomeManager
    {
        public IBiome GetBiome(int index)
        {
            switch (index)
            {
                case 0:
                    return MakeCountryside();
                case 1:
                    return MakeDesert();
                case 2:
                    return MakeSnowy();
                case 3:
                    return MakeForest();
                //No chip since it is a special zone
            }

            throw new IndexOutOfRangeException($"Index {index} is greater than the size {Count}.");
        }

        public int Count => 4; //Keeping chip out for now

        public Biome MakeSnowy()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Snow006_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock022_1K",
                ReflectFloor = false,
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BgMusic = "Music/opengameart/congusbongus - Mythica.ogg",
                BgMusicNight = "Music/opengameart/Kistol - Snowfall (Looped ver.).ogg"
            };

            return biome;
        }

        public Biome MakeDesert()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock029_1K",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BgMusic = "Music/opengameart/Fantasy_Origins - Cavernous_Desert02.ogg",
                BgMusicNight = "Music/opengameart/HorrorPen - Winds Of Stories.ogg"
            };

            return biome;
        }

        public Biome MakeCountryside()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Rocks023_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Ground037_1K",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                }
            };

            return biome;
        }

        public Biome MakeForest()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Ground042_1K",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                }
            };

            return biome;
        }

        public Biome MakeChip()
        {
            var biome = new Biome
            {
                ReflectFloor = true,
                FloorTexture = "Graphics/Textures/AmbientCG/Metal032_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Chip005_1K",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                }
            };

            return biome;
        }
    }
}
