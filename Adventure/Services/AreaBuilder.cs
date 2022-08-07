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

        public bool PlaceTreasure { get; set; }

        public bool IncludeWeakElement { get; set; } = true;

        public bool IncludeStrongElement { get; set; } = true;

        public virtual void SetupZone(int zoneIndex, Zone.Description o, Random initRandom)
        {
            //TODO: change treasure to be based off an item level, not the current enemy level
            //TODO: Set the biomes
            //TODO: setup gates

            //It is important to keep the random order here, or everything changes
            o.LevelSeed = initRandom.Next(int.MinValue, int.MaxValue);
            o.EnemySeed = initRandom.Next(int.MinValue, int.MaxValue);
            var monsterRandom = new Random(initRandom.Next(int.MinValue, int.MaxValue));
            var treasureRandom = new Random(initRandom.Next(int.MinValue, int.MaxValue));
            var elementalRandom = new Random(initRandom.Next(int.MinValue, int.MaxValue));

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
            var weakElement = Element.None;
            var resistElement = Element.None;
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

            if (IncludeWeakElement)
            {
                weakElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
            }
            if (IncludeStrongElement)
            {
                resistElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
            }

            //Dumb test treasure
            if (PlaceTreasure) 
            { 
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                InventoryItem weapon = null;
                switch (zoneIndex % 3)
                {
                    case 0:
                        weapon = worldDatabase.SwordCreator.CreateNormal(o.EnemyLevel);
                        break;
                    case 1:
                        weapon = worldDatabase.FireStaffCreator.CreateNormal(o.EnemyLevel);
                        break;
                    case 2:
                        weapon = worldDatabase.AxeCreator.CreateNormal(o.EnemyLevel);
                        break;
                }

                treasures.Add(new Treasure(weapon));

                treasures.Add(new Treasure(worldDatabase.ShieldCreator.CreateNormal(o.EnemyLevel)));

                //These don't really do anything right now
                //var acc = new InventoryItem(accessoryCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipAccessory));
                //treasures.Add(new Treasure(acc));

                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(o.EnemyLevel)));

                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateFerrymansBribe()));

                o.StealTreasure = new List<ITreasure>()
                {
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel))
                };

                var uniqueStealTreasure = new List<ITreasure>();
                o.UniqueStealTreasure = uniqueStealTreasure;

                if (o.MakeBoss)
                {
                    var element = bossMonster.Resistances.Where(i =>
                       (i.Key == Element.Slashing
                     || i.Key == Element.Piercing
                     || i.Key == Element.Bludgeoning) && i.Value == Resistance.Weak)
                        .FirstOrDefault();

                    ITreasure bossWeaknessTreasure = null;

                    switch (element.Key)
                    {
                        case Element.Slashing:
                            bossWeaknessTreasure = new Treasure(worldDatabase.SwordCreator.CreateEpic(o.EnemyLevel));
                            break;
                        case Element.Piercing:
                            bossWeaknessTreasure = new Treasure(worldDatabase.SpearCreator.CreateEpic(o.EnemyLevel));
                            break;
                        case Element.Bludgeoning:
                            bossWeaknessTreasure = new Treasure(worldDatabase.MaceCreator.CreateEpic(o.EnemyLevel));
                            break;
                    }

                    if (bossWeaknessTreasure != null)
                    {
                        var storageLoc = treasureRandom.Next(2);
                        switch (storageLoc)
                        {
                            case 0:
                                treasures.Add(bossWeaknessTreasure);
                                break;
                            case 1:
                                uniqueStealTreasure.Add(bossWeaknessTreasure);
                                break;
                        }
                    }
                }

                //Elemental staff in first zone
                //Good one
                if (weakElement != Element.None && zoneIndex == StartZone)
                {
                    var staffCreator = worldDatabase.ElementalStaffCreator.GetStaffCreator(weakElement);

                    if (staffCreator != null)
                    {
                        var elementalStaff = new Treasure(staffCreator.CreateEpic(o.EnemyLevel));
                        var storageLoc = treasureRandom.Next(2);
                        switch (storageLoc)
                        {
                            case 0:
                                treasures.Add(elementalStaff);
                                break;
                            case 1:
                                uniqueStealTreasure.Add(elementalStaff);
                                break;
                        }
                    }
                }

                //Not good one, but it could end up good later
                if (resistElement != Element.None && zoneIndex == StartZone)
                {
                    var staffCreator = worldDatabase.ElementalStaffCreator.GetStaffCreator(resistElement);

                    if (staffCreator != null)
                    {
                        var elementalStaff = new Treasure(staffCreator.CreateEpic(o.EnemyLevel));
                        var storageLoc = treasureRandom.Next(2);
                        switch (storageLoc)
                        {
                            case 0:
                                treasures.Add(elementalStaff);
                                break;
                            case 1:
                                uniqueStealTreasure.Add(elementalStaff);
                                break;
                        }
                    }
                }

                if (zoneIndex % 3 == 0)
                {
                    uniqueStealTreasure.Add(new Treasure(worldDatabase.DaggerCreator.CreateNormal(o.EnemyLevel)));
                }
            }

            if (weakElement != Element.None && resistElement != Element.None && weakElement == resistElement)
            {
                resistElement += 1;
                if (resistElement >= Element.MagicEnd)
                {
                    resistElement = Element.MagicStart;
                }
            }

            worldDatabase.MonsterMaker.PopulateBiome(o.Biome, regularMonsters, bossMonster, weakElement, resistElement);
        }
    }

    class Area0Builder : AreaBuilder
    {
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
                o.MaxMainCorridorBattles = 1;
                o.MakePhilip = false;
                o.MakeRest = false;
                o.MakeBoss = false;
                o.MakeGate = false;

                //Give out starting weapons
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                treasures.Add(new Treasure(worldDatabase.SwordCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.FireStaffCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.AxeCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.SpearCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.MaceCreator.CreateNormal(o.EnemyLevel)));
                                           
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateFerrymansBribe()));
            }
            else if (zoneIndex == 1)
            {
                o.EnemyLevel = 3;
                o.MakePhilip = false;
                o.MakeRest = true;
                o.MakeBoss = true;
                o.MakeGate = false;
                o.StartEnd = true;

                //Give out starting armor
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                treasures.Add(new Treasure(worldDatabase.ShieldCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.DaggerCreator.CreateNormal(o.EnemyLevel)));

                //Change some of these to the other armor types
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.ArmorCreator.CreateNormal(o.EnemyLevel)));

                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(worldDatabase.PotionCreator.CreateFerrymansBribe()));

                o.BossUniqueStealTreasure = new List<ITreasure>()
                {
                    //This should be element based, so give out what is good in the next area
                    new Treasure(worldDatabase.SwordCreator.CreateEpic(o.EnemyLevel))
                };

                //You get the dagger in this zone, so some of this is missable
                o.StealTreasure = new List<ITreasure>()
                {
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(worldDatabase.PotionCreator.CreateManaPotion(o.EnemyLevel))
                };
            }
        }
    }
}
