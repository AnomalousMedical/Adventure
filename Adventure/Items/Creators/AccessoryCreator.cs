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
                InfoId = nameof(ItemText.CounterAttack),
                CounterPercent = 70L
            };

            return CreateInventoryItem(accessory, true);
        }

        public InventoryItem CreateTargetScope()
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.TargetScope),
                ShowEnemyInfo = true
            };

            return CreateInventoryItem(accessory, true);
        }

        public InventoryItem CreateHealing(float healingBonus, bool cureAll)
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.Healing),
                HealingBonus = healingBonus,
                CureAll = cureAll
            };

            return CreateInventoryItem(accessory, true);
        }

        public InventoryItem CreateDoublecast()
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.Doublecast),
                Doublecast = true,
            };

            return CreateInventoryItem(accessory, true);
        }

        public InventoryItem CreateItemUsage(float itemUsageBonus)
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.ItemUsage),
                ItemUsageBonus = itemUsageBonus
            };

            return CreateInventoryItem(accessory, true);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment, bool unique)
        {
            return new InventoryItem(equipment, nameof(EquipAccessory))
            {
                Unique = unique
            };
        }
    }
}
