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
        }

        public void SetupZone(int zoneIndex, Zone.Description o)
        {
            //It is important to keep the random order here, or everything changes
            var initRandom = new Random(GetZoneSeed(zoneIndex));
            o.LevelSeed = initRandom.Next(int.MinValue, int.MaxValue);
            o.EnemySeed = initRandom.Next(int.MinValue, int.MaxValue);
            var elementalRandom = new Random(initRandom.Next(int.MinValue, int.MaxValue));
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
            int biomeSelectorIndex;
            if (zoneIndex == 0)
            {
                o.EnemyLevel = 1;
                o.MaxMainCorridorBattles = 1;
                o.MakeBoss = true;
                biomeSelectorIndex = o.LevelSeed % biomeManager.Count;

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
                const int zoneLevelScaler = 2;
                const int levelScale = 3;
                o.EnemyLevel = zoneBasis / zoneLevelScaler * levelScale + levelScale;
                o.MakeAsimov = zoneBasis % zoneLevelScaler == 0;
                o.MakeRest = zoneBasis % zoneLevelScaler == 1;
                o.MakeBoss = zoneBasis % zoneLevelScaler == 1;
                o.MakeGate = zoneBasis % 4 == 3;
                biomeSelectorIndex = GetZoneSeed(zoneBasis / zoneLevelScaler); //Division keeps us pinned on the same type of zone for that many zones

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
            o.Biome = biomeManager.GetBiome(Math.Abs(biomeSelectorIndex) % biomeManager.Count);

            var weakness = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
            var strength = (Element)elementalRandom.Next((int)Element.MagicStart, (int)Element.MagicEnd);
            if(weakness == strength)
            {
                strength += 1;
                if(strength >= Element.MagicEnd)
                {
                    strength = Element.MagicStart;
                }
            }
            monsterMaker.PopulateBiome(o.Biome, monsterInfo, weakness, strength, elementalRandom);
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
