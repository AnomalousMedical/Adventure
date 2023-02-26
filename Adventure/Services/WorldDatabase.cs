using Adventure.Battle.Skills;
using Adventure.Items.Creators;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Services
{
    interface IWorldDatabase
    {
        int GetZoneSeed(int index);
        IAreaBuilder GetAreaBuilder(int zoneIndex);
        int GetLevelDelta(int area);
        void Reset(int newSeed);

        IBiomeManager BiomeManager { get; }
        SwordCreator SwordCreator { get; }
        SpearCreator SpearCreator { get; }
        MaceCreator MaceCreator { get; }
        ShieldCreator ShieldCreator { get; }
        ElementalStaffCreator ElementalStaffCreator { get; }
        AccessoryCreator AccessoryCreator { get; }
        ArmorCreator ArmorCreator { get; }
        PotionCreator PotionCreator { get; }
        DaggerCreator DaggerCreator { get; }
        IMonsterMaker MonsterMaker { get; }
        WorldMapData WorldMap { get; }
        List<IAreaBuilder> AreaBuilders { get; }
        List<IntVector2> PortalLocations { get; }
        int CurrentSeed { get; }
        IntVector2 AirshipStartSquare { get; }
        IntVector2 AirshipPortalSquare { get; }
        BookCreator BookCreator { get; }
    }

    class WorldDatabase : IWorldDatabase
    {
        private List<IAreaBuilder> areaBuilders;
        private List<int> createdZoneSeeds;
        private int currentSeed;
        private FIRandom zoneRandom;
        private readonly Persistence persistence;
        private List<IntVector2> portalLocations;
        private IntVector2 airshipStartSquare;
        private IntVector2 airshipPortalSquare;

        public IBiomeManager BiomeManager { get; }
        public SwordCreator SwordCreator { get; }
        public SpearCreator SpearCreator { get; }
        public MaceCreator MaceCreator { get; }
        public ShieldCreator ShieldCreator { get; }
        public ElementalStaffCreator ElementalStaffCreator { get; }
        public AccessoryCreator AccessoryCreator { get; }
        public ArmorCreator ArmorCreator { get; }
        public PotionCreator PotionCreator { get; }
        public DaggerCreator DaggerCreator { get; }
        public BookCreator BookCreator { get; }
        public IMonsterMaker MonsterMaker { get; }
        public List<IntVector2> PortalLocations => portalLocations;
        public IntVector2 AirshipStartSquare => airshipStartSquare;
        public IntVector2 AirshipPortalSquare => airshipPortalSquare;

        public int GetLevelDelta(int currentLevel)
        {
            //Based off 20 areas
            var delta = 5;
            if (currentLevel + delta > 99)
            {
                delta = 99 - currentLevel;
            }

            return delta;
        }

        private WorldMapData worldMap;
        public WorldMapData WorldMap => worldMap;

        public List<IAreaBuilder> AreaBuilders => areaBuilders;

        public int CurrentSeed => persistence.Current.World.Seed;

        class ZoneCounter
        {
            private int index;

            public int GetZoneStart()
            {
                return index;
            }

            public int GetZoneEnd(int numZones)
            {
                var zoneDelta = numZones - 1;
                var endZone = index + zoneDelta;
                index += numZones;
                return endZone;
            }
        }

        public WorldDatabase
        (
            Persistence persistence,
            IMonsterMaker monsterMaker,
            IBiomeManager biomeManager,
            SwordCreator swordCreator,
            SpearCreator spearCreator,
            MaceCreator maceCreator,
            ShieldCreator shieldCreator,
            ElementalStaffCreator elementalStaffCreator,
            AccessoryCreator accessoryCreator,
            ArmorCreator armorCreator,
            PotionCreator potionCreator,
            DaggerCreator daggerCreator,
            BookCreator bookCreator
        )
        {
            this.persistence = persistence;
            this.MonsterMaker = monsterMaker;
            BiomeManager = biomeManager;
            SwordCreator = swordCreator;
            SpearCreator = spearCreator;
            MaceCreator = maceCreator;
            ShieldCreator = shieldCreator;
            ElementalStaffCreator = elementalStaffCreator;
            AccessoryCreator = accessoryCreator;
            ArmorCreator = armorCreator;
            PotionCreator = potionCreator;
            DaggerCreator = daggerCreator;
            BookCreator = bookCreator;
        }

        public int GetZoneSeed(int zoneIndex)
        {
            var end = zoneIndex + 1;
            for (var i = createdZoneSeeds.Count; i < end; ++i)
            {
                createdZoneSeeds.Add(zoneRandom.Next(int.MinValue, int.MaxValue));
            }
            return createdZoneSeeds[zoneIndex];
        }

        public IAreaBuilder GetAreaBuilder(int zoneIndex)
        {
            foreach (var area in areaBuilders)
            {
                if (zoneIndex >= area.StartZone && zoneIndex <= area.EndZone)
                {
                    return area;
                }
            }

            return areaBuilders[0];
        }

        public void Reset(int newSeed)
        {
            //Setup seeds and randoms
            createdZoneSeeds = new List<int>();
            zoneRandom = new FIRandom(newSeed);
            var biomeRandom = new FIRandom(newSeed);
            var placementRandom = new FIRandom(newSeed);
            var elementalRandom = new FIRandom(newSeed);
            var treasureRandom = new FIRandom(newSeed);
            currentSeed = newSeed;

            //Setup map
            worldMap = new WorldMapData(newSeed);
            var numIslands = 1  //Phase 0, 1
                            + 2  //Phase 2
                            + 4  //Phase 3
                            + 1  //End zone
                            + 1  //Endless corridor
                            + 1; //Airship
            worldMap.Map.RemoveExtraIslands(numIslands);
            var map = worldMap.Map;
            //TODO: need to check maps
            //3 largest islands need to have enough spaces for each phase
            //World needs enough islands to cover all zones

            //Setup areas
            var usedSquares = new bool[map.MapX, map.MapY];
            var usedIslands = new bool[map.NumIslands];
            portalLocations = new List<IntVector2>(5);

            //Reserve the 3 largest islands
            usedIslands[map.IslandSizeOrder[0]] = true;
            usedIslands[map.IslandSizeOrder[1]] = true;
            usedIslands[map.IslandSizeOrder[2]] = true;

            SetupAirshipIsland(placementRandom, out airshipStartSquare, out airshipPortalSquare, usedSquares, usedIslands, map);
            areaBuilders = SetupAreaBuilder(newSeed, biomeRandom, placementRandom, elementalRandom, treasureRandom, portalLocations, usedSquares, usedIslands, map).ToList();
        }

        private static void SetupAirshipIsland(FIRandom placementRandom, out IntVector2 airshipSquare, out IntVector2 airshipPortalSquare, bool[,] usedSquares, bool[] usedIslands, csIslandMaze map)
        {
            //Airship Island
            var islandIndex = map.IslandSizeOrder[map.NumIslands - 1];
            var island = map.IslandInfo[islandIndex];
            usedIslands[islandIndex] = true;
            airshipSquare = GetUnusedSquare(usedSquares, island, placementRandom, island.Eastmost);
            usedSquares[airshipSquare.x, airshipSquare.y] = true;
            airshipPortalSquare = GetUnusedSquare(usedSquares, island, placementRandom, island.Westmost);
            usedSquares[airshipPortalSquare.x, airshipPortalSquare.y] = true;
        }

        private IntVector2 GetSquare(List<IntVector2> items, FIRandom random)
        {
            var index = random.Next(items.Count);
            var square = items[index];
            items.RemoveAt(index);
            return square;
        }

        private IEnumerable<IAreaBuilder> SetupAreaBuilder(int seed, FIRandom biomeRandom, FIRandom placementRandom, FIRandom elementalRandom, FIRandom treasureRandom, List<IntVector2> portalLocations, bool[,] usedSquares, bool[] usedIslands, csIslandMaze map)
        {
            var starterBiomes = new List<BiomeType>() { BiomeType.Desert, BiomeType.Forest, BiomeType.Snowy };

            var monsterInfo = MonsterMaker.CreateBaseMonsters(seed);
            var elementalMonsters = new Dictionary<Element, List<MonsterInfo>>()
            {
                { Element.Fire, MonsterMaker.CreateElemental(seed, Element.Fire) },
                { Element.Ice, MonsterMaker.CreateElemental(seed, Element.Ice) },
                { Element.Electricity, MonsterMaker.CreateElemental(seed, Element.Electricity) }
            };

            var filled = new bool[map.MapX, map.MapY];
            int area = 0;
            AreaBuilder areaBuilder;
            var zoneCounter = new ZoneCounter();
            var biomeMax = (int)BiomeType.Max;

            AddPortal(map.IslandInfo[map.IslandSizeOrder[0]], usedSquares, placementRandom, portalLocations);
            AddPortal(map.IslandInfo[map.IslandSizeOrder[1]], usedSquares, placementRandom, portalLocations);
            AddPortal(map.IslandInfo[map.IslandSizeOrder[2]], usedSquares, placementRandom, portalLocations);

            var island = map.IslandInfo[map.IslandSizeOrder[0]];
            var firstIslandSquares = new List<IntVector2>
            {
                //Northmost is the teleporter
                GetUnusedSquare(usedSquares, island, placementRandom, island.Westmost),
                GetUnusedSquare(usedSquares, island, placementRandom, island.Eastmost),
                GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost),
            };

            //Phase 0
            {
                var startingBiome = BiomeType.Countryside;
                var phase0TreasureLevel = 1;
                var phase0Adjective = "Busted";
                var firstBoss = monsterInfo.Where(i => i.NativeBiome == startingBiome).First();
                var bossResistance = firstBoss.Resistances.Where(i => i.Value == Resistance.Weak && i.Key > Element.MagicStart && i.Key < Element.MagicEnd);
                var startingElementStaff = bossResistance.Any() ? bossResistance.Select(i => i.Key).First() : Element.Fire;
                string[] spells;
                string staffName = $"{phase0Adjective} ";
                switch (startingElementStaff)
                {
                    case Element.Ice:
                        spells = new []{ nameof(Ice) };
                        staffName += "Ice";
                        break;
                    case Element.Fire:
                        spells = new[] { nameof(Fire) };
                        staffName += "Fire";
                        break;
                    default:
                        spells = new[] { nameof(Zap) };
                        staffName += "Electrical";
                        break;
                }
                var phase0UniqueTreasures = new List<Treasure>();
                phase0UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase0TreasureLevel, phase0Adjective)));
                phase0UniqueTreasures.Add(new Treasure(ElementalStaffCreator.CreateNormal(phase0TreasureLevel, staffName, spells)));
                phase0UniqueTreasures.Add(new Treasure(BookCreator.CreateRestoration(phase0TreasureLevel, phase0Adjective, nameof(Cure))));
                phase0UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase0TreasureLevel, phase0Adjective)));
                phase0UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase0TreasureLevel, phase0Adjective)));

                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)));

                phase0UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase0TreasureLevel, phase0Adjective, 60)));
                phase0UniqueTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase0TreasureLevel, phase0Adjective)));

                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase0TreasureLevel, phase0Adjective)));
                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase0TreasureLevel, phase0Adjective)));
                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase0TreasureLevel, phase0Adjective)));
                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase0TreasureLevel, phase0Adjective)));

                //Area 1
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 0;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = startingBiome;
                areaBuilder.BossMonster = firstBoss;
                areaBuilder.Location = GetSquare(firstIslandSquares, placementRandom);
                areaBuilder.Treasure = phase0UniqueTreasures;
                areaBuilder.StartEnd = true;
                areaBuilder.MaxMainCorridorBattles = 1;
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;
            }

            //Phase 1
            {
                var phase1TreasureLevel = 30;
                var phase1Adjective = "Common";
                var phase1UniqueTreasures = new List<Treasure>();
                phase1UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueTreasures.Add(new Treasure(BookCreator.CreateRestoration(phase1TreasureLevel, phase1Adjective, nameof(Cure))));
                phase1UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase1TreasureLevel, phase1Adjective, 70)));
                phase1UniqueTreasures.Add(new Treasure(ElementalStaffCreator.CreateNormal(phase1TreasureLevel, "Scholar's", nameof(Fire), nameof(Ice), nameof(Zap))));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));

                var phase1UniqueStolenTreasures = new List<Treasure>();
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueStolenTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase1TreasureLevel, phase1Adjective)));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));

                var uniqueTreasure = phase1UniqueTreasures.Count / 2;
                var stolenTreasure = phase1UniqueStolenTreasures.Count / 2;

                //Area 2
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 1;
                areaBuilder.IndexInPhase = 0;
                var biomeIndex = biomeRandom.Next(0, starterBiomes.Count);
                areaBuilder.Biome = starterBiomes[biomeIndex];
                starterBiomes.RemoveAt(biomeIndex);
                areaBuilder.Location = GetSquare(firstIslandSquares, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase1UniqueTreasures, treasureRandom, uniqueTreasure)
                    .Concat(new[] 
                    { 
                        new Treasure(PotionCreator.CreateHealthPotion(phase1TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase1UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase1TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase1TreasureLevel))
                };
                areaBuilder.PlotItem = PlotItems.PortalKey0;
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;

                //Area 3
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 1;
                areaBuilder.IndexInPhase = 1;
                biomeIndex = biomeRandom.Next(0, starterBiomes.Count);
                areaBuilder.Biome = starterBiomes[biomeIndex];
                starterBiomes.RemoveAt(biomeIndex);
                areaBuilder.Location = GetSquare(firstIslandSquares, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase1UniqueTreasures, treasureRandom, phase1UniqueTreasures.Count) //Last area gets remaining treasure
                    .Concat(new[] 
                    { 
                        new Treasure(PotionCreator.CreateHealthPotion(phase1TreasureLevel))
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase1UniqueStolenTreasures, treasureRandom, phase1UniqueStolenTreasures.Count); //Last area gets remaining treasure
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase1TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase1TreasureLevel))
                };
                areaBuilder.PlotItem = PlotItems.PortalKey1;
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;
            }

            //Phase 2
            {
                //TODO: This is really just a placeholder
                var phase2TreasureLevel = 50;
                var phase2Adjective = "Quality";
                var phase2UniqueTreasures = new List<Treasure>();
                phase2UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueTreasures.Add(new Treasure(BookCreator.CreateRestoration(phase2TreasureLevel, phase2Adjective, nameof(Cure), nameof(MegaCure))));
                phase2UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, 80)));
                phase2UniqueTreasures.Add(new Treasure(ElementalStaffCreator.CreateNormal(phase2TreasureLevel, "Mage's", nameof(Fire), nameof(FireBlast), nameof(Ice), nameof(IceBlast), nameof(Zap), nameof(ZapBlast))));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateCounterAttack()));
                phase2UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateHealing(phase2Adjective, 0.2f)));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));

                var phase2UniqueStolenTreasures = new List<Treasure>();
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueStolenTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase2TreasureLevel, phase2Adjective)));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));

                var uniqueTreasure = phase2UniqueTreasures.Count / 3;
                var stolenTreasure = phase2UniqueStolenTreasures.Count / 3;

                //2nd island
                island = map.IslandInfo[map.IslandSizeOrder[1]];

                var secondIslandSquares = new List<IntVector2>
                {
                    //Northmost is the teleporter
                    GetUnusedSquare(usedSquares, island, placementRandom, island.Westmost),
                    GetUnusedSquare(usedSquares, island, placementRandom, island.Eastmost),
                    GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost),
                };

                //Area 4
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.PlotItem = PlotItems.AirshipKey0;
                areaBuilder.Biome = starterBiomes[0];
                areaBuilder.Monsters = elementalMonsters[GetElementForBiome(areaBuilder.Biome)]
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetSquare(secondIslandSquares, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase2UniqueTreasures, treasureRandom, uniqueTreasure)
                    .Concat(new[] 
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase2TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase2UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel))
                };
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;

                //Area 5
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.PlotItem = PlotItems.AirshipKey1;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
                areaBuilder.Monsters = elementalMonsters[GetElementForBiome(areaBuilder.Biome)]
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetSquare(secondIslandSquares, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase2UniqueTreasures, treasureRandom, uniqueTreasure)
                    .Concat(new[] 
                    { 
                        new Treasure(PotionCreator.CreateHealthPotion(phase2TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase2UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel))
                };
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;

                //3rd island
                island = map.IslandInfo[map.IslandSizeOrder[2]];

                var thirdIslandSquares = new List<IntVector2>
                {
                    //Northmost is the teleporter
                    GetUnusedSquare(usedSquares, island, placementRandom, island.Westmost),
                    GetUnusedSquare(usedSquares, island, placementRandom, island.Eastmost),
                    GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost),
                };

                //Area 6
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.PlotItem = PlotItems.AirshipKey2;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
                areaBuilder.Monsters = elementalMonsters[GetElementForBiome(areaBuilder.Biome)]
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetSquare(thirdIslandSquares, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase2UniqueTreasures, treasureRandom, phase2UniqueTreasures.Count) //Last area gets remaining treasure
                    .Concat(new[]
                    { 
                        new Treasure(PotionCreator.CreateHealthPotion(phase2TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase2UniqueStolenTreasures, treasureRandom, phase2UniqueStolenTreasures.Count); //Last area gets remaining treasure
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel))
                };
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;
            }

            //Phase 3
            {
                //TODO: This is really just a placeholder
                var phase3TreasureLevel = 65;
                var phase3Adjective = "Superior";
                var phase3UniqueTreasures = new List<Treasure>();
                phase3UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(BookCreator.CreateRestoration(phase3TreasureLevel, "Arch Mage's", nameof(Fire), nameof(FireBlast), nameof(FireLash), nameof(Ice), nameof(IceBlast), nameof(IceLash), nameof(Zap), nameof(ZapBlast), nameof(ZapLash))));
                phase3UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, 90)));
                phase3UniqueTreasures.Add(new Treasure(ElementalStaffCreator.CreateNormal(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateItemUsage(phase3Adjective, 0.5f)));
                phase3UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateHealing(phase3Adjective, 0.3f)));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));

                var phase3UniqueStolenTreasures = new List<Treasure>();
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueStolenTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase3TreasureLevel, phase3Adjective)));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));

                var uniqueTreasure = phase3UniqueTreasures.Count / 4;
                var stolenTreasure = phase3UniqueStolenTreasures.Count / 4;

                Element firstMonsterElement;
                //Area 7
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase3UniqueTreasures, treasureRandom, uniqueTreasure)
                    .Concat(new[] 
                    { 
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel))
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;

                //Area 8
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase3UniqueTreasures, treasureRandom, uniqueTreasure)
                    .Concat(new[] 
                    { 
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel))
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;

                //Area 9
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase3UniqueTreasures, treasureRandom, uniqueTreasure)
                    .Concat(new[] 
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel))
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;

                //Area 10
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 3;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure = RemoveRandomItems(phase3UniqueTreasures, treasureRandom, phase3UniqueTreasures.Count) //Last area gets the remaining treasure
                    .Concat(new[] 
                    { 
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel)) 
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, phase3UniqueStolenTreasures.Count); //Last area gets the remaining treasure
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel))
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;
            }

            //Phase 4
            {
                var phase4TreasureLevel = 70;
                var phase4Adjective = "Flawless";

                //Area 11
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(2);
                areaBuilder.Phase = 3;
                areaBuilder.EnemyLevel = 51;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.Biome = BiomeType.Chip;
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);

                //You get all the monsters in this zone
                areaBuilder.Monsters = monsterInfo;
                foreach(var monsters in elementalMonsters.Values)
                {
                    areaBuilder.Monsters = areaBuilder.Monsters.Concat(monsters);
                }
                areaBuilder.Monsters = areaBuilder.Monsters.ToList();

                //TODO: Specify the boss and some treasure
                //areaBuilder.Treasure = RemoveRandomItems(phase3UniqueTreasures, treasureRandom, uniqueTreasure)
                //    .Concat(new[]
                //    {
                //        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel))
                //    });
                //areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                //areaBuilder.StealTreasure = new[]
                //{
                //    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel)),
                //    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel))
                //};
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;
            }
        }

        private static Element GetRandomMagicElement(FIRandom elementalRandom, params Element[] except)
        {
            var elements = Enum.GetValues<Element>().Where(i => i > Element.MagicStart && i < Element.MagicEnd && !except.Contains(i)).ToArray();
            if(elements.Length == 0)
            {
                throw new InvalidOperationException("No elements left to select from");
            }

            return elements[elementalRandom.Next(0, elements.Length)];
        }

        private static Element GetElementForBiome(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Snowy:
                    return Element.Ice;
                case BiomeType.Countryside:
                    return Element.Fire;
                case BiomeType.Forest:
                    return Element.Electricity;
                case BiomeType.Desert:
                    return Element.Fire;
                default:
                    return Element.Fire;
            }
        }

        private static void AddPortal(IslandInfo island, bool[,] usedSquares, FIRandom placementRandom, List<IntVector2> portalLocations)
        {
            var square = GetUnusedSquare(usedSquares, island, placementRandom, island.Northmost);
            portalLocations.Add(square);
        }

        private static IntVector2 GetUnusedSquare(bool[,] usedSquares, IslandInfo island, FIRandom placementRandom, IntVector2 desired)
        {
            if (!usedSquares[desired.x, desired.y])
            {
                usedSquares[desired.x, desired.y] = true;
                return desired;
            }

            return GetUnusedSquare(usedSquares, island, placementRandom);
        }

        private static IntVector2 GetUnusedSquare(bool[,] usedSquares, IslandInfo island, FIRandom placementRandom)
        {
            for (var i = 0; i < 5; ++i)
            {
                var next = placementRandom.Next(0, island.Size);
                var square = island.islandPoints[next];
                if (!usedSquares[square.x, square.y])
                {
                    usedSquares[square.x, square.y] = true;
                    return square;
                }
            }

            foreach (var square in island.islandPoints)
            {
                if (!usedSquares[square.x, square.y])
                {
                    usedSquares[square.x, square.y] = true;
                    return square;
                }
            }

            //This should not happen
            throw new InvalidOperationException($"Cannot find unused point on island {island.Id} out of possible {island.Size}");
        }

        private static int GetUnusedIsland(bool[] usedIslands, FIRandom placementRandom)
        {
            for (var i = 0; i < 5; ++i)
            {
                var next = placementRandom.Next(0, usedIslands.Length);
                if (!usedIslands[next])
                {
                    usedIslands[next] = true;
                    return next;
                }
            }

            for (int i = 0; i < usedIslands.Length; ++i)
            {
                if (!usedIslands[i])
                {
                    usedIslands[i] = true;
                    return i;
                }
            }

            //This should not happen
            throw new InvalidOperationException($"Cannot find unused island {usedIslands.Length}");
        }

        private static int GetBiomeIndex(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Chip:
                    return (int)BiomeType.Max + 2; //Max is the stand in for the chip zone + 2 for the cliff and sea floor
                default:
                    return (int)biome;
            }
        }

        private static void SetIslandBiome(IslandInfo island, csIslandMaze map, BiomeType biome)
        {            
            foreach (var square in island.islandPoints)
            {
                map.TextureOffsets[square.x, square.y] = GetBiomeIndex(biome);
            }
        }

        private static void FillSurroundings(csIslandMaze map, BiomeType biome, IntVector2 startPoint, bool[,] filled)
        {
            //The start point will always be filled out even if its already filled
            map.TextureOffsets[startPoint.x, startPoint.y] = GetBiomeIndex(biome);
            filled[startPoint.x, startPoint.y] = true;

            var nextGeneration = new List<IntVector2>(25);
            var currentGeneration = new List<IntVector2>(25) { startPoint };

            for (var gen = 0; gen < 4 && currentGeneration.Count > 0; ++gen)
            {
                foreach (var item in currentGeneration)
                {
                    //Check each dir
                    var check = item;
                    ++check.y;
                    if (check.y < map.MapY && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = GetBiomeIndex(biome);
                        filled[check.x, check.y] = true;
                    }

                    check = item;
                    --check.y;
                    if (check.y > 0 && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = GetBiomeIndex(biome);
                        filled[check.x, check.y] = true;
                    }

                    check = item;
                    ++check.x;
                    if (check.x < map.MapX && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = GetBiomeIndex(biome);
                        filled[check.x, check.y] = true;
                    }

                    check = item;
                    --check.x;
                    if (check.x > 0 && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = GetBiomeIndex(biome);
                        filled[check.x, check.y] = true;
                    }
                }
                currentGeneration = nextGeneration;
                nextGeneration = new List<IntVector2>(25);
            }
        }

        private List<T> RemoveRandomItems<T>(List<T> items, FIRandom random, int count)
        {
            //This needs to actually iterate or else you won't get the same treasure
            //because it won't be solved until later when the enumerable runs
            var results = new List<T>();
            for(int i = 0; i <count; ++i)
            {
                results.Add(RemoveRandomItem(items, random));
            }
            return results;
        }

        private T RemoveRandomItem<T>(List<T> items, FIRandom random)
        {
            var index = random.Next(items.Count);
            var item = items[index];
            items.RemoveAt(index);
            return item;
        }
    }
}
