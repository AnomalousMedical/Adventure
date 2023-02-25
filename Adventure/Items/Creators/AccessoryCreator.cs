using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Creators
{
    class AccessoryCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public AccessoryCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public InventoryItem CreateCounterAttack()
        {
            var accessory = new Equipment
            {
                Name = $"Counter attack accessory",
                SpecialEffects = new[] { BattleSpecialEffects.Counterattack }
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateHealing(int level)
        {
            var accessory = new Equipment
            {
                Name = $"{nameGenerator.GetLevelName(level).Adjective} Healing Accessory",
            };

            if(level < SpellLevels.Common)
            {
                accessory.HealingBonus = 0.1f;
            }
            else if (level < SpellLevels.Superior)
            {
                accessory.HealingBonus = 0.2f;
            }
            else
            {
                accessory.HealingBonus = 0.3f;
            }

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateItemUsage(int level)
        {
            var accessory = new Equipment
            {
                Name = $"{nameGenerator.GetLevelName(level).Adjective} Item Use Accessory",
            };

            if (level < SpellLevels.Common)
            {
                accessory.ItemUsageBonus = 0.1f;
            }
            else if (level < SpellLevels.Superior)
            {
                accessory.ItemUsageBonus = 0.25f;
            }
            else
            {
                accessory.ItemUsageBonus = 0.5f;
            }

            return CreateInventoryItem(accessory);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipAccessory));
        }
    }
}
