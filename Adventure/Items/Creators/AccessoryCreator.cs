using Adventure.Items.Actions;
using RpgMath;
using System;

namespace Adventure.Items.Creators
{
    class AccessoryCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public AccessoryCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateCounterAttack()
        {
            var accessory = new Equipment
            {
                Name = $"Gauntlets of Revenge",
                CounterPercent = 40L
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateTargetScope()
        {
            var accessory = new Equipment
            {
                Name = $"Target Scope",
                ShowEnemyInfo = true
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateHealing(float healingBonus, bool cureAll)
        {
            var accessory = new Equipment
            {
                Name = "Ring of Healing",
                HealingBonus = healingBonus,
                CureAll = cureAll
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateDoublecast()
        {
            var accessory = new Equipment
            {
                Name = "Elemental Amplifier",
                Doublecast = true,
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateItemUsage(float itemUsageBonus)
        {
            var accessory = new Equipment
            {
                Name = "Gloves of Handling",
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
