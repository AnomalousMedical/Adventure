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
        IntVector2 InkeeperPosition { get; }
        IntVector2 BlacksmithPosition { get; }
        IntVector2 AlchemistPosition { get; }
        IntVector2 BlacksmithUpgradePosition { get; }
        IntVector2 AlchemistUpgradePosition { get; }
        IntVector2 FortuneTellerPosition { get; }
        IntVector2 ElementalStonePosition { get; }
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
        private IntVector2 airshipStartSquare;

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
        public IntVector2 InkeeperPosition { get; private set; }
        public IntVector2 BlacksmithPosition { get; private set; }
        public IntVector2 AlchemistPosition { get; private set; }
        public IntVector2 BlacksmithUpgradePosition { get; private set; }
        public IntVector2 AlchemistUpgradePosition { get; private set; }
        public IntVector2 FortuneTellerPosition { get; private set; }
        public IntVector2 ElementalStonePosition { get; private set; }

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
            var usedSquares = new bool[map.MapX, map.MapY];
            var usedIslands = new bool[map.NumIslands];

            //Reserve the 3 largest islands
            usedIslands[map.IslandSizeOrder[0]] = true;

            var islandIndex = map.IslandSizeOrder[0];
            var island = map.IslandInfo[islandIndex];
            usedIslands[islandIndex] = true;
            airshipStartSquare = GetUnusedSquare(usedSquares, island, placementRandom);
            usedSquares[airshipStartSquare.x, airshipStartSquare.y] = true;

            areaBuilders = SetupAreaBuilder(newSeed, biomeRandom, placementRandom, elementalRandom, treasureRandom, alignmentRandom, usedSquares, usedIslands, map).ToList();
        }

        const int phase0TreasureLevel = 10;
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
                GiveAndEquip(hero, new Treasure(SpearCreator.CreateNormal(phase0TreasureLevel, "Rusty", nameof(Spear1))));
                GiveAndEquip(hero, new Treasure(ShieldCreator.CreateNormal(phase0TreasureLevel, "Buckler", 0.15f, nameof(Shield1))));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreatePlate(phase1TreasureLevel, "Tarnished", EquipmentTier.Tier1)));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = "My shield will guard us.",
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
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, "Worn", EquipmentTier.Tier1)));
                GiveAndEquip(hero, new Treasure(ElementalStaffCreator.CreateNormal(nameof(Staff1), phase0TreasureLevel, "Cracked", nameof(Fire), nameof(Ice), nameof(Lightning))));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = "Let's get moving.",
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
                GiveAndEquip(hero, new Treasure(SwordCreator.CreateNormal(phase0TreasureLevel, "Busted", nameof(Sword1))));
                GiveAndEquip(hero, new Treasure(DaggerCreator.CreateNormal(nameof(Dagger1), phase0TreasureLevel, "Rusty", nameof(Steal))));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateLeather(phase1TreasureLevel, "Cracked", EquipmentTier.Tier1)));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = "I hope we find lots of great treasure!",
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
                GiveAndEquip(hero, new Treasure(MaceCreator.CreateNormal(phase0TreasureLevel, "Rusty", nameof(Hammer1))));
                GiveAndEquip(hero, new Treasure(BookCreator.CreateRestoration(nameof(Book1), phase0TreasureLevel, "Torn", nameof(Cure))));
                GiveAndEquip(hero, new Treasure(ArmorCreator.CreateCloth(phase1TreasureLevel, "Worn", EquipmentTier.Tier1)));
                yield return new PartyMember
                {
                    CharacterData = hero,
                    Greeting = "I wonder what's made everything so agressive?",
                };
            }
        }

        void GiveAndEquip(Persistence.CharacterData hero, Treasure treasure)
        {
            treasure.GiveTo(hero.Inventory, null);
            treasure.Use(hero.Inventory, hero.CharacterSheet, inventoryFunctions, null);
        }

        private IEnumerable<IAreaBuilder> SetupAreaBuilder(int seed, FIRandom biomeRandom, FIRandom placementRandom, FIRandom elementalRandom, FIRandom treasureRandom, FIRandom alignmentRandom, bool[,] usedSquares, bool[] usedIslands, csIslandMaze map)
        {
            var biomes = new List<BiomeType>() { BiomeType.Forest, BiomeType.Snowy, BiomeType.Beach, BiomeType.Swamp, BiomeType.Mountain }; //BiomeType.Desert, 
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

            IslandInfo blacksmithUpgradeIsland;
            IslandInfo alchemistUpgradeIsland;
            IslandInfo elementalStoneIsland;

            var bigIsland = map.IslandInfo[map.IslandSizeOrder[0]];
            //Phase 0
            {
                var startingBiome = BiomeType.Countryside;

                var firstBoss = monsterInfo.Where(i => i.NativeBiome == startingBiome).First();
                var phase0Treasures = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel)),
                    new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)),
                    new Treasure(PotionCreator.CreateHealthPotion(phase0TreasureLevel)),
                    new Treasure(AccessoryCreator.CreateTargetScope()),
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
                areaBuilder.StealTreasure = new[]
                {
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel)),
                    new Treasure(PotionCreator.CreateManaPotion(phase0TreasureLevel))
                };
                areaBuilder.StartEnd = true;
                areaBuilder.MaxMainCorridorBattles = 1;
                areaBuilder.PartyMembers = CreateParty().Skip(1);
                FillSurroundings(map, areaBuilder.Biome, areaBuilder.Location, filled);
                yield return areaBuilder;
            }

            {
                var phase2TreasureLevel = 35;
                var phase2Adjective = "Common";
                var phase2Weapons = new List<Treasure>()
                {
                    new Treasure(SwordCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, nameof(Sword2))),
                    new Treasure(SpearCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, nameof(Spear2))),
                    new Treasure(MaceCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, nameof(Hammer2))),
                    new Treasure(BookCreator.CreateRestoration(nameof(Book2), phase2TreasureLevel, phase2Adjective, nameof(MegaCure), nameof(BattleCry), nameof(Focus))),
                    new Treasure(ShieldCreator.CreateNormal(phase2TreasureLevel, phase2Adjective, 0.35f, nameof(Shield2))),
                    new Treasure(ElementalStaffCreator.CreateNormal(nameof(Staff2), phase2TreasureLevel, "Mage's", nameof(StrongFire), nameof(StrongIce), nameof(StrongLightning))),
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
                    new Treasure(AccessoryCreator.CreateHealing(0.25f, true)),
                };

                var phase2Potions = new List<Treasure>()
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(PotionCreator.CreateStrengthBoost(5)),
                    new Treasure(PotionCreator.CreateMagicBoost(5)),
                    new Treasure(PotionCreator.CreateSpiritBoost(4)),
                    new Treasure(PotionCreator.CreateVitalityBoost(4)),
                };

                var phase2UniqueStolenTreasures = new List<Treasure>
                {
                    new Treasure(DaggerCreator.CreateNormal(nameof(Dagger2), phase2TreasureLevel, phase2Adjective, nameof(Steal)))
                    { Id = 200, FortuneText = "Assassin" }
                };

                const int zoneCount = 2;
                var weapons = phase2Weapons.Count / zoneCount;
                var armors = phase2Armors.Count / zoneCount;
                var accessories = phase2Accessories.Count / zoneCount;
                var potions = phase2Potions.Count / zoneCount;
                var stolenTreasure = phase2UniqueStolenTreasures.Count / zoneCount;

                //Area 3
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
                areaBuilder.Location = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
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
                areaBuilder.Location = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
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

            //Phase 2
            {
                IslandInfo island;

                var phase3TreasureLevel = 55;
                var phase3Adjective = "Superior";
                var phase3Weapons = new List<Treasure>
                {
                    new Treasure(SwordCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, nameof(Sword3))),
                    new Treasure(SpearCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, nameof(Spear3))),
                    new Treasure(MaceCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, nameof(Hammer3))),
                    new Treasure(ElementalStaffCreator.CreateNormal(nameof(Staff3), phase3TreasureLevel, "Arch Mage's", nameof(IonShread), nameof(ArchFire), nameof(ArchIce), nameof(ArchLightning))),
                    new Treasure(ShieldCreator.CreateNormal(phase3TreasureLevel, phase3Adjective, 0.45f, nameof(Shield3))),
                    new Treasure(BookCreator.CreateRestoration(nameof(Book3), phase3TreasureLevel, phase3Adjective, nameof(UltraCure), nameof(Reanimate), nameof(WarCry), nameof(IntenseFocus))),
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
                    new Treasure(AccessoryCreator.CreateItemUsage(0.5f)),
                };

                var phase3Potions = new List<Treasure>
                {
                    new Treasure(PotionCreator.CreateFerrymansBribe()),
                    new Treasure(PotionCreator.CreateStrengthBoost(5)),
                    new Treasure(PotionCreator.CreateStrengthBoost(5)),
                    new Treasure(PotionCreator.CreateMagicBoost(5)),
                    new Treasure(PotionCreator.CreateMagicBoost(5)),
                    new Treasure(PotionCreator.CreateSpiritBoost(4)),
                    new Treasure(PotionCreator.CreateSpiritBoost(4)),
                    new Treasure(PotionCreator.CreateVitalityBoost(4)),
                    new Treasure(PotionCreator.CreateVitalityBoost(4)),
                };

                var phase3UniqueStolenTreasures = new List<ITreasure>
                {
                    new Treasure(DaggerCreator.CreateNormal(nameof(Dagger3), phase3TreasureLevel, phase3Adjective, nameof(Steal), nameof(Haste)))
                    { Id = 300, FortuneText = "Hourglass" },
                    new Treasure(PotionCreator.CreateLuckBoost(20))
                    { Id = 302, FortuneText = "Elephant" },
                    new PlotItemTreasure(PlotItems.RuneOfIce, "Rune of Ice")
                    { Id = 303, FortuneText = "Ice" }

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
                blacksmithUpgradeIsland = island;
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 0;
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
                alchemistUpgradeIsland = island;
                areaBuilder = new AreaBuilder(this, monsterInfo, area++);
                areaBuilder.StartZone = zoneCounter.GetZoneStart();
                areaBuilder.EndZone = zoneCounter.GetZoneEnd(1);
                areaBuilder.Phase = 3;
                areaBuilder.IndexInPhase = 1;
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
                elementalStoneIsland = island;
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
            
            InkeeperPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            BlacksmithPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            AlchemistPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);
            FortuneTellerPosition = GetUnusedSquare(usedSquares, bigIsland, placementRandom);

            BlacksmithUpgradePosition = GetUnusedSquare(usedSquares, blacksmithUpgradeIsland, placementRandom);
            AlchemistUpgradePosition = GetUnusedSquare(usedSquares, alchemistUpgradeIsland, placementRandom);
            ElementalStonePosition = GetUnusedSquare(usedSquares, elementalStoneIsland, placementRandom);
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

        public IEnumerable<ShopEntry> CreateShopItems(ShopType shopType, HashSet<PlotItems> plotItems)
        {
            switch (shopType)
            {
                case ShopType.Alchemist:
                    if (plotItems.Contains(PlotItems.AlchemistUpgrade))
                    {
                        yield return new ShopEntry("Giant Mana Potion", 135, () => PotionCreator.CreateManaPotion(55));
                    }

                    yield return new ShopEntry("Big Mana Potion", 70, () => PotionCreator.CreateManaPotion(35));
                    yield return new ShopEntry("Ferryman's Bribe", 350, () => PotionCreator.CreateFerrymansBribe());

                    if(!plotItems.Contains(PlotItems.RuneOfElectricity))
                    {
                        yield return new ShopEntry("Rune of Electricity", 1500, null, PlotItems.RuneOfElectricity);
                    }

                    break;

                case ShopType.Blacksmith:
                    if (plotItems.Contains(PlotItems.BlacksmithUpgrade))
                    {
                        var treasureLevel = 40;
                        var adjective = "Fancy Store Bought";
                        yield return new ShopEntry($"{adjective} Sword", 350, () => SwordCreator.CreateNormal(treasureLevel, adjective, nameof(Sword1)));
                        yield return new ShopEntry($"{adjective} Spear", 350, () => SpearCreator.CreateNormal(treasureLevel, adjective, nameof(Spear1)));
                        yield return new ShopEntry($"{adjective} Mace", 350, () => MaceCreator.CreateNormal(treasureLevel, adjective, nameof(Hammer1)));
                    }

                    {
                        var treasureLevel = 15;
                        var adjective = "Store Bought";
                        yield return new ShopEntry($"{adjective} Sword", 150, () => SwordCreator.CreateNormal(treasureLevel, adjective, nameof(Sword1)));
                        yield return new ShopEntry($"{adjective} Spear", 150, () => SpearCreator.CreateNormal(treasureLevel, adjective, nameof(Spear1)));
                        yield return new ShopEntry($"{adjective} Mace", 150, () => MaceCreator.CreateNormal(treasureLevel, adjective, nameof(Hammer1)));
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
