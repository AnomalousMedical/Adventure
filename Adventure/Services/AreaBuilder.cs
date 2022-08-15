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
        void SetupZone(int zoneIndex, Zone.Description o, Random initRandom);

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

        public bool PlaceTreasure { get; set; } = true;

        public int TreasureLevel { get; set; } = 3;

        public Element EnemyWeakElement { get; set; } = Element.None;

        public Element EnemyStrongElement { get; set; } = Element.None;

        public IEnumerable<ITreasure> Treasure { get; set; }

        public IEnumerable<ITreasure> StealTreasure { get; set; }

        public IEnumerable<ITreasure> BossStealTreasure { get; set; }

        public IEnumerable<ITreasure> UniqueStealTreasure { get; set; }

        public IEnumerable<ITreasure> BossUniqueStealTreasure { get; set; }

        public virtual void SetupZone(int zoneIndex, Zone.Description o, Random initRandom)
        {
            //TODO: change treasure to be based off an item level, not the current enemy level
            //TODO: Set the biomes
            //TODO: setup gates

            //It is important to keep the random order here, or everything changes
            o.LevelSeed = initRandom.Next(int.MinValue, int.MaxValue);
            o.EnemySeed = initRandom.Next(int.MinValue, int.MaxValue);
            var monsterRandom = new Random(initRandom.Next(int.MinValue, int.MaxValue));

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
            MonsterInfo bossMonster;
            IEnumerable<MonsterInfo> regularMonsters;

            o.MakePhilip = false;
            o.MakeRest = false;
            o.MakeBoss = zoneIndex == EndZone;
            o.MakeGate = GateZones?.Contains(zoneIndex) == true;

            o.Biome = worldDatabase.BiomeManager.GetBiome(Biome);
            var biomeMonsters = monsterInfo.Where(i => i.NativeBiome == Biome).ToList();
            regularMonsters = biomeMonsters;
            bossMonster = biomeMonsters[monsterRandom.Next(biomeMonsters.Count)];

            if (PlaceTreasure) 
            { 
                var treasures = new List<ITreasure>(Treasure ?? Enumerable.Empty<Treasure>());
                o.Treasure = treasures;

                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateHealthPotion(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateFerrymansBribe()));

                o.StealTreasure = new List<ITreasure>(StealTreasure ?? Enumerable.Empty<Treasure>())
                {
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel)),
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel)),
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel))
                };

                o.UniqueStealTreasure = UniqueStealTreasure;
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

    class Area0Builder : AreaBuilder
    {
        public Element StartingElementStaff { get; set; } = Element.Fire;

        public Area0Builder(IWorldDatabase worldDatabase, IList<MonsterInfo> monsterInfo, int index) : base(worldDatabase, monsterInfo, index)
        {
            PlaceTreasure = false;
        }

        public override void SetupZone(int zoneIndex, Zone.Description o, Random initRandom)
        {
            base.SetupZone(zoneIndex, o, initRandom);

            if (zoneIndex == 0)
            {
                o.EnemyLevel = 1;
                TreasureLevel = 3;
                o.MaxMainCorridorBattles = 1;
                o.MakePhilip = false;
                o.MakeRest = false;
                o.MakeBoss = false;
                o.MakeGate = false;

                //Give out starting weapons
                var treasures = new List<ITreasure>(Treasure ?? Enumerable.Empty<Treasure>());
                o.Treasure = treasures;

                treasures.Add(new Treasure(worldDatabase.SwordCreator.CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.ElementalStaffCreator.GetStaffCreator(StartingElementStaff).CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.AxeCreator.CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.SpearCreator.CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.MaceCreator.CreateNormal(TreasureLevel)));
                                           
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateHealthPotion(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateFerrymansBribe()));
            }
            else if (zoneIndex == 1)
            {
                o.EnemyLevel = 3;
                TreasureLevel = 3;
                o.MakePhilip = false;
                o.MakeRest = true;
                o.MakeBoss = true;
                o.MakeGate = false;
                o.StartEnd = true;

                //Give out starting armor
                var treasures = new List<ITreasure>(Treasure ?? Enumerable.Empty<Treasure>());
                o.Treasure = treasures;

                treasures.Add(new Treasure(worldDatabase.ShieldCreator.CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.DaggerCreator.CreateNormal(TreasureLevel)));

                //Change some of these to the other armor types
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(TreasureLevel)));

                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateHealthPotion(TreasureLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateFerrymansBribe()));

                o.BossUniqueStealTreasure = new List<ITreasure>(BossUniqueStealTreasure ?? Enumerable.Empty<Treasure>())
                {
                    //This should be element based, so give out what is good in the next area
                    new Treasure(worldDatabase.SwordCreator.CreateEpic(TreasureLevel))
                };

                //You get the dagger in this zone, so some of this is missable
                o.StealTreasure = new List<ITreasure>(StealTreasure ?? Enumerable.Empty<Treasure>())
                {
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel)),
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(TreasureLevel))
                };
            }
        }
    }
}
