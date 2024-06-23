using Adventure.Assets;
using Adventure.Assets.SoundEffects;
using Adventure.Assets.World;
using DiligentEngine.RT.Resources;
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
        Mountain = 6,
        Volcano = 1000, //This isn't included in random selection
        FinalBoss = 30000
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

        public ISoundEffect AttackSound { get; set; }

        public IEnumerable<BattleStats.SkillInfo> Skills { get; set; }
    }

    class BiomeTreasure
    {
        public ISpriteAsset Weapon { get; set; }
        public ISpriteAsset OffHand { get; set; }
        public ISpriteAsset Accessory { get; set; }
        public ISpriteAsset Armor { get; set; }
        public ISpriteAsset PlotItem { get; set; }
        public ISpriteAsset StatBoost { get; set; }
        public ISpriteAsset Potion { get; set; }

        public ISpriteAsset GetTreasureAsset(TreasureType type)
        {
            switch(type)
            {
                case TreasureType.Weapon:
                    return Weapon;
                case TreasureType.OffHand:
                    return OffHand;
                case TreasureType.Accessory:
                    return Accessory;
                case TreasureType.Armor:
                    return Armor;
                case TreasureType.PlotItem:
                    return PlotItem;
                case TreasureType.StatBoost:
                    return StatBoost;
                case TreasureType.Potion:
                    return Potion;
            }

            throw new InvalidOperationException($"Cannot get asset for treasure type '{type}'.");
        }
    }

    class BiomeManager : IBiomeManager
    {
        private readonly TerrainNoise terrainNoise;

        public BiomeManager(TerrainNoise terrainNoise)
        {
            this.terrainNoise = terrainNoise;
        }

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
                case BiomeType.Mountain:
                    return MakeMountain();
                case BiomeType.Volcano:
                    return MakeVolcano();
            }

            throw new IndexOutOfRangeException($"Biome type '{type}' is not supported.");
        }

        private BiomeTreasure MakeDefaultTreasure()
        {
            return new BiomeTreasure()
            {
                Weapon = new WeaponTreasureChest(),
                OffHand = new OffHandTreasureChest(),
                Accessory = new AccessoryTreasureChest(),
                Armor = new ArmorTreasureChest(),
                PlotItem = new PlotItemTreasureChest(),
                StatBoost = new StatBoostTreasureChest(),
                Potion = new PotionTreasureChest(),
            };
        }

        public Biome MakeSnowy()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Snow006_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Snow004_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Snow003_1K",
                ReflectFloor = false,
                Treasure = MakeDefaultTreasure(),
                BgMusic = "Music/opengameart/Kistol - Snowfall (Looped ver.).ogg",
                BgMusicNight = "Music/opengameart/Kistol - Snowfall (Looped ver.).ogg",
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(4, new PineTree(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(7, new PineTree.Swap1(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(10, new PineTree.Swap2(), 0.8f, 0.4f),
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
                Treasure = MakeDefaultTreasure(),
                BgMusic = "Music/freepd/Desert Fox Underscore - Rafael Krux.ogg",
                BgMusicNight = "Music/freepd/Desert Fox Underscore - Rafael Krux.ogg",
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(2, new Cactus(), 0.8f, 0.4f)
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
                Treasure = MakeDefaultTreasure(),
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(4, new Tree(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(7, new Tree.Swap1(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(10, new Tree.Swap2(), 0.8f, 0.4f),
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
                Treasure = MakeDefaultTreasure(),
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(4, new PalmTree(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(7, new PalmTree.Swap1(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(10, new PalmTree.Swap2(), 0.8f, 0.4f),
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
                Treasure = MakeDefaultTreasure(),
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(4, new BanyanTree(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(7, new BanyanTree.Swap1(), 0.8f, 0.4f),
                    new BiomeBackgroundItem(10, new BanyanTree.Swap2(), 0.8f, 0.4f),
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
                Treasure = MakeDefaultTreasure(),
                BackgroundItems = new List<BiomeBackgroundItem>
                {
                    new BiomeBackgroundItem(10, new TallTree(), 0.9f, 0.2f, 0.5f),
                    new BiomeBackgroundItem(20, new TallTree.Swap1(), 0.9f, 0.2f, 0.5f),
                }
            };

            return biome;
        }

        public Biome MakeMountain()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                FloorTexture2 = "Graphics/Textures/AmbientCG/Ground049C_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock023_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Rock026_1K",
                BgMusic = "Music/freepd/Rafael Krux - Lonely Mountain.ogg",
                BgMusicNight = "Music/freepd/Rafael Krux - Lonely Mountain.ogg",
                MapUnitY = 0.8f,
                RandomizeMapUnitYDirection = false,
                Treasure = MakeDefaultTreasure(),
                BackgroundItems = new List<BiomeBackgroundItem>
                {

                }
            };

            return biome;
        }

        public Biome MakeVolcano()
        {
            var biome = new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Lava003_1K",
                FloorTexture2 = "Graphics/Textures/AmbientCG/Ground031_1K",
                WallTexture2 = "Graphics/Textures/AmbientCG/Rock037_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Lava004_1K",
                BgMusic = "Music/freepd/Kevin MacLeod - Evil Incoming.ogg",
                BgMusicNight = "Music/freepd/Kevin MacLeod - Evil Incoming.ogg",
                EntranceAsset = new VolcanoEntrance(),
                Treasure = MakeDefaultTreasure(),
                BackgroundItems = new List<BiomeBackgroundItem>
                {

                },
                MaxBackgroundItemRoll = 250,
                CreateNoise = seed => terrainNoise.CreateLavaNoise(seed),
                MapUnitY = 0.4f,
                RandomizeMapUnitYDirection = false,
                CorridorSlopeMultiple = -1.0f
            };

            return biome;
        }
    }
}
