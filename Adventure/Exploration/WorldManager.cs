using Adventure.Assets;
using Adventure.Items;
using Adventure.Items.Creators;
using Adventure.Services;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration
{
    interface IWorldManager
    {
        void SetupZone(int zoneIndex, Zone.Description o);
    }

    class WorldManager : IWorldManager
    {
        private readonly Persistence persistence;
        private readonly IBiomeManager biomeManager;
        private readonly IMonsterMaker monsterMaker;
        private List<int> createdZoneSeeds = new List<int>();
        private Random zoneRandom;
        private readonly SwordCreator swordCreator;
        private readonly SpearCreator spearCreator;
        private readonly MaceCreator maceCreator;
        private readonly ShieldCreator shieldCreator;
        private readonly FireStaffCreator fireStaffCreator;
        private readonly AccessoryCreator accessoryCreator;
        private readonly ArmorCreator armorCreator;
        private readonly PotionCreator potionCreator;
        private readonly AxeCreator axeCreator;
        private readonly DaggerCreator daggerCreator;
        private List<MonsterInfo> monsterInfo;
        private HashSet<int> chipZones = new HashSet<int>();
        private List<BiomeType> startingZones;
        private int numStartingZones = (int)BiomeType.Max;

        const int zoneLevelScaler = 2;
        const int levelScale = 3;

        public WorldManager
        (
            Persistence persistence,
            IBiomeManager biomeManager,
            IMonsterMaker monsterMaker,
            SwordCreator swordCreator,
            SpearCreator spearCreator,
            MaceCreator maceCreator,
            ShieldCreator shieldCreator,
            FireStaffCreator fireStaffCreator,
            AccessoryCreator accessoryCreator,
            ArmorCreator armorCreator,
            PotionCreator potionCreator,
            AxeCreator axeCreator,
            DaggerCreator daggerCreator
        )
        {
            this.zoneRandom = new Random(persistence.Current.World.Seed);
            this.persistence = persistence;
            this.biomeManager = biomeManager;
            this.monsterMaker = monsterMaker;
            this.swordCreator = swordCreator;
            this.spearCreator = spearCreator;
            this.maceCreator = maceCreator;
            this.shieldCreator = shieldCreator;
            this.fireStaffCreator = fireStaffCreator;
            this.accessoryCreator = accessoryCreator;
            this.armorCreator = armorCreator;
            this.potionCreator = potionCreator;
            this.axeCreator = axeCreator;
            this.daggerCreator = daggerCreator;

            var weaknessRandom = new Random(persistence.Current.World.Seed);
            this.monsterInfo = monsterMaker.CreateBaseMonsters(weaknessRandom);

            var chipZoneRandom = new Random(persistence.Current.World.Seed);
            var chipMax = 99 / levelScale;
            var chipMin = chipMax / 2;
            //Make the last couple zones chip zones
            chipZones.Add(chipMax);
            chipZones.Add(chipMax - 1);
            chipMax -= 2;
            //Randomly generate a few more zones
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax));
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax));
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax));
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax));
            //Ensure we have enough chip zones
            while (chipZones.Count < 7)
            {
                --chipMax;
                chipZones.Add(chipMax);
            }
        }

        public void SetupZone(int zoneIndex, Zone.Description o)
        {
            //It is important to keep the random order here, or everything changes
            var initRandom = new Random(GetZoneSeed(zoneIndex));
            o.LevelSeed = initRandom.Next(int.MinValue, int.MaxValue);
            o.EnemySeed = initRandom.Next(int.MinValue, int.MaxValue);

            o.Index = zoneIndex;
            o.Width = 50;
            o.Height = 50;
            o.CorridorSpace = 10;
            o.RoomDistance = 3;
            o.RoomMin = new IntSize2(2, 2);
            o.RoomMax = new IntSize2(6, 6); //Between 3-6 is good here, 3 for more cityish with small rooms, 6 for more open with more big rooms, sometimes connected
            o.CorridorMaxLength = 4;
            o.GoPrevious = zoneIndex != 0;
            var attackElement = Element.None;
            var defendElement = Element.None;
            MonsterInfo bossMonster;
            IEnumerable<MonsterInfo> regularMonsters;

            o.EnemyLevel = zoneIndex / zoneLevelScaler * levelScale;
            o.MakeAsimov = zoneIndex % zoneLevelScaler == 0;
            o.MakeRest = zoneIndex % zoneLevelScaler == 1;
            o.MakeBoss = zoneIndex % zoneLevelScaler == 1;
            o.MakeGate = zoneIndex % 4 == 3;
            var zoneSeedIndex = zoneIndex / zoneLevelScaler;
            var zoneSeed = GetZoneSeed(zoneSeedIndex); //Division keeps us pinned on the same type of zone for that many zones
            var monsterRandom = new Random(zoneSeed);
            var treasureRandom = new Random(zoneSeed);

            if (chipZones.Contains(zoneSeedIndex))
            {
                o.Biome = biomeManager.MakeChip();
                regularMonsters = monsterInfo;
                bossMonster = monsterInfo[monsterRandom.Next(monsterInfo.Count)];
            }
            else
            {
                BiomeType biomeType;
                if (zoneSeedIndex < numStartingZones)
                {
                    var startupZones = GetStartingZones();
                    biomeType = startingZones[zoneSeedIndex];
                }
                else
                {
                    biomeType = (BiomeType)(Math.Abs(zoneSeed) % (int)BiomeType.Max);
                }
                o.Biome = biomeManager.GetBiome(biomeType);
                var biomeMonsters = monsterInfo.Where(i => i.NativeBiome == biomeType).ToList();
                regularMonsters = biomeMonsters;
                bossMonster = biomeMonsters[monsterRandom.Next(biomeMonsters.Count)];
            }

            var elementalRandom = new Random(zoneSeed);
            if (zoneSeedIndex >= numStartingZones)
            {
                attackElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
            }
            if (zoneSeedIndex > numStartingZones + 3)
            {
                defendElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
            }

            if (zoneIndex == 0)
            {
                o.EnemyLevel = 1;
                o.MaxMainCorridorBattles = 1;
                o.MakeAsimov = false;
                o.MakeRest = false;
                o.MakeBoss = false;
                o.MakeGate = false;

                //Give out starting weapons
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                treasures.Add(new Treasure(swordCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(fireStaffCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(axeCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(spearCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(maceCreator.CreateNormal(o.EnemyLevel)));

                treasures.Add(new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateFerrymansBribe()));
            }
            else if (zoneIndex == 1)
            {
                o.EnemyLevel = 1;
                o.MakeAsimov = false;
                o.MakeRest = true;
                o.MakeBoss = true;
                o.MakeGate = false;

                //Give out starting armor
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                treasures.Add(new Treasure(shieldCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(daggerCreator.CreateNormal(o.EnemyLevel)));

                //Change some of these to the other armor types
                treasures.Add(new Treasure(armorCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(armorCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(armorCreator.CreateNormal(o.EnemyLevel)));
                treasures.Add(new Treasure(armorCreator.CreateNormal(o.EnemyLevel)));

                treasures.Add(new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateFerrymansBribe()));

                o.BossUniqueStealTreasure = new List<ITreasure>()
                {
                    //This should be element based, so give out what is good in the next area
                    new Treasure(swordCreator.CreateEpic(o.EnemyLevel))
                };

                o.StealTreasure = new List<ITreasure>()
                {
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel))
                };
            }
            else //All other zones
            {
                //Dumb test treasure
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                InventoryItem weapon = null;
                switch (zoneIndex % 3)
                {
                    case 0:
                        weapon = swordCreator.CreateNormal(o.EnemyLevel);
                        break;
                    case 1:
                        weapon = fireStaffCreator.CreateNormal(o.EnemyLevel);
                        break;
                    case 2:
                        weapon = axeCreator.CreateNormal(o.EnemyLevel);
                        break;
                }

                treasures.Add(new Treasure(weapon));

                treasures.Add(new Treasure(shieldCreator.CreateNormal(o.EnemyLevel)));

                //These don't really do anything right now
                //var acc = new InventoryItem(accessoryCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipAccessory));
                //treasures.Add(new Treasure(acc));

                treasures.Add(new Treasure(armorCreator.CreateNormal(o.EnemyLevel)));

                treasures.Add(new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateFerrymansBribe()));

                o.StealTreasure = new List<ITreasure>()
                {
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel))
                };

                var uniqueStealTreasure = new List<ITreasure>();
                o.UniqueStealTreasure = uniqueStealTreasure;

                if (o.MakeBoss)
                {
                    var element = o.Biome.BossEnemy.Resistances.Where(i =>
                       (i.Key == Element.Slashing
                     || i.Key == Element.Piercing
                     || i.Key == Element.Bludgeoning) && i.Value == Resistance.Weak)
                        .FirstOrDefault();

                    ITreasure bossWeaknessTreasure = null;

                    switch (element.Key)
                    {
                        case Element.Slashing:
                            bossWeaknessTreasure = new Treasure(swordCreator.CreateEpic(o.EnemyLevel));
                            break;
                        case Element.Piercing:
                            bossWeaknessTreasure = new Treasure(spearCreator.CreateEpic(o.EnemyLevel));
                            break;
                        case Element.Bludgeoning:
                            bossWeaknessTreasure = new Treasure(maceCreator.CreateEpic(o.EnemyLevel));
                            break;
                    }

                    if (bossWeaknessTreasure != null)
                    {
                        var storageLoc = treasureRandom.Next(2);
                        switch (storageLoc)
                        {
                            case 0:
                                uniqueStealTreasure.Add(bossWeaknessTreasure);
                                break;
                            case 1:
                                treasures.Add(bossWeaknessTreasure);
                                break;
                        }
                    }
                }

                if (zoneIndex % 3 == 0)
                {
                    uniqueStealTreasure.Add(new Treasure(daggerCreator.CreateNormal(o.EnemyLevel)));
                }
            }

            if (attackElement != Element.None && defendElement != Element.None && attackElement == defendElement)
            {
                defendElement += 1;
                if (defendElement >= Element.MagicEnd)
                {
                    defendElement = Element.MagicStart;
                }
            }

            monsterMaker.PopulateBiome(o.Biome, regularMonsters, bossMonster, attackElement, defendElement);
        }

        private int GetZoneSeed(int index)
        {
            var end = index + 1;
            for (var i = createdZoneSeeds.Count; i < end; ++i)
            {
                createdZoneSeeds.Add(zoneRandom.Next(int.MinValue, int.MaxValue));
            }
            return createdZoneSeeds[index];
        }

        private List<BiomeType> GetStartingZones()
        {
            var starupZoneRandom = new Random(persistence.Current.World.Seed);
            if (startingZones == null)
            {
                var zoneTypes = new List<BiomeType>();
                for (var i = 0; i < numStartingZones; ++i)
                {
                    zoneTypes.Add((BiomeType)i);
                }

                startingZones = new List<BiomeType>();
                while (zoneTypes.Count > 0)
                {
                    var index = starupZoneRandom.Next(zoneTypes.Count);
                    startingZones.Add(zoneTypes[index]);
                    zoneTypes.RemoveAt(index);
                }
            }
            return startingZones;
        }
    }
}
