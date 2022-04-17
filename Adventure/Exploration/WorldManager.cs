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
        private readonly IBiomeManager biomeManager;
        private readonly IMonsterMaker monsterMaker;
        private List<int> createdZoneSeeds = new List<int>();
        private Random zoneRandom;
        private readonly SwordCreator swordCreator;
        private readonly ShieldCreator shieldCreator;
        private readonly FireStaffCreator fireStaffCreator;
        private readonly AccessoryCreator accessoryCreator;
        private readonly ArmorCreator armorCreator;
        private readonly PotionCreator potionCreator;
        private readonly AxeCreator axeCreator;
        private readonly DaggerCreator daggerCreator;
        private List<MonsterInfo> monsterInfo;
        private List<int> chipZones = new List<int>();

        const int zoneLevelScaler = 2;
        const int levelScale = 3;

        public WorldManager
        (
            Persistence persistence,
            IBiomeManager biomeManager,
            IMonsterMaker monsterMaker,
            SwordCreator swordCreator,
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
            this.biomeManager = biomeManager;
            this.monsterMaker = monsterMaker;
            this.swordCreator = swordCreator;
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
            chipZones.Add(chipMax); //The last couple of zones are chip zones, then randomly before that
            chipZones.Add(chipMax - 1);
            chipMax -= 2;
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax)); //This could repeat, but that is ok its just appearance
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax));
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax));
            chipZones.Add(chipZoneRandom.Next(chipMin, chipMax));
        }

        public void SetupZone(int zoneIndex, Zone.Description o)
        {
            //It is important to keep the random order here, or everything changes
            var initRandom = new Random(GetZoneSeed(zoneIndex));
            o.LevelSeed = initRandom.Next(int.MinValue, int.MaxValue);
            o.EnemySeed = initRandom.Next(int.MinValue, int.MaxValue);
            //Add treasure random and stuff here too

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
            if (zoneIndex == 0)
            {
                o.EnemyLevel = 1;
                o.MaxMainCorridorBattles = 1;
                o.MakeBoss = true;
                var zoneSeed = o.LevelSeed;
                var monsterRandom = new Random(zoneSeed);

                var biomeType = (BiomeType)(Math.Abs(zoneSeed) % (int)BiomeType.Max);
                o.Biome = biomeManager.GetBiome(biomeType);
                var biomeMonsters = monsterInfo.Where(i => i.NativeBiome == biomeType).ToList();
                regularMonsters = biomeMonsters;
                bossMonster = biomeMonsters[monsterRandom.Next(biomeMonsters.Count)];

                //Give out starting weapons
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                InventoryItem weapon = null;
                weapon = new InventoryItem(swordCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipMainHand));
                treasures.Add(new Treasure(weapon));
                weapon = new InventoryItem(fireStaffCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipMainHand));
                treasures.Add(new Treasure(weapon));
                weapon = new InventoryItem(axeCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipMainHand));
                treasures.Add(new Treasure(weapon));
                weapon = new InventoryItem(swordCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipMainHand));
                treasures.Add(new Treasure(weapon));

                var dagger = new InventoryItem(daggerCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipOffHand));
                treasures.Add(new Treasure(dagger));

                treasures.Add(new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateFerrymansBribe()));

                o.BossUniqueStealTreasure = new List<ITreasure>()
                {
                    new Treasure(new InventoryItem(swordCreator.CreateEpic(o.EnemyLevel), nameof(Items.Actions.EquipMainHand)))
                };

                o.StealTreasure = new List<ITreasure>()
                {
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel))
                };
            }
            else
            {
                var zoneBasis = zoneIndex - 1;
                o.EnemyLevel = zoneBasis / zoneLevelScaler * levelScale + levelScale;
                o.MakeAsimov = zoneBasis % zoneLevelScaler == 0;
                o.MakeRest = zoneBasis % zoneLevelScaler == 1;
                o.MakeBoss = zoneBasis % zoneLevelScaler == 1;
                o.MakeGate = zoneBasis % 4 == 3;
                var zoneSeedIndex = zoneBasis / zoneLevelScaler;
                var zoneSeed = GetZoneSeed(zoneSeedIndex); //Division keeps us pinned on the same type of zone for that many zones
                var monsterRandom = new Random(zoneSeed);

                if (chipZones.Contains(zoneSeedIndex) || o.EnemyLevel > 92)
                {
                    o.Biome = biomeManager.MakeChip();
                    regularMonsters = monsterInfo;
                    bossMonster = monsterInfo[monsterRandom.Next(monsterInfo.Count)];
                }
                else
                {
                    var biomeType = (BiomeType)(Math.Abs(zoneSeed) % (int)BiomeType.Max);
                    o.Biome = biomeManager.GetBiome(biomeType);
                    var biomeMonsters = monsterInfo.Where(i => i.NativeBiome == biomeType).ToList();
                    regularMonsters = biomeMonsters;
                    bossMonster = biomeMonsters[monsterRandom.Next(biomeMonsters.Count)];
                }

                var elementalRandom = new Random(zoneSeed);
                if (o.EnemyLevel > 14)
                {
                    attackElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
                }
                if (o.EnemyLevel > 20)
                {
                    defendElement = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
                }

                //Dumb test treasure
                var treasures = new List<ITreasure>();
                o.Treasure = treasures;

                InventoryItem weapon = null;
                switch (zoneIndex % 3)
                {
                    case 0:
                        weapon = new InventoryItem(swordCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipMainHand));
                        break;
                    case 1:
                        weapon = new InventoryItem(fireStaffCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipMainHand));
                        break;
                    case 2:
                        weapon = new InventoryItem(axeCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipMainHand));
                        break;
                }

                treasures.Add(new Treasure(weapon));

                var shield = new InventoryItem(shieldCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipOffHand));
                treasures.Add(new Treasure(shield));

                //These don't really do anything right now
                //var acc = new InventoryItem(accessoryCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipAccessory));
                //treasures.Add(new Treasure(acc));

                var armor = new InventoryItem(armorCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipBody));
                treasures.Add(new Treasure(armor));

                treasures.Add(new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateHealthPotion(o.EnemyLevel)));
                treasures.Add(new Treasure(potionCreator.CreateFerrymansBribe()));

                o.StealTreasure = new List<ITreasure>()
                {
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel)),
                    new Treasure(potionCreator.CreateManaPotion(o.EnemyLevel))
                };

                if(zoneBasis % 3 == 0)
                {
                    var dagger = new InventoryItem(daggerCreator.CreateNormal(o.EnemyLevel), nameof(Items.Actions.EquipOffHand));

                    o.UniqueStealTreasure = new List<ITreasure>()
                    {
                        new Treasure(dagger)
                    };
                }
            }

            if(attackElement != Element.None && defendElement != Element.None && attackElement == defendElement)
            {
                defendElement += 1;
                if(defendElement >= Element.MagicEnd)
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
    }
}
