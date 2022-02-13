using Adventure.Items;
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
        private List<int> createdZoneSeeds = new List<int>();
        private Random zoneRandom;
        private IEquipmentCurve equipmentCurve;

        public WorldManager
        (
            Persistence persistence,
            IBiomeManager biomeManager,
            IEquipmentCurve equipmentCurve
        )
        {
            this.zoneRandom = new Random(persistence.World.Seed);
            this.biomeManager = biomeManager;
            this.equipmentCurve = equipmentCurve;
        }

        public void SetupZone(int zoneIndex, Zone.Description o)
        {
            var random = new Random(GetZoneSeed(zoneIndex));

            o.Index = zoneIndex;
            o.LevelSeed = random.Next(int.MinValue, int.MaxValue);
            o.EnemySeed = random.Next(int.MinValue, int.MaxValue);
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
                //First zone is a special case, both rest and asimov and level 1 enemies
                o.EnemyLevel = 1;
                o.MakeAsimov = true;
                o.MakeRest = true;
                biomeSelectorIndex = o.LevelSeed % biomeManager.Count;
            }
            else
            {
                var zoneBasis = zoneIndex - 1;
                const int zoneLevelScaler = 2;
                const int levelScale = 3;
                o.EnemyLevel = zoneBasis / zoneLevelScaler * levelScale + levelScale;
                o.MakeAsimov = zoneBasis % zoneLevelScaler == 0;
                o.MakeRest = zoneBasis % zoneLevelScaler == 1;
                biomeSelectorIndex = GetZoneSeed(zoneBasis / zoneLevelScaler); //Division keeps us pinned on the same type of zone for that many zones
            }
            o.Biome = biomeManager.GetBiome(Math.Abs(biomeSelectorIndex) % biomeManager.Count);

            //Dumb test treasure
            var treasures = new List<ITreasure>();
            o.Treasure = treasures;

            if(zoneIndex % 2 == 0)
            {
                var weapon = new InventoryItem
                {
                    Action = nameof(Items.Actions.EquipMainHand),
                    Name = $"Test Sword {o.EnemyLevel}",
                    Equipment = new Equipment
                    {
                        Attack = equipmentCurve.GetAttack(o.EnemyLevel),
                        AttackPercent = 100,
                    }
                };
                treasures.Add(new Treasure(weapon));
            }
            else
            {
                var weapon = new InventoryItem
                {
                    Action = nameof(Items.Actions.EquipMainHand),
                    Name = $"Test Staff {o.EnemyLevel}",
                    Equipment = new Equipment
                    {
                        MagicAttack = equipmentCurve.GetAttack(o.EnemyLevel),
                        MagicAttackPercent = 100,
                        Attack = equipmentCurve.GetAttack(o.EnemyLevel) / 3,
                        AttackPercent = 35
                    }
                };
                treasures.Add(new Treasure(weapon));
            }

            var shield = new InventoryItem
            {
                Action = nameof(Items.Actions.EquipOffHand),
                Name = $"Test Shield {o.EnemyLevel}",
                Equipment = new Equipment
                {
                    Defense = equipmentCurve.GetDefense(o.EnemyLevel),
                    MagicDefense = equipmentCurve.GetMDefense(o.EnemyLevel)
                }
            };
            treasures.Add(new Treasure(shield));

            var acc = new InventoryItem
            {
                Action = nameof(Items.Actions.EquipOffHand),
                Name = $"Test Accessory {o.EnemyLevel}",
                Equipment = new Equipment
                {
                    Defense = equipmentCurve.GetDefense(o.EnemyLevel),
                    MagicDefense = equipmentCurve.GetMDefense(o.EnemyLevel)
                }
            };
            treasures.Add(new Treasure(acc));

            var armor = new InventoryItem
            {
                Action = nameof(Items.Actions.EquipOffHand),
                Name = $"Test Armor {o.EnemyLevel}",
                Equipment = new Equipment
                {
                    Defense = equipmentCurve.GetDefense(o.EnemyLevel),
                    MagicDefense = equipmentCurve.GetMDefense(o.EnemyLevel)
                }
            };
            treasures.Add(new Treasure(armor));
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
