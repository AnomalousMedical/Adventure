﻿using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Actions
{
    class EquipMainHand : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            target.MainHand = item.Equipment;
        }
    }

    class EquipOffHand : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            target.OffHand = item.Equipment;
        }
    }

    class EquipAccessory : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            target.Accessory = item.Equipment;
        }
    }

    class EquipBody : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            target.Body = item.Equipment;
        }
    }
}
