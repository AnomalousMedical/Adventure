using Newtonsoft.Json.Linq;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class InventoryItem
    {
        public String Name { get; set; }

        public String Action { get; set; }

        public Equipment Equipment { get; set; }

        public long? Number { get; set; }
    }

    public interface IInventoryAction
    {
        void Use(InventoryItem item, Inventory inventory, CharacterSheet target);
    }

    public class Inventory
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

        private T CreateInstance<T>(String name)
        {
            var type = Type.GetType(name);
            var instance = (T)Activator.CreateInstance(type);
            return instance;
        }
    }
}
