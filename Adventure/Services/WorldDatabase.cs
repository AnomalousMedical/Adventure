﻿using Adventure.Items.Creators;
using Engine;
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
        IBiomeManager BiomeManager { get; }
        SwordCreator SwordCreator { get; }
        SpearCreator SpearCreator { get; }
        MaceCreator MaceCreator { get; }
        ShieldCreator ShieldCreator { get; }
        FireStaffCreator FireStaffCreator { get; }
        ElementalStaffCreator ElementalStaffCreator { get; }
        AccessoryCreator AccessoryCreator { get; }
        ArmorCreator ArmorCreator { get; }
        PotionCreator PotionCreator { get; }
        AxeCreator AxeCreator { get; }
        DaggerCreator DaggerCreator { get; }
        IMonsterMaker MonsterMaker { get; }
        WorldMapData WorldMap { get; }
        List<IAreaBuilder> AreaBuilders { get; }
        List<IntVector2> PortalLocations { get; }
        int CurrentSeed { get; }
        IntVector2 AirshipStartSquare { get; }
        IntVector2 AirshipPortalSquare { get; }
    }

    class WorldDatabase : IWorldDatabase
    {
        private List<IAreaBuilder> areaBuilders;
        private List<int> createdZoneSeeds;
        private int currentSeed;
        private Random zoneRandom;
        private readonly Persistence persistence;
        private List<IntVector2> portalLocations;
        private IntVector2 airshipStartSquare;
        private IntVector2 airshipPortalSquare;

        public IBiomeManager BiomeManager { get; }
        public SwordCreator SwordCreator { get; }
        public SpearCreator SpearCreator { get; }
        public MaceCreator MaceCreator { get; }
        public ShieldCreator ShieldCreator { get; }
        public FireStaffCreator FireStaffCreator { get; }
        public ElementalStaffCreator ElementalStaffCreator { get; }
        public AccessoryCreator AccessoryCreator { get; }
        public ArmorCreator ArmorCreator { get; }
        public PotionCreator PotionCreator { get; }
        public AxeCreator AxeCreator { get; }
        public DaggerCreator DaggerCreator { get; }
        public IMonsterMaker MonsterMaker { get; }
        public List<IntVector2> PortalLocations => portalLocations;
        public IntVector2 AirshipStartSquare => airshipStartSquare;
        public IntVector2 AirshipPortalSquare => airshipPortalSquare;

        private WorldMapData worldMap;
        public WorldMapData WorldMap
        {
            get
            {
                CheckSeed();

                return worldMap;
            }
        }

        public List<IAreaBuilder> AreaBuilders
        {
            get
            {
                CheckSeed();

                return areaBuilders;
            }
        }

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
            FireStaffCreator fireStaffCreator,
            ElementalStaffCreator elementalStaffCreator,
            AccessoryCreator accessoryCreator,
            ArmorCreator armorCreator,
            PotionCreator potionCreator,
            AxeCreator axeCreator,
            DaggerCreator daggerCreator
        )
        {
            this.persistence = persistence;
            this.MonsterMaker = monsterMaker;
            BiomeManager = biomeManager;
            SwordCreator = swordCreator;
            SpearCreator = spearCreator;
            MaceCreator = maceCreator;
            ShieldCreator = shieldCreator;
            FireStaffCreator = fireStaffCreator;
            ElementalStaffCreator = elementalStaffCreator;
            AccessoryCreator = accessoryCreator;
            ArmorCreator = armorCreator;
            PotionCreator = potionCreator;
            AxeCreator = axeCreator;
            DaggerCreator = daggerCreator;
            Reset(persistence.Current.World.Seed);
        }

        public int GetZoneSeed(int zoneIndex)
        {
            CheckSeed();

            var end = zoneIndex + 1;
            for (var i = createdZoneSeeds.Count; i < end; ++i)
            {
                createdZoneSeeds.Add(zoneRandom.Next(int.MinValue, int.MaxValue));
            }
            return createdZoneSeeds[zoneIndex];
        }

        public IAreaBuilder GetAreaBuilder(int zoneIndex)
        {
            CheckSeed();

            foreach (var area in areaBuilders)
            {
                if (zoneIndex >= area.StartZone && zoneIndex <= area.EndZone)
                {
                    return area;
                }
            }

            return areaBuilders[0];
        }

        private void CheckSeed()
        {
            if (persistence.Current.World.Seed != currentSeed)
            {
                Reset(persistence.Current.World.Seed);
            }
        }

        private void Reset(int newSeed)
        {
            //Setup seeds and randoms
            createdZoneSeeds = new List<int>();
            zoneRandom = new Random(newSeed);
            var biomeRandom = new Random(newSeed);
            var placementRandom = new Random(newSeed);
            currentSeed = newSeed;

            //Setup map
            worldMap = new WorldMapData(newSeed);
            var numIslands =  1  //Phase 0, 1 and bonus 1
                            + 2  //Phase 2
                            + 3  //Bonus 2
                            + 6  //Phase 3
                            + 6  //Bonus 3
                            + 1; //Airship
            worldMap.Map.RemoveExtraIslands(numIslands);
            var map = worldMap.Map;
            //TODO: need to check maps
            //3 largest islands need to have enough spaces for each phase
            //World needs enough islands to cover all zones

            //Setup areas
            var weaknessRandom = new Random(newSeed);
            var monsterInfo = MonsterMaker.CreateBaseMonsters(weaknessRandom);
            var usedSquares = new bool[map.MapX, map.MapY];
            var usedIslands = new bool[map.NumIslands];
            portalLocations = new List<IntVector2>(5);

            //Reserve the 3 largest islands
            usedIslands[map.IslandSizeOrder[0]] = true;
            usedIslands[map.IslandSizeOrder[1]] = true;
            usedIslands[map.IslandSizeOrder[2]] = true;

            SetupAirshipIsland(placementRandom, out airshipStartSquare, out airshipPortalSquare, usedSquares, usedIslands, map);
            areaBuilders = SetupAreaBuilder(monsterInfo, biomeRandom, placementRandom, portalLocations, usedSquares, usedIslands, map).ToList();
        }

        private static void SetupAirshipIsland(Random placementRandom, out IntVector2 airshipSquare, out IntVector2 airshipPortalSquare, bool[,] usedSquares, bool[] usedIslands, csIslandMaze map)
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

        private IEnumerable<IAreaBuilder> SetupAreaBuilder(IList<MonsterInfo> monsterInfo, Random biomeRandom, Random placementRandom, List<IntVector2> portalLocations, bool[,] usedSquares, bool[] usedIslands, csIslandMaze map)
        {
            int area = 0;
            AreaBuilder areaBuilder;
            var biomeMax = (int)BiomeType.Max;

            AddPortal(map.IslandInfo[map.IslandSizeOrder[0]], usedSquares, placementRandom, portalLocations);
            AddPortal(map.IslandInfo[map.IslandSizeOrder[1]], usedSquares, placementRandom, portalLocations);
            AddPortal(map.IslandInfo[map.IslandSizeOrder[2]], usedSquares, placementRandom, portalLocations);

            var island = map.IslandInfo[map.IslandSizeOrder[0]];

            //Phase 0
            areaBuilder = new Area0Builder(this, monsterInfo, area++);
            areaBuilder.StartZone = 0;
            areaBuilder.EndZone = 1;
            areaBuilder.Phase = 0;
            areaBuilder.IndexInPhase = 0;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Westmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            //Phase 1
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 2;
            areaBuilder.EndZone = 3;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 1;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Eastmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 4;
            areaBuilder.EndZone = 5;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 2;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Northmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 6;
            areaBuilder.EndZone = 7;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 3;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 8;
            areaBuilder.EndZone = 9;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 4;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            //Phase 2
            island = map.IslandInfo[map.IslandSizeOrder[1]];

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 10;
            areaBuilder.EndZone = 11;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 0;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Eastmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 12;
            areaBuilder.EndZone = 13;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 1;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Westmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 14;
            areaBuilder.EndZone = 15;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 2;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            island = map.IslandInfo[map.IslandSizeOrder[2]];

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 16;
            areaBuilder.EndZone = 17;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 18;
            areaBuilder.EndZone = 19;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 4;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Eastmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 20;
            areaBuilder.EndZone = 21;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 5;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Westmost);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            //Phase 3
            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 22;
            areaBuilder.EndZone = 23;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 0;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 24;
            areaBuilder.EndZone = 25;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 1;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 26;
            areaBuilder.EndZone = 27;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 2;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 28;
            areaBuilder.EndZone = 29;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 30;
            areaBuilder.EndZone = 31;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 4;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 32;
            areaBuilder.EndZone = 33;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 5;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            //Don't return this for now
            ////Phase 4
            //areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            //areaBuilder.StartZone = 34;
            //areaBuilder.EndZone = 36;
            //areaBuilder.Phase = 4;
            //areaBuilder.IndexInPhase = 0;
            //areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            //yield return areaBuilder;

            //Bonus 1
            island = map.IslandInfo[map.IslandSizeOrder[0]];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 37;
            areaBuilder.EndZone = 39;
            areaBuilder.Phase = 1;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            map.TextureOffsets[areaBuilder.Location.x, areaBuilder.Location.y] = (int)areaBuilder.Biome;
            yield return areaBuilder;

            //Bonus 2
            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            AddPortal(island, usedSquares, placementRandom, portalLocations);
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 40;
            areaBuilder.EndZone = 42;
            areaBuilder.Phase = 2;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            AddPortal(island, usedSquares, placementRandom, portalLocations);
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 43;
            areaBuilder.EndZone = 45;
            areaBuilder.Phase = 2;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            AddPortal(island, usedSquares, placementRandom, portalLocations);
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 46;
            areaBuilder.EndZone = 48;
            areaBuilder.Phase = 2;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom, island.Southmost);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            //Bonus 3
            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 49;
            areaBuilder.EndZone = 51;
            areaBuilder.Phase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 52;
            areaBuilder.EndZone = 54;
            areaBuilder.Phase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 55;
            areaBuilder.EndZone = 57;
            areaBuilder.Phase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 58;
            areaBuilder.EndZone = 60;
            areaBuilder.Phase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 61;
            areaBuilder.EndZone = 63;
            areaBuilder.Phase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;

            island = map.IslandInfo[GetUnusedIsland(usedIslands, placementRandom)];
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 64;
            areaBuilder.EndZone = 66;
            areaBuilder.Phase = 3;
            areaBuilder.Biome = (BiomeType)biomeRandom.Next(0, biomeMax);
            areaBuilder.Location = GetUnusedSquare(usedSquares, island, placementRandom);
            SetIslandBiome(island, map, areaBuilder.Biome);
            yield return areaBuilder;
        }

        private static void AddPortal(IslandInfo island, bool[,] usedSquares, Random placementRandom, List<IntVector2> portalLocations)
        {
            var square = GetUnusedSquare(usedSquares, island, placementRandom, island.Northmost);
            portalLocations.Add(square);
        }

        private static IntVector2 GetUnusedSquare(bool[,] usedSquares, IslandInfo island, Random placementRandom, IntVector2 desired)
        {
            if (!usedSquares[desired.x, desired.y])
            {
                usedSquares[desired.x, desired.y] = true;
                return desired;
            }

            return GetUnusedSquare(usedSquares, island, placementRandom);
        }

        private static IntVector2 GetUnusedSquare(bool[,] usedSquares, IslandInfo island, Random placementRandom)
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

        private static int GetUnusedIsland(bool[] usedIslands, Random placementRandom)
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
            foreach(var square in island.islandPoints)
            {
                map.TextureOffsets[square.x, square.y] = (int)biome;
            }
        }
    }
}
