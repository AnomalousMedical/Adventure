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
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            Equip(item, target);
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            Equip(item, charSheet);
        }

        private static void Equip(InventoryItem item, CharacterSheet charSheet)
        {
            if (charSheet != null)
            {
                if (charSheet.MainHand?.Id == item.Equipment.Id)
                {
                    charSheet.RemoveEquipment(charSheet.MainHand.Id.Value);
                }
                else
                {
                    charSheet.MainHand = item.Equipment;
                }
            }
        }
    }

    class EquipOffHand : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            Equip(item, target);
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            Equip(item, charSheet);
        }

        private static void Equip(InventoryItem item, CharacterSheet charSheet)
        {
            if (charSheet != null)
            {
                if (charSheet.OffHand?.Id == item.Equipment.Id)
                {
                    charSheet.RemoveEquipment(charSheet.OffHand.Id.Value);
                }
                else
                {
                    charSheet.OffHand = item.Equipment;
                }
            }
        }
    }

    class EquipAccessory : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            Equip(item, target);
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            Equip(item, charSheet);
        }

        private static void Equip(InventoryItem item, CharacterSheet charSheet)
        {
            if (charSheet != null)
            {
                if (charSheet.Accessory?.Id == item.Equipment.Id)
                {
                    charSheet.RemoveEquipment(charSheet.Accessory.Id.Value);
                }
                else
                {
                    charSheet.Accessory = item.Equipment;
                }
            }
        }
    }

    class EquipBody : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            item.Equipment.EnsureEquipmentId();
            Equip(item, target);
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            item.Equipment.EnsureEquipmentId();
            var charSheet = target.Stats as CharacterSheet;
            Equip(item, charSheet);
        }

        private static void Equip(InventoryItem item, CharacterSheet charSheet)
        {
            if (charSheet != null)
            {
                if (charSheet.Body?.Id == item.Equipment.Id)
                {
                    charSheet.RemoveEquipment(charSheet.Body.Id.Value);
                }
                else
                {
                    charSheet.Body = item.Equipment;
                }
            }
        }
    }
}
