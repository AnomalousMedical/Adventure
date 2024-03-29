﻿using Adventure.Items.Actions;
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
                Name = $"Counter attack accessory",
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

        public InventoryItem CreateHealing(String adjective, float healingBonus, bool cureAll)
        {
            var accessory = new Equipment
            {
                Name = $"{adjective} Bangle of Restoration",
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
