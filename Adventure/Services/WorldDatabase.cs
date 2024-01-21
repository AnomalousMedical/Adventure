using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using Adventure.Items;
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
        IEnumerable<ShopEntry> CreateShopItems(HashSet<PlotItems> plotItems);
        IEnumerable<PartyMember> CreateParty();

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
        List<IntVector2> StorePhilipLocations { get; }
    }

    record ShopEntry(String Text, long Cost, Func<InventoryItem> CreateItem, PlotItems? UniqueSalePlotItem = null) { }

    class WorldDatabase : IWorldDatabase
    {
        private List<IAreaBuilder> areaBuilders;
        private List<int> createdZoneSeeds;
        private int currentSeed;
        private FIRandom zoneRandom;
        private readonly Persistence persistence;
        private readonly IInventoryFunctions inventoryFunctions;
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
        public List<IntVector2> StorePhilipLocations { get; private set; }

        public int GetLevelDelta(int currentLevel)
        {
            var delta = 5;
            var levelBreaks = new int[] { 17, 29, 36, 44, 51, 56, 61 };

            foreach (var levelBreak in levelBreaks)
            {
                if (currentLevel < levelBreak)
                {
                    delta = levelBreak - currentLevel;
                    break;
                }
            }

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
            BookCreator bookCreator,
            IInventoryFunctions inventoryFunctions
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
            this.inventoryFunctions = inventoryFunctions;
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
            StorePhilipLocations = new List<IntVector2>();
            var biomeRandom = new FIRandom(newSeed);
            var placementRandom = new FIRandom(newSeed);
            var elementalRandom = new FIRandom(newSeed);
            var treasureRandom = new FIRandom(newSeed);
            var alignmentRandom = new FIRandom(newSeed);
            currentSeed = newSeed;

            //Setup map
            worldMap = new WorldMapData(newSeed);
            var numIslands = 1  //Phase 0, 1
                            + 2  //Phase 2
                            + 3  //Phase 3
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
            areaBuilders = SetupAreaBuilder(newSeed, biomeRandom, placementRandom, elementalRandom, treasureRandom, alignmentRandom, portalLocations, usedSquares, usedIslands, map).ToList();
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

        const int phase0TreasureLevel = 1;
        const int phase1TreasureLevel = 20;

        public IEnumerable<PartyMember> CreateParty()
        {
            var characterRandom = new FIRandom(this.currentSeed);

            {
                var sheet = CharacterSheet.CreateStartingFighter(characterRandom);
                sheet.Name = "Bob";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.FighterPlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(SpearCreator.CreateNormal(phase0TreasureLevel, "Rusty")));
                GiveAndEquip(hero, new Treasure(ShieldCreator.CreateNormal(phase0TreasureLevel, "Buckler", 0.15f, nameof(Buckler))));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreatePlate(phase1TreasureLevel, "Common", EquipmentTier.Tier1)));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = $"I am {sheet.Name}. I am a Warrior.",
                };
            }

            {
                var sheet = CharacterSheet.CreateStartingMage(characterRandom);
                sheet.Name = "Magic Joe";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.MagePlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, "Common", EquipmentTier.Tier1)));
                GiveAndEquip(hero, new Treasure(ElementalStaffCreator.CreateNormal(phase0TreasureLevel, "Cracked", nameof(WeakFire), nameof(WeakIce), nameof(WeakLightning))));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = $"I am {sheet.Name}. I am a Mage.",
                };
            }

            {
                var sheet = CharacterSheet.CreateStartingThief(characterRandom);
                sheet.Name = "Stabby McStabface";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ThiefPlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(SwordCreator.CreateNormal(phase0TreasureLevel, "Busted")));
                GiveAndEquip(hero, new Treasure(DaggerCreator.CreateNormal(phase0TreasureLevel, "Rusty", nameof(Steal))));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateLeather(phase1TreasureLevel, "Common", EquipmentTier.Tier1)));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = $"I am {sheet.Name}. I am a Thief.",
                };
            }

            {
                var sheet = CharacterSheet.CreateStartingSage(characterRandom);
                sheet.Name = "Wendy";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ClericPlayerSprite),
                    CharacterSheet = sheet
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(MaceCreator.CreateNormal(phase0TreasureLevel, "Rusty")));
                GiveAndEquip(hero, new Treasure(BookCreator.CreateRestoration(phase0TreasureLevel, "Torn", nameof(Cure))));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, "Common", EquipmentTier.Tier1)));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = $"I am {sheet.Name}. I am a Cleric.",
                };
            }
        }

        void GiveAndEquip(Persistence.CharacterData hero, Treasure treasure)
        {
            treasure.GiveTo(hero.Inventory);
            treasure.Use(hero.Inventory, hero.CharacterSheet, inventoryFunctions);
        }

        private IEnumerable<IAreaBuilder> SetupAreaBuilder(int seed, FIRandom biomeRandom, FIRandom placementRandom, FIRandom elementalRandom, FIRandom treasureRandom, FIRandom alignmentRandom, List<IntVector2> portalLocations, bool[,] usedSquares, bool[] usedIslands, csIslandMaze map)
        {
            var biomes = new List<BiomeType>() { BiomeType.Desert, BiomeType.Forest, BiomeType.Snowy, BiomeType.Beach, BiomeType.Swamp, BiomeType.Mountain };
            var biomeDistributor = new EnumerableDistributor<BiomeType>(biomes);

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
            var zoneAlignment = new RandomItemDistributor<Zone.Alignment>(new[] { Zone.Alignment.EastWest, Zone.Alignment.WestEast, Zone.Alignment.NorthSouth, Zone.Alignment.SouthNorth });

            //Setup islands                      
            var firstIslandSquares = GetIslandExtremes(map.IslandInfo[map.IslandSizeOrder[0]], placementRandom, usedSquares);
            var secondIslandSquares = GetIslandExtremes(map.IslandInfo[map.IslandSizeOrder[1]], placementRandom, usedSquares);
            var thirdIslandSquares = GetIslandExtremes(map.IslandInfo[map.IslandSizeOrder[2]], placementRandom, usedSquares);
            portalLocations.Add(firstIslandSquares[0][0]);
            portalLocations.Add(airshipPortalSquare);

            IslandInfo firstStorePhilipIsland = map.IslandInfo[map.IslandSizeOrder[0]];
            IslandInfo secondStorePhilipIsland = map.IslandInfo[map.IslandSizeOrder[2]];
            IslandInfo thirdStorePhilipIsland;

            var phase1Adjective = "Common";

            //Phase 0
            {
                var startingBiome = BiomeType.Countryside;
                
                var firstBoss = monsterInfo.Where(i => i.NativeBiome == startingBiome).First();
                var bossResistance = firstBoss.Resistances.Where(i => i.Value == Resistance.Weak && i.Key > Element.MagicStart && i.Key < Element.MagicEnd);
                var phase0UniqueTreasures = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel)),
                    new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)),
                    new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)),
                };

                //Area 1
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 0;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = startingBiome;
                areaBuilder.BossMonster = firstBoss;
                areaBuilder.Location = firstIslandSquares[1][0];
                areaBuilder.Treasure = phase0UniqueTreasures;
                areaBuilder.StartEnd = true;
                areaBuilder.MaxMainCorridorBattles = 1;
                areaBuilder.PartyMembers = CreateParty().Skip(1);
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;
            }

            //Phase 1
            {
                var phase1UniqueTreasures = new List<Treasure>
                {
                    new Treasure(SwordCreator.CreateNormal(phase1TreasureLevel, phase1Adjective)),
                    new Treasure(SpearCreator.CreateNormal(phase1TreasureLevel, phase1Adjective)),
                    new Treasure(MaceCreator.CreateNormal(phase1TreasureLevel, phase1Adjective)),
                    new Treasure(BookCreator.CreateRestoration(phase1TreasureLevel, phase1Adjective, nameof(Cure))),
                    new Treasure(ShieldCreator.CreateNormal(phase1TreasureLevel, phase1Adjective, 0.25f, nameof(ShieldOfReflection))),
                    new Treasure(ElementalStaffCreator.CreateNormal(phase1TreasureLevel, "Scholar's", nameof(Fire), nameof(Ice), nameof(Lightning))),
                    new Treasure(AccessoryCreator.CreateTargetScope())
                };

                var phase1UniqueStolenTreasures = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(DaggerCreator.CreateNormal(phase1TreasureLevel, phase1Adjective, nameof(Steal))),
                    new Treasure(PotionCreator.CreateStrengthBoost(2)),
                    new Treasure(PotionCreator.CreateMagicBoost(2)),
                    new Treasure(PotionCreator.CreateSpiritBoost(2)),
                    new Treasure(PotionCreator.CreateVitalityBoost(2)),
                    new Treasure(PotionCreator.CreateLuckBoost(1))
                };

                var uniqueTreasure = phase1UniqueTreasures.Count;
                var stolenTreasure = phase1UniqueStolenTreasures.Count;

                //Area 2
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 1;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 2;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                areaBuilder.Location = firstIslandSquares[1][1];
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
            }

            //Phase 2
            {
                var phase2TreasureLevel = 35;
                var phase2Adjective = "Quality";
                var phase2Weapons = new List<Treasure>()
                {
                    new Treasure(SwordCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, nameof(UltimateSword))),
                    new Treasure(SpearCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, nameof(UltimateSpear))),
                    new Treasure(MaceCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, nameof(UltimateHammer))),
                    new Treasure(BookCreator.CreateRestoration(phase2TreasureLevel, phase2Adjective, nameof(MegaCure), nameof(BattleCry), nameof(Focus))),
                    new Treasure(ShieldCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, 0.35f, nameof(UltimateShield))),
                    new Treasure(ElementalStaffCreator.CreateNormal(phase2TreasureLevel, "Mage's", nameof(StrongFire), nameof(StrongIce), nameof(StrongLightning))),
                };

                var phase2Armors = new List<Treasure>()
                {
                    new Treasure(ArmorCreator.CreatePlate(phase2TreasureLevel, phase2Adjective, EquipmentTier.Tier2)),
                    new Treasure(ArmorCreator.CreateLeather(phase2TreasureLevel, phase2Adjective, EquipmentTier.Tier2)),
                    new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel, phase2Adjective, EquipmentTier.Tier2)),
                    new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel, phase2Adjective, EquipmentTier.Tier2)),
                };

                var phase2Accessories = new List<Treasure>()
                {
                    new Treasure(AccessoryCreator.CreateCounterAttack()),
                    new Treasure(AccessoryCreator.CreateHealing(phase2Adjective, 0.25f, true)),
                };

                var phase2Potions = new List<Treasure>()
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(PotionCreator.CreateStrengthBoost(2)),
                    new Treasure(PotionCreator.CreateMagicBoost(2)),
                    new Treasure(PotionCreator.CreateSpiritBoost(2)),
                    new Treasure(PotionCreator.CreateVitalityBoost(2)),
                };

                var phase2UniqueStolenTreasures = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(DaggerCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, nameof(Steal))),
                    new Treasure(PotionCreator.CreateStrengthBoost(3)),
                    new Treasure(PotionCreator.CreateMagicBoost(3)),
                    new Treasure(PotionCreator.CreateSpiritBoost(2)),
                    new Treasure(PotionCreator.CreateVitalityBoost(2)),
                    new Treasure(PotionCreator.CreateLuckBoost(2))
                };

                const int zoneCount = 2;
                var weapons = phase2Weapons.Count / zoneCount;
                var armors = phase2Armors.Count / zoneCount;
                var accessories = phase2Accessories.Count / zoneCount;
                var potions = phase2Potions.Count / zoneCount;
                var stolenTreasure = phase2UniqueStolenTreasures.Count / zoneCount;

                //Area 3
                portalLocations.Add(secondIslandSquares[0][0]);
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.PlotItem = PlotItems.AirshipKey0;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 2;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                areaBuilder.Monsters = elementalMonsters[GetElementForBiome(areaBuilder.Biome)]
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = secondIslandSquares[0][1];
                areaBuilder.Treasure =
                    new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase2TreasureLevel))
                    }
                    .Concat(RemoveRandomItems(phase2Weapons, treasureRandom, weapons))
                    .Concat(RemoveRandomItems(phase2Armors, treasureRandom, armors))
                    .Concat(RemoveRandomItems(phase2Accessories, treasureRandom, accessories))
                    .Concat(RemoveRandomItems(phase2Potions, treasureRandom, potions));
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase2UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel))
                };
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;

                //Area 4
                portalLocations.Add(thirdIslandSquares[0][0]);
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.PlotItem = PlotItems.AirshipKey1;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 2;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                areaBuilder.Monsters = elementalMonsters[GetElementForBiome(areaBuilder.Biome)]
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = thirdIslandSquares[0][1];
                areaBuilder.Treasure =
                    new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase2TreasureLevel))
                    }
                    .Concat(RemoveRandomItems(phase2Weapons, treasureRandom, phase2Weapons.Count))
                    .Concat(RemoveRandomItems(phase2Armors, treasureRandom, phase2Armors.Count))
                    .Concat(RemoveRandomItems(phase2Accessories, treasureRandom, phase2Accessories.Count))
                    .Concat(RemoveRandomItems(phase2Potions, treasureRandom, phase2Potions.Count));
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
                IslandInfo island;

                var phase3TreasureLevel = 55;
                var phase3Adjective = "Superior";
                var phase3Weapons = new List<Treasure>
                {
                    new Treasure(SwordCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, nameof(FinalSword))),
                    new Treasure(SpearCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, nameof(FinalSpear))),
                    new Treasure(MaceCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, nameof(FinalHammer))),
                    new Treasure(ElementalStaffCreator.CreateNormal(nameof(UltimateStaff), phase3TreasureLevel, "Arch Mage's", nameof(IonShread), nameof(ArchFire), nameof(ArchIce), nameof(ArchLightning))),
                    new Treasure(ShieldCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, 0.45f, nameof(FinalShield))),
                    new Treasure(BookCreator.CreateRestoration(nameof(UltimateBook), phase3TreasureLevel, phase3Adjective, nameof(UltraCure), nameof(Reanimate), nameof(WarCry), nameof(IntenseFocus))),
                };

                var phase3Armors = new List<Treasure>
                {
                    new Treasure(ArmorCreator.CreatePlate(phase3TreasureLevel, phase3Adjective, EquipmentTier.Tier3)),
                    new Treasure(ArmorCreator.CreateLeather(phase3TreasureLevel, phase3Adjective, EquipmentTier.Tier3)),
                    new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel, phase3Adjective, EquipmentTier.Tier3)),
                    new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel, phase3Adjective, EquipmentTier.Tier3)),
                };

                var phase3Accessories = new List<Treasure>
                {
                    new Treasure(AccessoryCreator.CreateItemUsage(phase3Adjective, 0.5f)),
                };

                var phase3Potions = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(PotionCreator.CreateStrengthBoost(2)),
                    new Treasure(PotionCreator.CreateStrengthBoost(1)),
                    new Treasure(PotionCreator.CreateMagicBoost(2)),
                    new Treasure(PotionCreator.CreateMagicBoost(1)),
                    new Treasure(PotionCreator.CreateSpiritBoost(2)),
                    new Treasure(PotionCreator.CreateSpiritBoost(2)),
                    new Treasure(PotionCreator.CreateVitalityBoost(2)),
                    new Treasure(PotionCreator.CreateVitalityBoost(2)),
                    new Treasure(PotionCreator.CreateLuckBoost(2)),
                };

                var phase3UniqueStolenTreasures = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(DaggerCreator.CreateNormal(nameof(UltimateDagger), phase3TreasureLevel, phase3Adjective, nameof(Steal), nameof(Haste))),
                    new Treasure(AccessoryCreator.CreateDoublecast()),
                    new Treasure(PotionCreator.CreateStrengthBoost(3)),
                    new Treasure(PotionCreator.CreateStrengthBoost(2)),
                    new Treasure(PotionCreator.CreateStrengthBoost(1)),
                    new Treasure(PotionCreator.CreateMagicBoost(3)),
                    new Treasure(PotionCreator.CreateMagicBoost(2)),
                    new Treasure(PotionCreator.CreateMagicBoost(1)),
                    new Treasure(PotionCreator.CreateSpiritBoost(2)),
                    new Treasure(PotionCreator.CreateSpiritBoost(1)),
                    new Treasure(PotionCreator.CreateVitalityBoost(2)),
                    new Treasure(PotionCreator.CreateVitalityBoost(1)),
                    new Treasure(PotionCreator.CreateLuckBoost(2)),
                    new Treasure(PotionCreator.CreateLuckBoost(2))
                };

                var zoneCount = 3;
                var weapons = phase3Weapons.Count / zoneCount;
                var armors = phase3Armors.Count / zoneCount;
                var accessories = phase3Accessories.Count / zoneCount;
                var potions = phase3Potions.Count / zoneCount;
                var stolenTreasure = phase3UniqueStolenTreasures.Count / zoneCount;

                Element firstMonsterElement;
                //Area 5
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 3;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure =
                    new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel))
                    }
                    .Concat(RemoveRandomItems(phase3Weapons, treasureRandom, weapons))
                    .Concat(RemoveRandomItems(phase3Armors, treasureRandom, armors))
                    .Concat(RemoveRandomItems(phase3Accessories, treasureRandom, accessories))
                    .Concat(RemoveRandomItems(phase3Potions, treasureRandom, potions));
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel))
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;

                //Area 6
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                thirdStorePhilipIsland = island;
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 3;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure =
                    new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel))
                    }
                    .Concat(RemoveRandomItems(phase3Weapons, treasureRandom, weapons))
                    .Concat(RemoveRandomItems(phase3Armors, treasureRandom, armors))
                    .Concat(RemoveRandomItems(phase3Accessories, treasureRandom, accessories))
                    .Concat(RemoveRandomItems(phase3Potions, treasureRandom, potions));
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel))
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;

                //Area 7
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.GateZones = new[] { areaBuilder.StartZone };
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 3;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure =
                    new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel))
                    }
                    .Concat(RemoveRandomItems(phase3Weapons, treasureRandom, phase3Weapons.Count))
                    .Concat(RemoveRandomItems(phase3Armors, treasureRandom, phase3Armors.Count))
                    .Concat(RemoveRandomItems(phase3Accessories, treasureRandom, phase3Accessories.Count))
                    .Concat(RemoveRandomItems(phase3Potions, treasureRandom, phase3Potions.Count));
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
                var phase4TreasureLevel = 60;

                var phase4UniqueTreasures = new List<Treasure>()
                {
                    new Treasure(PotionCreator.CreateLevelBoost()),
                    new Treasure(PotionCreator.CreateLevelBoost()),
                    new Treasure(PotionCreator.CreateLevelBoost()),
                    new Treasure(PotionCreator.CreateLevelBoost()),
                };

                //Area 8
                var island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.EnemyLevel = 61;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.Biome = BiomeType.Volcano;
                areaBuilder.MaxMainCorridorBattles = 4;
                areaBuilder.Alignment = alignmentRandom.Next(2) == 0 ? Zone.Alignment.EastWest : Zone.Alignment.WestEast;
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);

                //You get all the monsters in this zone
                areaBuilder.Monsters = monsterInfo;
                foreach (var monsters in elementalMonsters.Values)
                {
                    areaBuilder.Monsters = areaBuilder.Monsters.Concat(monsters);
                }
                areaBuilder.Monsters = areaBuilder.Monsters.ToList();
                areaBuilder.BossMonster = monsterInfo.Where(i => i.NativeBiome == BiomeType.FinalBoss).First();
                areaBuilder.Treasure = phase4UniqueTreasures;
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase4TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase4TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase4TreasureLevel)),
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                yield return areaBuilder;
            }

            StorePhilipLocations.Add(GetUnusedSquare(usedSquares, firstStorePhilipIsland, placementRandom));
            StorePhilipLocations.Add(GetUnusedSquare(usedSquares, secondStorePhilipIsland, placementRandom));
            StorePhilipLocations.Add(GetUnusedSquare(usedSquares, thirdStorePhilipIsland, placementRandom));
        }

        /// <summary>
        /// Get the extremes of the island. The returned results are scrambled, so you can access the same indexes externally, but the values will not
        /// always be the same. The pairs returned will be at either extreme, northmost and southmost or eastmost and westmost, but you won't know 
        /// what is in what index.
        /// </summary>
        private static List<List<IntVector2>> GetIslandExtremes(IslandInfo island, FIRandom placementRandom, bool[,] usedSquares)
        {
            const int Max = 100;
            const int HalfMax = Max / 100;

            IntVector2 pair00;
            IntVector2 pair01;
            IntVector2 pair10;
            IntVector2 pair11;
            if (placementRandom.Next(Max) < HalfMax)
            {
                if (placementRandom.Next(Max) < HalfMax)
                {
                    pair00 = island.Northmost;
                    pair01 = island.Southmost;
                }
                else
                {
                    pair00 = island.Southmost;
                    pair01 = island.Northmost;
                }

                if (placementRandom.Next(Max) < HalfMax)
                {
                    pair10 = island.Eastmost;
                    pair11 = island.Westmost;
                }
                else
                {
                    pair10 = island.Westmost;
                    pair11 = island.Eastmost;
                }
            }
            else
            {
                if (placementRandom.Next(Max) < HalfMax)
                {
                    pair10 = island.Northmost;
                    pair11 = island.Southmost;
                }
                else
                {
                    pair10 = island.Southmost;
                    pair11 = island.Northmost;
                }

                if (placementRandom.Next(Max) < HalfMax)
                {
                    pair00 = island.Eastmost;
                    pair01 = island.Westmost;
                }
                else
                {
                    pair00 = island.Westmost;
                    pair01 = island.Eastmost;
                }
            }

            var islandZoneLocations = new List<List<IntVector2>>
            {
                new List<IntVector2>
                {
                    GetUnusedSquare(usedSquares, island, placementRandom, pair00),
                    GetUnusedSquare(usedSquares, island, placementRandom, pair01)
                },
                new List<IntVector2>
                {
                    GetUnusedSquare(usedSquares, island, placementRandom, pair10),
                    GetUnusedSquare(usedSquares, island, placementRandom, pair11)
                },
            };

            return islandZoneLocations;
        }

        public IEnumerable<ShopEntry> CreateShopItems(HashSet<PlotItems> plotItems)
        {
            if (plotItems.Contains(PlotItems.Phase3Shop))
            {
                yield return new ShopEntry("Giant Mana Potion", 135, () => PotionCreator.CreateManaPotion(55));
            }

            if (plotItems.Contains(PlotItems.Phase2Shop))
            {
                yield return new ShopEntry("Big Mana Potion", 70, () => PotionCreator.CreateManaPotion(35));
            }

            if (plotItems.Contains(PlotItems.Phase1Shop))
            {
                yield return new ShopEntry("Mana Potion", 25, () => PotionCreator.CreateManaPotion(1));
            }

            if (plotItems.Contains(PlotItems.Phase1Shop))
            {
                yield return new ShopEntry("Ferryman's Bribe", 350, () => PotionCreator.CreateFerrymansBribe());
            }

            if (plotItems.Contains(PlotItems.Phase2Shop))
            {
                var treasureLevel = 40;
                var adjective = "Fancy Store Bought";
                yield return new ShopEntry($"{adjective} Sword", 1500, () => SwordCreator.CreateNormal(treasureLevel, adjective));
                yield return new ShopEntry($"{adjective} Spear", 1500, () => SpearCreator.CreateNormal(treasureLevel, adjective));
                yield return new ShopEntry($"{adjective} Mace", 1500, () => MaceCreator.CreateNormal(treasureLevel, adjective));
            }

            if (plotItems.Contains(PlotItems.Phase1Shop))
            {
                var treasureLevel = 10;
                var adjective = "Store Bought";
                yield return new ShopEntry($"{adjective} Sword", 150, () => SwordCreator.CreateNormal(treasureLevel, adjective));
                yield return new ShopEntry($"{adjective} Spear", 150, () => SpearCreator.CreateNormal(treasureLevel, adjective));
                yield return new ShopEntry($"{adjective} Mace", 150, () => MaceCreator.CreateNormal(treasureLevel, adjective));
            }
        }

        private static Element GetRandomMagicElement(FIRandom elementalRandom, params Element[] except)
        {
            var elements = Enum.GetValues<Element>().Where(i => i > Element.MagicStart && i < Element.MagicEnd && !except.Contains(i)).ToArray();
            if (elements.Length == 0)
            {
                throw new InvalidOperationException("No elements left to select from");
            }

            return elements[elementalRandom.Next(0, elements.Length)];
        }

        private static Element GetElementForBiome(BiomeType biome)
        {
            switch (biome)
            {
                case BiomeType.Beach:
                case BiomeType.Snowy:
                    return Element.Ice;
                case BiomeType.Countryside:
                    return Element.Fire;
                case BiomeType.Forest:
                case BiomeType.Swamp:
                    return Element.Electricity;
                case BiomeType.Desert:
                    return Element.Fire;
                default:
                    return Element.Fire;
            }
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
                case BiomeType.Mountain: //temp, give mountain its own thing
                case BiomeType.Volcano:
                    return 8;
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
            for (int i = 0; i < count; ++i)
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
