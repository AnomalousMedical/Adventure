using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Actions
{
    class RestoreHp : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0) { return; }

            target.CurrentHp += item.Number.Value;
            if(target.CurrentHp > target.Hp)
            {
                target.CurrentHp = target.Hp;
            }
        }
    }

    class RestoreMp : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0) { return; }

            target.CurrentMp += item.Number.Value;
            if (target.CurrentMp > target.Mp)
            {
                target.CurrentMp = target.Mp;
            }
        }
    }

    class Resurrect : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp != 0) { return; }

            target.CurrentHp += (long)(target.Hp * item.Number.Value * 0.01f);
        }
    }
}
