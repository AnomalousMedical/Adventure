using Adventure.Items;
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
    interface IAreaBuilder
    {
        void SetupZone(int zoneIndex, Zone.Description o, FIRandom initRandom);

        int StartZone { get; }

        int EndZone { get; }

        int Phase { get; }

        BiomeType Biome { get; }

        int Index { get; }

        int IndexInPhase { get; }

        IntVector2 Location { get; }
    }

    class AreaBuilder : IAreaBuilder
    {
        protected readonly IWorldDatabase worldDatabase;
        protected readonly IList<MonsterInfo> monsterInfo;

        public AreaBuilder
        (
            IWorldDatabase worldDatabase,
            IList<MonsterInfo> monsterInfo,
            int index
        )
        {
            this.worldDatabase = worldDatabase;
            this.monsterInfo = monsterInfo;
            this.Index = index;
        }

        public int StartZone { get; set; }

        public int EndZone { get; set; }

        public int Phase { get; set; }

        public int Index { get; private set; }

        public int IndexInPhase { get; set; } = int.MaxValue;

        public IEnumerable<int> GateZones { get; set; }

        public BiomeType Biome { get; set; }

        public IntVector2 Location { get; set; }

        public PlotItems? PlotItem { get; set; }

        public Element EnemyWeakElement { get; set; } = Element.None;

        public Element EnemyStrongElement { get; set; } = Element.None;

        public IEnumerable<ITreasure> Treasure { get; set; }

        public IEnumerable<ITreasure> StealTreasure { get; set; }

        public IEnumerable<ITreasure> BossStealTreasure { get; set; }

        public IEnumerable<ITreasure> UniqueStealTreasure { get; set; }

        public IEnumerable<ITreasure> BossUniqueStealTreasure { get; set; }

        /// <summary>
        /// This is mostly a special case for the 1st zone. It makes you start at the end instead
        /// of the start, since the game starts in the first zone.
        /// </summary>
        public bool StartEnd { get; set; }

        public int MaxMainCorridorBattles { get; set; } = int.MaxValue;

        public virtual void SetupZone(int zoneIndex, Zone.Description o, FIRandom initRandom)
        {
            //It is important to keep the random order here, or everything changes
            o.LevelSeed = initRandom.Next(int.MinValue, int.MaxValue);
            o.EnemySeed = initRandom.Next(int.MinValue, int.MaxValue);
            var monsterRandom = new FIRandom(initRandom.Next(int.MinValue, int.MaxValue));
            var areaTreasureRandom = new FIRandom(worldDatabase.CurrentSeed + Index); //This determines if this zone gets part of an area's treasure by rolling its index, it must be the same per area
            var treasureZoneStart = StartZone;
            var treasureZoneEnd = EndZone + 1;

            o.Index = zoneIndex;
            o.Width = 50;
            o.Height = 50;
            o.CorridorSpace = 10;
            o.RoomDistance = 3;
            o.RoomMin = new IntSize2(2, 2);
            o.RoomMax = new IntSize2(6, 6); //Between 3-6 is good here, 3 for more cityish with small rooms, 6 for more open with more big rooms, sometimes connected
            o.CorridorMaxLength = 4;
            o.GoPrevious = zoneIndex != 0;
            o.ConnectPreviousToWorld = zoneIndex == StartZone;
            o.ConnectNextToWorld = zoneIndex == EndZone;
            o.MaxMainCorridorBattles = MaxMainCorridorBattles;
            MonsterInfo bossMonster;
            IEnumerable<MonsterInfo> regularMonsters;

            o.MakePhilip = false;
            o.MakeRest = false;
            o.MakeBoss = zoneIndex == EndZone;
            o.MakeGate = GateZones?.Contains(zoneIndex) == true;
            o.StartEnd = StartEnd;

            if(zoneIndex == EndZone && PlotItem != null)
            {
                o.PlotItem = PlotItem;
            }

            o.Biome = worldDatabase.BiomeManager.GetBiome(Biome);
            var biomeMonsters = monsterInfo.Where(i => i.NativeBiome == Biome).ToList();
            regularMonsters = biomeMonsters;
            bossMonster = biomeMonsters[monsterRandom.Next(biomeMonsters.Count)];

            o.Treasure = Treasure?.Where(i => areaTreasureRandom.Next(treasureZoneStart, treasureZoneEnd) == zoneIndex);
            o.StealTreasure = StealTreasure?.Where(i => areaTreasureRandom.Next(treasureZoneStart, treasureZoneEnd) == zoneIndex);
            o.UniqueStealTreasure = UniqueStealTreasure?.Where(i => areaTreasureRandom.Next(treasureZoneStart, treasureZoneEnd) == zoneIndex);

            if (o.MakeBoss) //This assumes 1 boss per area
            {
                o.BossUniqueStealTreasure = BossUniqueStealTreasure;
            }

            if (EnemyWeakElement != Element.None && EnemyStrongElement != Element.None && EnemyWeakElement == EnemyStrongElement)
            {
                EnemyStrongElement += 1;
                if (EnemyStrongElement >= Element.MagicEnd)
                {
                    EnemyStrongElement = Element.MagicStart;
                }
            }

            worldDatabase.MonsterMaker.PopulateBiome(o.Biome, regularMonsters, bossMonster, EnemyWeakElement, EnemyStrongElement);
        }
    }
}
