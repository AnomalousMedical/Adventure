using Adventure.Items.Creators;
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
        int CurrentSeed { get; }
    }

    class WorldDatabase : IWorldDatabase
    {
        private List<IAreaBuilder> areaBuilders;
        private List<int> createdZoneSeeds;
        private int currentSeed;
        private Random zoneRandom;
        private readonly Persistence persistence;

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

        private WorldMapData _worldMap;
        public WorldMapData WorldMap
        {
            get
            {
                CheckSeed();

                return _worldMap;
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
            createdZoneSeeds = new List<int>();
            zoneRandom = new Random(newSeed);
            currentSeed = newSeed;
            areaBuilders = new List<IAreaBuilder>(27);
            var weaknessRandom = new Random(newSeed);
            var monsterInfo = MonsterMaker.CreateBaseMonsters(weaknessRandom);
            areaBuilders = SetupAreaBuilder(monsterInfo).ToList();
            _worldMap = new WorldMapData(newSeed);
            //Phase 0, 1 is 1 island, 2 is 2 islands + 3 more for the phase 2 bonus areas, everything above phase 2 has its own, except phase 4, which is not in the world
            var numIslands = areaBuilders.Where(i => i.Phase > 2).Count() + 1 + 2 + 3 - 1;
            _worldMap.Map.RemoveExtraIslands(numIslands);
            //TODO: need to check maps
            //3 largest islands need to have enough spaces for each phase
            //World needs enough islands to cover all zones
            //remove extra islands
        }

        private IEnumerable<IAreaBuilder> SetupAreaBuilder(IList<MonsterInfo> monsterInfo)
        {
            //TODO: Get rid of this function and loop and just write this out

            int area = 0;
            AreaBuilder areaBuilder = null;

            //Phase 0
            areaBuilder = new Area0Builder(this, monsterInfo, area++);
            areaBuilder.StartZone = 0;
            areaBuilder.EndZone = 1;
            areaBuilder.Phase = 0;
            areaBuilder.IndexInPhase = 0;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            yield return areaBuilder;

            //Phase 1
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 2;
            areaBuilder.EndZone = 3;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 1;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 4;
            areaBuilder.EndZone = 5;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 2;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 6;
            areaBuilder.EndZone = 7;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 3;
            areaBuilder.IncludeStrongElement = false;
            areaBuilder.IncludeWeakElement = false;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 8;
            areaBuilder.EndZone = 9;
            areaBuilder.Phase = 1;
            areaBuilder.IndexInPhase = 4;
            areaBuilder.IncludeStrongElement = false;
            yield return areaBuilder;

            //Phase 2
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 10;
            areaBuilder.EndZone = 11;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 0;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 12;
            areaBuilder.EndZone = 13;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 1;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 14;
            areaBuilder.EndZone = 15;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 2;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 16;
            areaBuilder.EndZone = 17;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 3;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 18;
            areaBuilder.EndZone = 19;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 4;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 20;
            areaBuilder.EndZone = 21;
            areaBuilder.Phase = 2;
            areaBuilder.IndexInPhase = 5;
            yield return areaBuilder;

            //Phase 3
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 22;
            areaBuilder.EndZone = 23;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 0;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 24;
            areaBuilder.EndZone = 25;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 1;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 26;
            areaBuilder.EndZone = 27;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 2;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 28;
            areaBuilder.EndZone = 29;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 3;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 30;
            areaBuilder.EndZone = 31;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 4;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 32;
            areaBuilder.EndZone = 33;
            areaBuilder.Phase = 3;
            areaBuilder.IndexInPhase = 5;
            yield return areaBuilder;

            //Phase 4
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 34;
            areaBuilder.EndZone = 36;
            areaBuilder.Phase = 4;
            areaBuilder.IndexInPhase = 0;
            yield return areaBuilder;

            //Bonus 1
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 37;
            areaBuilder.EndZone = 39;
            areaBuilder.Phase = 1;
            yield return areaBuilder;

            //Bonus 2
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 40;
            areaBuilder.EndZone = 42;
            areaBuilder.Phase = 2;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 43;
            areaBuilder.EndZone = 45;
            areaBuilder.Phase = 2;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 46;
            areaBuilder.EndZone = 48;
            areaBuilder.Phase = 2;
            yield return areaBuilder;

            //Bonus 3
            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 49;
            areaBuilder.EndZone = 51;
            areaBuilder.Phase = 3;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 52;
            areaBuilder.EndZone = 54;
            areaBuilder.Phase = 3;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 55;
            areaBuilder.EndZone = 57;
            areaBuilder.Phase = 3;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 58;
            areaBuilder.EndZone = 60;
            areaBuilder.Phase = 3;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 61;
            areaBuilder.EndZone = 63;
            areaBuilder.Phase = 3;
            yield return areaBuilder;

            areaBuilder = new AreaBuilder(this, monsterInfo, area++);
            areaBuilder.StartZone = 64;
            areaBuilder.EndZone = 66;
            areaBuilder.Phase = 3;
            yield return areaBuilder;
        }
    }
}
