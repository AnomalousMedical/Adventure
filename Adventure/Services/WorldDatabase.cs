using Adventure.Assets.Equipment;
using Adventure.Items;
using Adventure.Items.Creators;
using Adventure.Skills;
using Adventure.Skills.Spells;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Services
{
    public enum ShopType
    {
        Alchemist,
        Blacksmith,
    }

    interface IWorldDatabase
    {
        int GetZoneSeed(int index);
        IAreaBuilder GetAreaBuilder(int zoneIndex);
        int GetLevelDelta(int area);
        void Reset(int newSeed);
        IEnumerable<ShopEntry> CreateShopItems(ShopType shopType, HashSet<PlotItems> plotItems);
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
        int CurrentSeed { get; }
        IntVector2 AirshipStartSquare { get; }
        BookCreator BookCreator { get; }
        IntVector2 InnkeeperPosition { get; }
        IntVector2 BlacksmithPosition { get; }
        IntVector2 AlchemistPosition { get; }
        IntVector2 BlacksmithUpgradePosition { get; }
        IntVector2 AlchemistUpgradePosition { get; }
        IntVector2 FortuneTellerPosition { get; }
        IntVector2 ElementalStonePosition { get; }
        IntVector2 ItemStoragePosition { get; }
        IEnumerable<IntVector2> BiomePropLocations { get; }
    }

    record ShopEntry(String InfoId, long Cost, bool IsEquipment, Func<InventoryItem> CreateItem, PlotItems? UniqueSalePlotItem = null) { }

    class WorldDatabase : IWorldDatabase
    {
        public record Text
        (
            String FighterName,
            String FighterGreeting,
            String FighterClass,
            String FighterDescription,
            String MageName,
            String MageGreeting,
            String MageClass,
            String MageDescription,
            String ThiefName,
            String ThiefGreeting,
            String ThiefClass,
            String ThiefDescription,
            String ClericName,
            String ClericGreeting,
            String ClericClass,
            String ClericDescription
        );

        private List<IAreaBuilder> areaBuilders;
        private List<int> createdZoneSeeds;
        private int currentSeed;
        private FIRandom zoneRandom;
        private readonly Persistence persistence;
        private readonly IInventoryFunctions inventoryFunctions;
        private readonly ILanguageService languageService;
        private IntVector2 airshipStartSquare;
        private const byte MaxFillGeneration = 6;

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
        public IntVector2 AirshipStartSquare => airshipStartSquare;
        public IntVector2 InnkeeperPosition { get; private set; }
        public IntVector2 BlacksmithPosition { get; private set; }
        public IntVector2 AlchemistPosition { get; private set; }
        public IntVector2 BlacksmithUpgradePosition { get; private set; }
        public IntVector2 AlchemistUpgradePosition { get; private set; }
        public IntVector2 FortuneTellerPosition { get; private set; }
        public IntVector2 ElementalStonePosition { get; private set; }
        public IntVector2 ItemStoragePosition { get; private set; }
        public IEnumerable<IntVector2> BiomePropLocations { get; private set; }

        public int GetLevelDelta(int currentLevel)
        {
            var delta = 5;
            var levelBreaks = new int[] { 29, 36, 44, 51, 56, 61 };

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
            IInventoryFunctions inventoryFunctions,
            ILanguageService languageService
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
            this.languageService = languageService;
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
            var alignmentRandom = new FIRandom(newSeed);
            currentSeed = newSeed;

            //Setup map
            worldMap = new WorldMapData(newSeed);
            var numIslands = 1  //Phase 0, 1
                            + 3  //Phase 2
                            + 1  //End zone
                            ;
            worldMap.Map.RemoveExtraIslands(numIslands);
            var map = worldMap.Map;
            //TODO: need to check maps
            //3 largest islands need to have enough spaces for each phase
            //World needs enough islands to cover all zones

            //Setup areas
            var usedSquares = new byte[map.MapX, map.MapY];
            var usedIslands = new bool[map.NumIslands];

            //Reserve the 3 largest islands
            usedIslands[map.IslandSizeOrder[0]] = true;

            var islandIndex = map.IslandSizeOrder[0];
            var island = map.IslandInfo[islandIndex];
            usedIslands[islandIndex] = true;
            airshipStartSquare = GetUnusedSquare(usedSquares, island, placementRandom);
            usedSquares[airshipStartSquare.x, airshipStartSquare.y] = byte.MaxValue;
            PreventUsageNorthSouth(airshipStartSquare, usedSquares);

            areaBuilders = SetupAreaBuilder(newSeed, biomeRandom, placementRandom, elementalRandom, treasureRandom, alignmentRandom, usedSquares, usedIslands, map).ToList();
        }

        const int phase0TreasureLevel = 10;
        const int phase1TreasureLevel = 20;

        public IEnumerable<PartyMember> CreateParty()
        {
            var characterRandom = new FIRandom(this.currentSeed);

            {
                var sheet = CharacterSheet.CreateStartingFighter(characterRandom);
                sheet.Name = languageService.Current.WorldDatabase.FighterName;
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.FighterPlayerSprite),
                    CharacterSheet = sheet,
                    StyleIndex = 0,
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(SpearCreator.CreateNormal(phase0TreasureLevel, nameof(ItemText.Spear1), nameof(Spear1), false), TreasureType.Weapon));
                GiveAndEquip(hero, new Treasure(ShieldCreator.CreateNormal(phase0TreasureLevel, nameof(ItemText.Shield1), 0.15f, nameof(Shield1)), TreasureType.OffHand));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreatePlate(phase1TreasureLevel, nameof(ItemText.Plate1), EquipmentTier.Tier1), TreasureType.Armor));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = languageService.Current.WorldDatabase.FighterGreeting,
                    Class = languageService.Current.WorldDatabase.FighterClass,
                    Description = languageService.Current.WorldDatabase.FighterDescription,
                };
            }

            {
                var sheet = CharacterSheet.CreateStartingMage(characterRandom);
                sheet.Name = languageService.Current.WorldDatabase.MageName;
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.MagePlayerSprite),
                    CharacterSheet = sheet,
                    StyleIndex = 1,
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, nameof(ItemText.Cloth1), EquipmentTier.Tier1), TreasureType.Armor));
                GiveAndEquip(hero, new Treasure(ElementalStaffCreator.CreateNormal(nameof(Staff1), phase0TreasureLevel, nameof(ItemText.Staff1), nameof(Fire), nameof(Ice), nameof(Lightning)), TreasureType.Weapon));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = languageService.Current.WorldDatabase.MageGreeting,
                    Class = languageService.Current.WorldDatabase.MageClass,
                    Description = languageService.Current.WorldDatabase.MageDescription,
                };
            }

            {
                var sheet = CharacterSheet.CreateStartingThief(characterRandom);
                sheet.Name = languageService.Current.WorldDatabase.ThiefName;
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ThiefPlayerSprite),
                    CharacterSheet = sheet,
                    StyleIndex = 2,
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(SwordCreator.CreateNormal(phase0TreasureLevel, nameof(ItemText.Sword1), nameof(Sword1), false), TreasureType.Weapon));
                GiveAndEquip(hero, new Treasure(DaggerCreator.CreateNormal(nameof(Dagger1), phase0TreasureLevel, nameof(ItemText.Dagger1), nameof(Steal)), TreasureType.OffHand));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateLeather(phase1TreasureLevel, nameof(ItemText.Leather1), EquipmentTier.Tier1, 1), TreasureType.Armor));
                new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel), TreasureType.Potion).GiveTo(hero.Inventory, null);
                new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel), TreasureType.Potion).GiveTo(hero.Inventory, null);
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = languageService.Current.WorldDatabase.ThiefGreeting,
                    Class = languageService.Current.WorldDatabase.ThiefClass,
                    Description = languageService.Current.WorldDatabase.ThiefDescription,
                };
            }

            {
                var sheet = CharacterSheet.CreateStartingSage(characterRandom);
                sheet.Name = languageService.Current.WorldDatabase.ClericName;
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ClericPlayerSprite),
                    CharacterSheet = sheet,
                    StyleIndex = 3,
                };
                hero.CharacterSheet.Rest();
                GiveAndEquip(hero, new Treasure(MaceCreator.CreateNormal(phase0TreasureLevel, nameof(ItemText.Hammer1), nameof(Hammer1), false), TreasureType.Weapon));
                GiveAndEquip(hero, new Treasure(BookCreator.CreateRestoration(nameof(Book1), phase0TreasureLevel, nameof(ItemText.Book1), nameof(Cure)), TreasureType.OffHand));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, nameof(ItemText.Cloth1), EquipmentTier.Tier1), TreasureType.Armor));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = languageService.Current.WorldDatabase.ClericGreeting,
                    Class = languageService.Current.WorldDatabase.ClericClass,
                    Description = languageService.Current.WorldDatabase.ClericDescription,
                };
            }
        }

        void GiveAndEquip(Persistence.CharacterData hero, Treasure treasure)
        {
            treasure.GiveTo(hero.Inventory, null);
            treasure.Use(hero.Inventory, hero.CharacterSheet, inventoryFunctions, null);
        }

        private IEnumerable<IAreaBuilder> SetupAreaBuilder(int seed, FIRandom biomeRandom, FIRandom placementRandom, FIRandom elementalRandom, FIRandom treasureRandom, FIRandom alignmentRandom, byte[,] usedSquares, bool[] usedIslands, csIslandMaze map)
        {
            //BiomeType.Desert is not being used
            var biomeDistributor = new EnumerableDistributor<BiomeType>(new[] { BiomeType.Forest, BiomeType.Snowy, BiomeType.Swamp });//This is not all of the biomes, some are added later

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
            var zoneAlignment = new RandomItemDistributor<Zone.Alignment>(new[] { Zone.Alignment.EastWest, Zone.Alignment.WestEast, Zone.Alignment.NorthSouth });

            List<int> specialIslands = new List<int>() { 0, 1, 2 }; //This is used to select from the items below
            IslandInfo blacksmithUpgradeIsland = null;
            IslandInfo alchemistUpgradeIsland = null;
            IslandInfo elementalStoneIsland = null;
            void AssignToSpecialIsland(IslandInfo theIsland)
            {
                if(specialIslands.Count == 0)
                {
                    throw new InvalidOperationException("Ran out of special islands.");
                }
                var randomNum = placementRandom.Next(specialIslands.Count);
                var specialIslandId = specialIslands[randomNum];
                specialIslands.RemoveAt(randomNum);
                switch (specialIslandId)
                {
                    case 0:
                        blacksmithUpgradeIsland = theIsland;
                        break;
                    case 1:
                        alchemistUpgradeIsland = theIsland;
                        break;
                    case 2:
                        elementalStoneIsland = theIsland;
                        break;
                }
            }

            var bigIsland = map.IslandInfo[map.IslandSizeOrder[0]];
            //Phase 0
            {
                var startingBiome = BiomeType.Countryside;

                var firstBoss = monsterInfo.Where(i => i.NativeBiome == startingBiome).First();
                var phase0Treasures = new List<Treasure>
                {
                    new Treasure(AccessoryCreator.CreateTargetScope(), TreasureType.Accessory),
                    new Treasure(PotionCreator.CreateFerrymansBribe(), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel), TreasureType.Potion),
                };

                //Area 1
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 0;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.Biome = startingBiome;
                areaBuilder.BossMonster = firstBoss;
                areaBuilder.Location = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
                areaBuilder.Treasure = phase0Treasures;
                areaBuilder.MakeRest = true;
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel), TreasureType.Potion)
                };
                areaBuilder.StartEnd = true;
                areaBuilder.HelpBookPlotItem = PlotItems.GuideToPowerAndMayhem;
                areaBuilder.MaxMainCorridorBattles = 1;
                areaBuilder.PartyMembers = CreateParty();
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled, usedSquares);
                PreventUsageNorthSouth(areaBuilder.Location, usedSquares);
                yield return areaBuilder;
            }

            {
                var phase2TreasureLevel = 35;
                var phase2Weapons = new List<Treasure>()
                {
                    new Treasure(SwordCreator.CreateNormal(phase2TreasureLevel, nameof(ItemText.Sword2), nameof(Sword2), true), TreasureType.Weapon),
                    new Treasure(SpearCreator.CreateNormal(phase2TreasureLevel, nameof(ItemText.Spear2), nameof(Spear2), true), TreasureType.Weapon),
                    new Treasure(MaceCreator.CreateNormal(phase2TreasureLevel, nameof(ItemText.Hammer2), nameof(Hammer2), true), TreasureType.Weapon),
                    new Treasure(BookCreator.CreateRestoration(nameof(Book2), phase2TreasureLevel, nameof(ItemText.Book2), nameof(MegaCure), nameof(BattleCry), nameof(Focus)), TreasureType.OffHand),
                    new Treasure(ShieldCreator.CreateNormal(phase2TreasureLevel, nameof(ItemText.Shield2), 0.35f, nameof(Shield2)), TreasureType.OffHand),
                    new Treasure(ElementalStaffCreator.CreateNormal(nameof(Staff2), phase2TreasureLevel, nameof(ItemText.Staff2), nameof(StrongFire), nameof(StrongIce), nameof(StrongLightning)), TreasureType.Weapon),
                };

                var phase2Armors = new List<Treasure>()
                {
                    new Treasure(ArmorCreator.CreatePlate(phase2TreasureLevel, nameof(ItemText.Plate2), EquipmentTier.Tier2), TreasureType.Armor),
                    new Treasure(ArmorCreator.CreateLeather(phase2TreasureLevel, nameof(ItemText.Leather2), EquipmentTier.Tier2, 2), TreasureType.Armor),
                    new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel, nameof(ItemText.Cloth2), EquipmentTier.Tier2), TreasureType.Armor),
                    new Treasure(ArmorCreator.CreateCloth(phase2TreasureLevel, nameof(ItemText.Cloth2), EquipmentTier.Tier2), TreasureType.Armor),
                };

                var phase2Accessories = new List<Treasure>()
                {
                    new Treasure(AccessoryCreator.CreateCounterAttack(), TreasureType.Accessory),
                    new Treasure(AccessoryCreator.CreateHealing(0.25f, true), TreasureType.Accessory),
                };

                var phase2Potions = new List<Treasure>()
                {
                    new Treasure(PotionCreator.CreateStrengthBoost(5), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateMagicBoost(5), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateSpiritBoost(4), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateVitalityBoost(4), TreasureType.StatBoost),
                };

                var phase2UniqueStolenTreasures = new List<Treasure>
                {
                    new Treasure(DaggerCreator.CreateNormal(nameof(Dagger2), phase2TreasureLevel, nameof(ItemText.Dagger2), nameof(Steal)), TreasureType.OffHand)
                    { Id = 200, FortuneText = "Assassin" }
                };

                const int zoneCount = 2;
                var weapons = Math.Max(1, phase2Weapons.Count / zoneCount);
                var armors = Math.Max(1, phase2Armors.Count / zoneCount);
                var accessories = Math.Max(1, phase2Accessories.Count / zoneCount);
                var potions = Math.Max(1, phase2Potions.Count / zoneCount);
                var stolenTreasure = Math.Max(1, phase2UniqueStolenTreasures.Count / zoneCount);

                //Area 3
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.PlotItem = PlotItems.AirshipFuel;
                areaBuilder.HelpBookPlotItem = PlotItems.GuideToPowerAndMayhemChapter4;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 2;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                areaBuilder.Monsters = elementalMonsters[GetElementForBiome(areaBuilder.Biome)]
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
                areaBuilder.Treasure =
                    RemoveRandomItems(phase2Weapons, treasureRandom, weapons)
                    .Concat(RemoveRandomItems(phase2Armors, treasureRandom, armors))
                    .Concat(RemoveRandomItems(phase2Accessories, treasureRandom, accessories))
                    .Concat(RemoveRandomItems(phase2Potions, treasureRandom, potions))
                    .Concat(new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase2TreasureLevel), TreasureType.Potion)
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase2UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel), TreasureType.Potion)
                };
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled, usedSquares);
                PreventUsageNorthSouth(areaBuilder.Location, usedSquares);
                yield return areaBuilder;

                //Area 4
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 2;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.PlotItem = PlotItems.AirshipWheel;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 2;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                areaBuilder.Monsters = elementalMonsters[GetElementForBiome(areaBuilder.Biome)]
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
                areaBuilder.Treasure =
                    RemoveRandomItems(phase2Weapons, treasureRandom, phase2Weapons.Count)
                    .Concat(RemoveRandomItems(phase2Armors, treasureRandom, phase2Armors.Count))
                    .Concat(RemoveRandomItems(phase2Accessories, treasureRandom, phase2Accessories.Count))
                    .Concat(RemoveRandomItems(phase2Potions, treasureRandom, phase2Potions.Count))
                    .Concat(new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase2TreasureLevel), TreasureType.Potion)
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase2UniqueStolenTreasures, treasureRandom, phase2UniqueStolenTreasures.Count); //Last area gets remaining treasure
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase2TreasureLevel), TreasureType.Potion)
                };
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled, usedSquares);
                PreventUsageNorthSouth(areaBuilder.Location, usedSquares);
                yield return areaBuilder;
            }

            //Phase 2
            //Add remaining biome types
            biomeDistributor = new EnumerableDistributor<BiomeType>(biomeDistributor.GetRemaining().Concat(new[] { BiomeType.Mountain, BiomeType.Beach }));

            {
                IslandInfo island;

                var phase3TreasureLevel = 55;
                var phase3Weapons = new List<Treasure>
                {
                    new Treasure(SwordCreator.CreateNormal(phase3TreasureLevel, nameof(ItemText.Sword3), nameof(Sword3), true), TreasureType.Weapon),
                    new Treasure(SpearCreator.CreateNormal(phase3TreasureLevel, nameof(ItemText.Spear3), nameof(Spear3), true), TreasureType.Weapon),
                    new Treasure(MaceCreator.CreateNormal(phase3TreasureLevel, nameof(ItemText.Hammer3), nameof(Hammer3), true), TreasureType.Weapon),
                    new Treasure(ElementalStaffCreator.CreateNormal(nameof(Staff3), phase3TreasureLevel, nameof(ItemText.Staff3), nameof(IonShread), nameof(ArchFire), nameof(ArchIce), nameof(ArchLightning)), TreasureType.Weapon),
                    new Treasure(ShieldCreator.CreateNormal(phase3TreasureLevel, nameof(ItemText.Shield3), 0.45f, nameof(Shield3)), TreasureType.OffHand),
                    new Treasure(BookCreator.CreateRestoration(nameof(Book3), phase3TreasureLevel, nameof(ItemText.Book3), nameof(UltraCure), nameof(Reanimate), nameof(WarCry), nameof(IntenseFocus)), TreasureType.OffHand),
                };

                var phase3Armors = new List<Treasure>
                {
                    new Treasure(ArmorCreator.CreatePlate(phase3TreasureLevel, nameof(ItemText.Plate3), EquipmentTier.Tier3), TreasureType.Armor),
                    new Treasure(ArmorCreator.CreateLeather(phase3TreasureLevel, nameof(ItemText.Leather3), EquipmentTier.Tier3, 3), TreasureType.Armor),
                    new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel, nameof(ItemText.Cloth3), EquipmentTier.Tier3), TreasureType.Armor),
                    new Treasure(ArmorCreator.CreateCloth(phase3TreasureLevel, nameof(ItemText.Cloth3), EquipmentTier.Tier3), TreasureType.Armor),
                };

                var phase3Accessories = new List<ITreasure>
                {
                    new Treasure(AccessoryCreator.CreateItemUsage(0.5f), TreasureType.Accessory),
                    new PlotItemTreasure(PlotItems.RuneOfIce, nameof(ItemText.RuneOfIce))
                };

                var phase3Potions = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateStrengthBoost(10), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateMagicBoost(10), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateSpiritBoost(10), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateVitalityBoost(10), TreasureType.StatBoost),
                };

                var phase3UniqueStolenTreasures = new List<ITreasure>
                {
                    new Treasure(DaggerCreator.CreateNormal(nameof(Dagger3), phase3TreasureLevel, nameof(ItemText.Dagger3), nameof(Steal), nameof(Haste)), TreasureType.OffHand)
                    { Id = 300, FortuneText = "Hourglass" },
                    new Treasure(PotionCreator.CreateLuckBoost(20), TreasureType.StatBoost)
                    { Id = 302, FortuneText = "Elephant" },
                };

                var zoneCount = 3;
                var weapons = Math.Max(1, phase3Weapons.Count / zoneCount);
                var armors = Math.Max(1, phase3Armors.Count / zoneCount);
                var accessories = Math.Max(1, phase3Accessories.Count / zoneCount);
                var potions = Math.Max(1, phase3Potions.Count / zoneCount);
                var stolenTreasure = Math.Max(1, phase3UniqueStolenTreasures.Count / zoneCount);

                Element firstMonsterElement;
                //Area 5
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                AssignToSpecialIsland(island);
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 0;
                areaBuilder.TorchZones = new[] { areaBuilder.StartZone };
                areaBuilder.HelpBookPlotItem = PlotItems.GuideToPowerAndMayhemChapter5;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 3;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure =
                    RemoveRandomItems(phase3Weapons, treasureRandom, weapons)
                    .Concat(RemoveRandomItems(phase3Armors, treasureRandom, armors))
                    .Concat(RemoveRandomItems(phase3Accessories, treasureRandom, accessories))
                    .Concat(RemoveRandomItems(phase3Potions, treasureRandom, potions))
                    .Concat(new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel), TreasureType.Potion)
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel), TreasureType.Potion)
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                PreventUsageNorthSouth(areaBuilder.Location, usedSquares);
                yield return areaBuilder;

                //Area 6
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                AssignToSpecialIsland(island);
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 1;
                areaBuilder.TorchZones = new[] { areaBuilder.StartZone };
                areaBuilder.HelpBookPlotItem = PlotItems.GuideToPowerAndMayhemChapter6;
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 3;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure =
                    RemoveRandomItems(phase3Weapons, treasureRandom, weapons)
                    .Concat(RemoveRandomItems(phase3Armors, treasureRandom, armors))
                    .Concat(RemoveRandomItems(phase3Accessories, treasureRandom, accessories))
                    .Concat(RemoveRandomItems(phase3Potions, treasureRandom, potions))
                    .Concat(new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel), TreasureType.Potion)
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, stolenTreasure);
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel), TreasureType.Potion)
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                PreventUsageNorthSouth(areaBuilder.Location, usedSquares);
                yield return areaBuilder;

                //Area 7
                island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
                AssignToSpecialIsland(island);
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 2;
                areaBuilder.GateZones = new[] { areaBuilder.StartZone };
                areaBuilder.TorchZones = new[] { areaBuilder.StartZone };
                areaBuilder.Biome = biomeDistributor.GetNext(biomeRandom);
                areaBuilder.MaxMainCorridorBattles = 3;
                areaBuilder.Alignment = zoneAlignment.GetItem(alignmentRandom);
                firstMonsterElement = GetElementForBiome(areaBuilder.Biome);
                areaBuilder.Monsters = elementalMonsters[GetRandomMagicElement(elementalRandom)]
                    .Concat(elementalMonsters[GetRandomMagicElement(elementalRandom, firstMonsterElement)])
                    .Where(i => i.NativeBiome == areaBuilder.Biome).ToList();
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.Treasure =
                    RemoveRandomItems(phase3Weapons, treasureRandom, phase3Weapons.Count)
                    .Concat(RemoveRandomItems(phase3Armors, treasureRandom, phase3Armors.Count))
                    .Concat(RemoveRandomItems(phase3Accessories, treasureRandom, phase3Accessories.Count))
                    .Concat(RemoveRandomItems(phase3Potions, treasureRandom, phase3Potions.Count))
                    .Concat(new[]
                    {
                        new Treasure(PotionCreator.CreateHealthPotion(phase3TreasureLevel), TreasureType.Potion)
                    });
                areaBuilder.UniqueStealTreasure = RemoveRandomItems(phase3UniqueStolenTreasures, treasureRandom, phase3UniqueStolenTreasures.Count); //Last area gets the remaining treasure
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase3TreasureLevel), TreasureType.Potion)
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                PreventUsageNorthSouth(areaBuilder.Location, usedSquares);
                yield return areaBuilder;
            }

            //Phase 4
            {
                var phase4TreasureLevel = 60;

                var phase4UniqueTreasures = new List<Treasure>()
                {
                    new Treasure(PotionCreator.CreateLevelBoost(), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateLevelBoost(), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateLevelBoost(), TreasureType.StatBoost),
                    new Treasure(PotionCreator.CreateLevelBoost(), TreasureType.StatBoost),
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
                areaBuilder.Alignment = Zone.Alignment.SouthNorth;
                areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
                areaBuilder.IsFinalArea = true;
                areaBuilder.NumEmptyRooms = 5;

                //You get all the monsters in this zone
                areaBuilder.Monsters = monsterInfo;
                foreach (var monsters in elementalMonsters.Values)
                {
                    areaBuilder.Monsters = areaBuilder.Monsters.Concat(monsters);
                }
                areaBuilder.Monsters = areaBuilder.Monsters.Where(i => i.NativeBiome != BiomeType.FinalBoss).ToList();
                areaBuilder.BossMonster = monsterInfo.Where(i => i.NativeBiome == BiomeType.FinalBoss).First();
                areaBuilder.Treasure = phase4UniqueTreasures;
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase4TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase4TreasureLevel), TreasureType.Potion),
                    new Treasure(PotionCreator.CreateManaPotion(phase4TreasureLevel), TreasureType.Potion),
                };
                SetIslandBiome(island, map, areaBuilder.Biome);
                PreventUsageNorthSouth(areaBuilder.Location, usedSquares);
                yield return areaBuilder;
            }

            //Make it less likely to place a npc on the main island if there is water nearby
            const byte NpcIgnoreSquare = byte.MaxValue - 1;
            var modifiedSquares = new List<Tuple<IntVector2, byte>>(bigIsland.islandPoints.Count);
            foreach (var square in bigIsland.islandPoints)
            {
                //Look for water in any direction
                var left = square.x - 1;
                var right = square.x + 1;
                var top = square.y + 1;
                var bottom = square.y - 1;
                if (left > 0 && right < map.Map.GetLength(0) && top < map.Map.GetLength(1) && bottom > 0)
                {
                    var nearbyCellEmpty = map.Map[left, square.y] == csIslandMaze.EmptyCell
                         || map.Map[right, square.y] == csIslandMaze.EmptyCell
                         || map.Map[square.x, top] == csIslandMaze.EmptyCell
                         || map.Map[square.x, bottom] == csIslandMaze.EmptyCell
                         || map.Map[left, top] == csIslandMaze.EmptyCell
                         || map.Map[right, top] == csIslandMaze.EmptyCell
                         || map.Map[left, bottom] == csIslandMaze.EmptyCell
                         || map.Map[right, bottom] == csIslandMaze.EmptyCell;

                    if (nearbyCellEmpty && usedSquares[square.x, square.y] < MaxFillGeneration)
                    {
                        modifiedSquares.Add(Tuple.Create(square, usedSquares[square.x, square.y]));
                        usedSquares[square.x, square.y] = NpcIgnoreSquare;
                    }
                }
            }

            InnkeeperPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            PreventUsageNorthSouth(InnkeeperPosition, usedSquares);
            BlacksmithPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            PreventUsageNorthSouth(BlacksmithPosition, usedSquares);
            AlchemistPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            PreventUsageNorthSouth(AlchemistPosition, usedSquares);
            FortuneTellerPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            PreventUsageNorthSouth(FortuneTellerPosition, usedSquares);
            ItemStoragePosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            PreventUsageNorthSouth(ItemStoragePosition, usedSquares);

            BlacksmithUpgradePosition = GetUnusedSquare(usedSquares, blacksmithUpgradeIsland, placementRandom);
            PreventUsageNorthSouth(BlacksmithUpgradePosition, usedSquares);
            AlchemistUpgradePosition = GetUnusedSquare(usedSquares, alchemistUpgradeIsland, placementRandom);
            PreventUsageNorthSouth(AlchemistUpgradePosition, usedSquares);
            ElementalStonePosition = GetUnusedSquare(usedSquares, elementalStoneIsland, placementRandom);
            PreventUsageNorthSouth(ElementalStonePosition, usedSquares);

            //Reset npc ignore squares so background items can be placed again
            //These can be changed above, so only restore if it is still equal to NpcIgnoreSquare
            foreach (var item in modifiedSquares)
            {
                var square = item.Item1;
                if (usedSquares[square.x, square.y] == NpcIgnoreSquare)
                {
                    usedSquares[square.x, square.y] = item.Item2;
                }
            }

            var biomePropLocations = new List<IntVector2>();
            foreach(var island in map.IslandInfo)
            {
                var islandPropLocationCount = island.islandPoints.Count / 5;
                for(int i = 0; i < islandPropLocationCount; i++)
                {
                    biomePropLocations.Add(GetUnusedSquare(usedSquares, island, placementRandom, MaxFillGeneration));
                }
            }
            this.BiomePropLocations = biomePropLocations;
        }

        private static void PreventUsageNorthSouth(IntVector2 startPos, byte[,] usedSquares, int num = 4)
        {
            //South
            var x = startPos.x;
            var startY = Math.Max(startPos.y - num, 0);
            var endY = startPos.y;
            for(int y = startY; y < endY; ++y)
            {
                usedSquares[x, y] = byte.MaxValue;
            }

            //North
            startY = startPos.y + 1;
            endY = Math.Min(startPos.y + num + 1, usedSquares.GetLength(1));
            for (int y = startY; y < endY; ++y)
            {
                usedSquares[x, y] = byte.MaxValue;
            }
        }

        /// <summary>
        /// Get the extremes of the island. The returned results are scrambled, so you can access the same indexes externally, but the values will not
        /// always be the same. The pairs returned will be at either extreme, northmost and southmost or eastmost and westmost, but you won't know 
        /// what is in what index.
        /// </summary>
        private static List<List<IntVector2>> GetIslandExtremes(IslandInfo island, FIRandom placementRandom, byte[,] usedSquares)
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

        public IEnumerable<ShopEntry> CreateShopItems(ShopType shopType, HashSet<PlotItems> plotItems)
        {
            switch (shopType)
            {
                case ShopType.Alchemist:
                    if (plotItems.Contains(PlotItems.AlchemistUpgrade))
                    {
                        yield return new ShopEntry(nameof(ItemText.Mana3), 135, false, () => PotionCreator.CreateManaPotion(55));
                    }

                    yield return new ShopEntry(nameof(ItemText.Mana2), 70, false, () => PotionCreator.CreateManaPotion(35));
                    yield return new ShopEntry(nameof(ItemText.FerrymansBribe), 350, false, () => PotionCreator.CreateFerrymansBribe());

                    if(!plotItems.Contains(PlotItems.RuneOfElectricity))
                    {
                        yield return new ShopEntry(nameof(ItemText.RuneOfElectricity), 1500, false, null, PlotItems.RuneOfElectricity);
                    }

                    break;

                case ShopType.Blacksmith:
                    if (plotItems.Contains(PlotItems.BlacksmithUpgrade))
                    {
                        var treasureLevel = 40;
                        yield return new ShopEntry(nameof(ItemText.StoreSword2), 350, true, () => SwordCreator.CreateNormal(treasureLevel, nameof(ItemText.StoreSword2), nameof(Scimitar), false));
                        yield return new ShopEntry(nameof(ItemText.StoreSpear2), 350, true, () => SpearCreator.CreateNormal(treasureLevel, nameof(ItemText.StoreSpear2), nameof(Trident), false));
                        yield return new ShopEntry(nameof(ItemText.StoreHammer2), 350, true, () => MaceCreator.CreateNormal(treasureLevel, nameof(ItemText.StoreHammer2), nameof(Club), false));
                    }

                    {
                        var treasureLevel = 15;
                        yield return new ShopEntry(nameof(ItemText.StoreSword1), 150, true, () => SwordCreator.CreateNormal(treasureLevel, nameof(ItemText.StoreSword1), nameof(SmithedSword), false));
                        yield return new ShopEntry(nameof(ItemText.StoreSpear1), 150, true, () => SpearCreator.CreateNormal(treasureLevel, nameof(ItemText.StoreSpear1), nameof(SmithedSpear), false));
                        yield return new ShopEntry(nameof(ItemText.StoreHammer1), 150, true, () => MaceCreator.CreateNormal(treasureLevel, nameof(ItemText.StoreHammer1), nameof(SmithedHammer), false));
                    }
                    break;
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

        private static IntVector2 GetUnusedSquare(byte[,] usedSquares, IslandInfo island, FIRandom placementRandom, IntVector2 desired, byte startLevel = 1)
        {
            if (usedSquares[desired.x, desired.y] == 0)
            {
                usedSquares[desired.x, desired.y] = byte.MaxValue;
                return desired;
            }

            return GetUnusedSquare(usedSquares, island, placementRandom, startLevel);
        }

        private static IntVector2 GetUnusedSquare(byte[,] usedSquares, IslandInfo island, FIRandom placementRandom, byte startLevel = 1)
        {
            //This will give you a square from level or below, so any lower levels can be included.
            //This skips 255, but that is ok since you would never select one of those anyway
            for (var level = startLevel; level < byte.MaxValue; ++level)
            {
                for (var i = 0; i < 5; ++i)
                {
                    var next = placementRandom.Next(0, island.Size);
                    var square = island.islandPoints[next];
                    if (usedSquares[square.x, square.y] < level)
                    {
                        usedSquares[square.x, square.y] = byte.MaxValue;
                        return square;
                    }
                }

                foreach (var square in island.islandPoints)
                {
                    if (usedSquares[square.x, square.y] < level)
                    {
                        usedSquares[square.x, square.y] = byte.MaxValue;
                        return square;
                    }
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
                case BiomeType.Volcano:
                    return 6;
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

        private static void FillSurroundings(csIslandMaze map, BiomeType biome, IntVector2 startPoint, bool[,] filled, byte[,] usedSquares)
        {
            //The start point will always be filled out even if its already filled
            map.TextureOffsets[startPoint.x, startPoint.y] = GetBiomeIndex(biome);
            filled[startPoint.x, startPoint.y] = true;

            var nextGeneration = new List<IntVector2>(25);
            var currentGeneration = new List<IntVector2>(25) { startPoint };

            for (byte gen = 0; gen < MaxFillGeneration && currentGeneration.Count > 0; ++gen)
            {
                byte usedSquareValue = (byte)(MaxFillGeneration - gen);
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
                        usedSquares[check.x, check.y] = Math.Max(usedSquareValue, usedSquares[check.x, check.y]);
                    }

                    check = item;
                    --check.y;
                    if (check.y > 0 && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = GetBiomeIndex(biome);
                        filled[check.x, check.y] = true;
                        usedSquares[check.x, check.y] = Math.Max(usedSquareValue, usedSquares[check.x, check.y]);
                    }

                    check = item;
                    ++check.x;
                    if (check.x < map.MapX && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = GetBiomeIndex(biome);
                        filled[check.x, check.y] = true;
                        usedSquares[check.x, check.y] = Math.Max(usedSquareValue, usedSquares[check.x, check.y]);
                    }

                    check = item;
                    --check.x;
                    if (check.x > 0 && filled[check.x, check.y] == false && map.Map[check.x, check.y] != csIslandMaze.EmptyCell)
                    {
                        nextGeneration.Add(check);
                        map.TextureOffsets[check.x, check.y] = GetBiomeIndex(biome);
                        filled[check.x, check.y] = true;
                        usedSquares[check.x, check.y] = Math.Max(usedSquareValue, usedSquares[check.x, check.y]);
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
            if (items.Count > 0)
            {
                for (int i = 0; i < count; ++i)
                {
                    results.Add(RemoveRandomItem(items, random));
                }
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
