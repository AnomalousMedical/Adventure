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

        public InventoryItem CreateHealing(String adjective, float healingBonus)
        {
            var accessory = new Equipment
            {
                Name = $"{adjective} Bangle of Restoration",
                HealingBonus = healingBonus
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateItemUsage(String adjective, float itemUsageBonus)
        {
            var accessory = new Equipment
            {
                Name = $"{adjective} Gloves of Handling",
                ItemUsageBonus = itemUsageBonus
            };

            return CreateInventoryItem(accessory);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipAccessory));
        }
    }
}
