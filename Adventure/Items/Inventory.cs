using Adventure.Battle;
using Engine;
using Newtonsoft.Json.Linq;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    class InventoryItem
    {
        public InventoryItem()
        {

        }

        public InventoryItem(Equipment equipment, String action)
        {
            this.Equipment = equipment;
            this.Name = equipment.Name;
            this.Action = action;
        }

        public String Name { get; set; }

        public String Action { get; set; }

        public Equipment Equipment { get; set; }

        public long? Number { get; set; }
    }

    interface IInventoryAction
    {
        bool AllowTargetChange => true;

        void Use(InventoryItem item, Inventory inventory, CharacterSheet target);
        void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target);
    }

    class Inventory
    {
        public List<InventoryItem> Items { get; } = new List<InventoryItem>();

        public int Size { get; set; } = 10;

        public bool HasRoom() => Items.Count < Size;

        public void Use(InventoryItem item, CharacterSheet target)
        {
            if(item.Action == null)
            {
                Items.Remove(item);
                return;
            }

            var action = CreateInstance<IInventoryAction>($"Adventure.Items.Actions.{item.Action}");
            action.Use(item, this, target);
        }
        
        public IInventoryAction CreateAction(InventoryItem item)
        {
            if (item.Action == null)
            {
                Items.Remove(item);
                return null;
            }

            var action = CreateInstance<IInventoryAction>($"Adventure.Items.Actions.{item.Action}");
            return action;
        }

        private T CreateInstance<T>(String name)
        {
            var type = Type.GetType(name);
            var instance = (T)Activator.CreateInstance(type);
            return instance;
        }
    }
}
