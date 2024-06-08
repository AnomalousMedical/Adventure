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

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateTargetScope()
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.TargetScope),
                ShowEnemyInfo = true
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateHealing(float healingBonus, bool cureAll)
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.Healing),
                HealingBonus = healingBonus,
                CureAll = cureAll
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateDoublecast()
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.Doublecast),
                Doublecast = true,
            };

            return CreateInventoryItem(accessory);
        }

        public InventoryItem CreateItemUsage(float itemUsageBonus)
        {
            var accessory = new Equipment
            {
                InfoId = nameof(ItemText.ItemUsage),
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
