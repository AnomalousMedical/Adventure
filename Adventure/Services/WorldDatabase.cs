using Adventure.Items.Creators;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var weaknessRandom = new FIRandom(newSeed);
            var monsterInfo = MonsterMaker.CreateBaseMonsters(weaknessRandom);
            var usedSquares = new bool[map.MapX, map.MapY];
            var usedIslands = new bool[map.NumIslands];
            portalLocations = new List<IntVector2>(5);

            //Reserve the 3 largest islands
            usedIslands[map.IslandSizeOrder[0]] = true;
            usedIslands[map.IslandSizeOrder[1]] = true;
            usedIslands[map.IslandSizeOrder[2]] = true;

            SetupAirshipIsland(placementRandom, out airshipStartSquare, out airshipPortalSquare, usedSquares, usedIslands, map);
            areaBuilders = SetupAreaBuilder(monsterInfo, biomeRandom, placementRandom, elementalRandom, treasureRandom, portalLocations, usedSquares, usedIslands, map).ToList();
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

        private IEnumerable<IAreaBuilder> SetupAreaBuilder(IList<MonsterInfo> monsterInfo, FIRandom biomeRandom, FIRandom placementRandom, FIRandom elementalRandom, FIRandom treasureRandom, List<IntVector2> portalLocations, bool[,] usedSquares, bool[] usedIslands, csIslandMaze map)
        {
            //TODO: Add enemy strengths and weaknesses in phase 2, 3
            //TODO: finish phase 2, 3 and bonus 2, 3
            //TODO: Randomize zones by placing squares in a list then pulling the squares out
            //TODO: Every boss should have unique steal treasure for permanent stat boosts

            var filled = new bool[map.MapX, map.MapY];
            int area = 0;
            AreaBuilder areaBuilder;
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
                var startingElementStaff = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
                var phase0TreasureLevel = 3;
                var phase0UniqueTreasures = new List<Treasure>();
                phase0UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(ElementalStaffCreator.GetStaffCreator(startingElementStaff).CreateNormal(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(BookCreator.CreateCure(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase0TreasureLevel)));

                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)));

                phase0UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase0TreasureLevel)));

                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase0TreasureLevel)));
                phase0UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase0TreasureLevel)));

                //Area 1
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 0;
                areaBuilder.EndZone = 0;
                areaBuilder.Phase = 0;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
                areaBuilder.Location = GetSquare(firstIslandSquares, placementRandom);
                areaBuilder.Treasure = phase0UniqueTreasures;
                areaBuilder.StartEnd = true;
                areaBuilder.MaxMainCorridorBattles = 1;
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;
            }

            //Phase 1
            {
                var phase1EndWeakElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
                var phase1BonusWeakElement = GetDifferentElement(elementalRandom, phase1EndWeakElement);
                var phase1TreasureLevel = 25;
                var phase1UniqueTreasures = new List<Treasure>();
                phase1UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(BookCreator.CreateCure(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(ElementalStaffCreator.GetStaffCreator(phase1EndWeakElement).CreateNormal(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel)));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateStrengthBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateMagicBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateSpiritBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateVitalityBoost()));
                phase1UniqueTreasures.Add(new Treasure(PotionCreator.CreateLuckBoost()));

                var phase1UniqueStolenTreasures = new List<Treasure>();
                phase1UniqueStolenTreasures.Add(new Treasure(ElementalStaffCreator.GetStaffCreator(phase1BonusWeakElement).CreateNormal(phase1TreasureLevel)));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase1UniqueStolenTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase1TreasureLevel)));
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
                areaBuilder.StartZone = 2;
                areaBuilder.EndZone = 2;
                areaBuilder.Phase = 1;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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

                //Area 4
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 6;
                areaBuilder.EndZone = 6;
                areaBuilder.Phase = 1;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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
                var phase2EndWeakElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
                var phase2BonusWeakElement = GetDifferentElement(elementalRandom, phase2EndWeakElement);
                var phase2TreasureLevel = 60;
                var phase2UniqueTreasures = new List<Treasure>();
                phase2UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(BookCreator.CreateCure(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(ElementalStaffCreator.GetStaffCreator(phase2EndWeakElement).CreateNormal(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(BookCreator.CreateReanimation(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateCounterAttack()));
                phase2UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateItemUsage(phase2TreasureLevel)));
                phase2UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateHealing(phase2TreasureLevel)));
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
                phase2UniqueStolenTreasures.Add(new Treasure(ElementalStaffCreator.GetStaffCreator(phase2BonusWeakElement).CreateNormal(phase2TreasureLevel)));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase2UniqueStolenTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase2TreasureLevel)));
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

                //Area 5
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 10;
                areaBuilder.EndZone = 10;
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.PlotItem = PlotItems.AirshipKey0;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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

                //Area 6
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 12;
                areaBuilder.EndZone = 12;
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.PlotItem = PlotItems.AirshipKey1;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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

                //Area 9
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 18;
                areaBuilder.EndZone = 18;
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.PlotItem = PlotItems.AirshipKey2;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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
                var phase3EndWeakElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
                var phase3BonusWeakElement = GetDifferentElement(elementalRandom, phase3EndWeakElement);
                var phase3TreasureLevel = 99;
                var phase3UniqueTreasures = new List<Treasure>();
                phase3UniqueTreasures.Add(new Treasure(SwordCreator.CreateNormal(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(SpearCreator.CreateNormal(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(MaceCreator.CreateNormal(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(BookCreator.CreateCure(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(ShieldCreator.CreateNormal(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(ElementalStaffCreator.GetStaffCreator(phase3EndWeakElement).CreateNormal(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreatePlate(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreateLeather(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(BookCreator.CreateReanimation(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateItemUsage(phase3TreasureLevel)));
                phase3UniqueTreasures.Add(new Treasure(AccessoryCreator.CreateHealing(phase3TreasureLevel)));
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
                phase3UniqueStolenTreasures.Add(new Treasure(ElementalStaffCreator.GetStaffCreator(phase3BonusWeakElement).CreateNormal(phase3TreasureLevel)));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueStolenTreasures.Add(new Treasure(PotionCreator.CreateFerrymansBribe()));
                phase3UniqueStolenTreasures.Add(new Treasure(DaggerCreator.CreateNormal(phase3TreasureLevel)));
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

                //Area 10
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 22;
                areaBuilder.EndZone = 22;
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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

                //Area 11
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 24;
                areaBuilder.EndZone = 24;
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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

                //Area 12
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 26;
                areaBuilder.EndZone = 26;
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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

                //Area 15
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = 32;
                areaBuilder.EndZone = 32;
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 3;
                areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
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
        }

        private static Element GetDifferentElement(FIRandom elementalRandom, Element notThisElement)
        {
            var otherElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
            int retry = 0;
            while (otherElement == notThisElement)
            {
                otherElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
                if (retry++ > 10)
                {
                    otherElement = (Element)(((int)notThisElement + 1) % (int)Element.MagicEnd);
                }
            }

            return otherElement;
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

        private static void SetIslandBiome(IslandInfo island, csIslandMaze map, BiomeType biome)
        {
            foreach (var square in island.islandPoints)
            {
                map.TextureOffsets[square.x, square.y] = (int)biome;
            }
        }

        private static void FillSurroundings(csIslandMaze map, BiomeType biome, IntVector2 startPoint, bool[,] filled)
        {
            //The start point will always be filled out even if its already filled
            map.TextureOffsets[startPoint.x, startPoint.y] = (int)biome;
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
                        map.TextureOffsets[check.x, check.y] = (int)biome;
                        filled[check.x, check.y] = true;
                    }

                    check = item;
                    --check.y;
                    if (check.y > 0 && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = (int)biome;
                        filled[check.x, check.y] = true;
                    }

                    check = item;
                    ++check.x;
                    if (check.x < map.MapX && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = (int)biome;
                        filled[check.x, check.y] = true;
                    }

                    check = item;
                    --check.x;
                    if (check.x > 0 && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = (int)biome;
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
