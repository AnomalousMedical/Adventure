using Adventure.Assets;
using Adventure.Assets.World;
using RpgMath;
using System;
using System.Collections.Generic;

namespace Adventure
{
    enum BiomeType
    {
        Countryside = 0,
        Desert = 1,
        Snowy = 2,
        Forest = 3,
        Beach = 4,
        Swamp = 5,
        Max = Swamp + 1,
        Volcano = 1000, //This isn't included in random selection
    }

    interface IBiomeManager
    {
        IBiome GetBiome(BiomeType type);
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
        public IBiome GetBiome(BiomeType type)
        {
            switch (type)
            {
                case BiomeType.Countryside:
                    return MakeCountryside();
                case BiomeType.Desert:
                    return MakeDesert();
                case BiomeType.Snowy:
                    return MakeSnowy();
                case BiomeType.Forest:
                    return MakeForest();
                case BiomeType.Beach:
                    return MakeBeach();
                case BiomeType.Swamp:
                    return MakeSwamp();
                case BiomeType.Volcano:
                    return MakeVolcano();
            }

            throw new IndexOutOfRangeException($"Biome type '{type}' is not supported.");
        }

        public Biome MakeSnowy()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Snow006_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Snow004_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Snow003_1K",
                ReflectFloor = false,
                EntranceAsset = new SnowyEntrance(),
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BgMusic = "Music/opengameart/Kistol - Snowfall (Looped ver.).ogg",
                BgMusicNight = "Music/opengameart/Kistol - Snowfall (Looped ver.).ogg",
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(10, new PineTree())
                }
            };

            return biome;
        }

        public Biome MakeDesert()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Rocks008_1K",
                FloorTexture2 = "Graphics/Textures/AmbientCG/Ground033_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Ground033_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Ground035_1K",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BgMusic = "Music/freepd/Desert Fox Underscore - Rafael Krux.ogg",
                BgMusicNight = "Music/freepd/Desert Fox Underscore - Rafael Krux.ogg",
                EntranceAsset = new DesertEntrance(),
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(2, new Cactus())
                }
            };

            return biome;
        }

        public Biome MakeCountryside()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                FloorTexture2 = "Graphics/Textures/AmbientCG/Ground067_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Ground037_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Grass004_1K",
                BgMusic = "Music/opengameart/Youre Perfect Studio - gone_fishin_by_memoraphile_CC0.ogg",
                BgMusicNight = "Music/opengameart/Youre Perfect Studio - gone_fishin_by_memoraphile_CC0.ogg",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(10, new Tree())
                }
            };

            return biome;
        }

        public Biome MakeBeach()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground060_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Ground027_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Rocks013_1K",
                BgMusic = "Music/freepd/Kevin MacLeod - Pickled Pink.ogg",
                BgMusicNight = "Music/freepd/Kevin MacLeod - Pickled Pink.ogg",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(10, new PalmTree())
                }
            };

            return biome;
        }

        public Biome MakeSwamp()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                FloorTexture2 = "Graphics/Textures/AmbientCG/Ground067_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Moss001_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Ground023_1K",
                BgMusic = "Music/freepd/Bryan Teoh - Murder On The Bayou.ogg",
                BgMusicNight = "Music/freepd/Bryan Teoh - Murder On The Bayou.ogg",
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(10, new BanyanTree())
                }
            };

            return biome;
        }

        public Biome MakeForest()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                FloorTexture2 = "Graphics/Textures/AmbientCG/Ground049C_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Ground042_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Ground023_1K",
                BgMusic = "Music/opengameart/HorrorPen - Winds Of Stories.ogg",
                BgMusicNight = "Music/opengameart/HorrorPen - Winds Of Stories.ogg",
                EntranceAsset = new ForestEntrance(),
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(20, new TallTree())
                }
            };

            return biome;
        }

        public Biome MakeVolcano()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Rock031_1K",
                FloorTexture2 = "Graphics/Textures/AmbientCG/Ground031_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock037_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Lava003_1K",
                BgMusic = "Music/opengameart/congusbongus - Mythica.ogg",
                BgMusicNight = "Music/opengameart/congusbongus - Mythica.ogg",
                EntranceAsset = new ForestEntrance(),
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    //new BiomeBackgroundItem(1, new ComputerDesk()),
                    //new BiomeBackgroundItem(3, new Tree())
                },
                MaxBackgroundItemRoll = 250
            };

            return biome;
        }
    }
}
