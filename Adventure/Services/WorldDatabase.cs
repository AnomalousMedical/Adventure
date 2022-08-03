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
        public WorldMapData WorldMap { get; private set; }

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

            foreach(var area in areaBuilders)
            {
                if(zoneIndex >= area.StartZone && zoneIndex <= area.EndZone)
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
            WorldMap = new WorldMapData(newSeed);
            areaBuilders = new List<IAreaBuilder>();
            for(var i = 0; i < 27; ++i)
            {
                var areaBuilder = SetupAreaBuilder(i, monsterInfo);
                areaBuilders.Add(areaBuilder);
            }
        }

        private IAreaBuilder SetupAreaBuilder(int area, IList<MonsterInfo> monsterInfo)
        {
            //TODO: Get rid of this function and loop and just write this out

            AreaBuilder areaBuilder = null;
            switch (area)
            {
                //Phase 0
                case 0:
                    areaBuilder = new Area0Builder(this, monsterInfo);
                    areaBuilder.StartZone = 0;
                    areaBuilder.EndZone = 1;
                    areaBuilder.IncludeStrongElement = false;
                    areaBuilder.IncludeWeakElement = false;
                    break;

                //Phase 1
                case 1:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 2;
                    areaBuilder.EndZone = 3;
                    areaBuilder.IncludeStrongElement = false;
                    areaBuilder.IncludeWeakElement = false;
                    break;

                case 2:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 4;
                    areaBuilder.EndZone = 5;
                    areaBuilder.IncludeStrongElement = false;
                    areaBuilder.IncludeWeakElement = false;
                    break;

                case 3:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 6;
                    areaBuilder.EndZone = 7;
                    areaBuilder.IncludeStrongElement = false;
                    areaBuilder.IncludeWeakElement = false;
                    break;

                case 4:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 8;
                    areaBuilder.EndZone = 9;
                    areaBuilder.IncludeStrongElement = false;
                    break;

                //Phase 2
                case 5:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 10;
                    areaBuilder.EndZone = 11;
                    break;

                case 6:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 12;
                    areaBuilder.EndZone = 13;
                    break;

                case 7:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 14;
                    areaBuilder.EndZone = 15;
                    break;

                case 8:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 16;
                    areaBuilder.EndZone = 17;
                    break;

                case 9:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 18;
                    areaBuilder.EndZone = 19;
                    break;

                case 10:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 20;
                    areaBuilder.EndZone = 21;
                    break;

                //Phase 3
                case 11:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 22;
                    areaBuilder.EndZone = 23;
                    break;

                case 12:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 24;
                    areaBuilder.EndZone = 25;
                    break;

                case 13:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 26;
                    areaBuilder.EndZone = 27;
                    break;

                case 14:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 28;
                    areaBuilder.EndZone = 29;
                    break;

                case 15:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 30;
                    areaBuilder.EndZone = 31;
                    break;

                case 16:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 32;
                    areaBuilder.EndZone = 33;
                    break;

                //Phase 4
                case 17:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 34;
                    areaBuilder.EndZone = 36;
                    break;

                //Bonus 1
                case 18:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 37;
                    areaBuilder.EndZone = 39;
                    break;

                //Bonus 2
                case 19:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 40;
                    areaBuilder.EndZone = 42;
                    break;

                case 20:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 43;
                    areaBuilder.EndZone = 45;
                    break;

                case 21:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 46;
                    areaBuilder.EndZone = 48;
                    break;

                //Bonus 3
                case 22:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 49;
                    areaBuilder.EndZone = 51;
                    break;

                case 23:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 52;
                    areaBuilder.EndZone = 54;
                    break;

                case 24:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 55;
                    areaBuilder.EndZone = 57;
                    break;

                case 25:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 58;
                    areaBuilder.EndZone = 60;
                    break;

                case 26:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 61;
                    areaBuilder.EndZone = 63;
                    break;

                case 27:
                    areaBuilder = new AreaBuilder(this, monsterInfo);
                    areaBuilder.StartZone = 64;
                    areaBuilder.EndZone = 66;
                    break;
            }

            return areaBuilder;
        }
    }
}
