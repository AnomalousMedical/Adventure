using Adventure.Battle;
using Engine;
using RpgMath;
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

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            if (charSheet != null)
            {
                charSheet.MainHand = item.Equipment;
            }
        }
    }

    class EquipOffHand : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            target.OffHand = item.Equipment;
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            if (charSheet != null)
            {
                charSheet.OffHand = item.Equipment;
            }
        }
    }

    class EquipAccessory : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            target.Accessory = item.Equipment;
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            if (charSheet != null)
            {
                charSheet.Accessory = item.Equipment;
            }
        }
    }

    class EquipBody : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            target.Body = item.Equipment;
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            if (charSheet != null)
            {
                charSheet.Body = item.Equipment;
            }
        }
    }
}
