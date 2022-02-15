using Adventure.Exploration.Menu;
using Adventure.Items.Actions;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Creators
{
    class PotionCreator
    {
        public ButtonColumnItem<ShopEntry> CreateManaPotionShopEntry(int level)
        {
            return new ButtonColumnItem<ShopEntry>
            {
                Text = $"Mana Potion",
                Item = new ShopEntry(50, () => CreateManaPotion(1))
            };
        }

        public InventoryItem CreateManaPotion(int level)
        {
            var item = new InventoryItem
            {
                Action = nameof(RestoreMp),
            };

            if (level < 30)
            {
                item.Number = 25;
                item.Name = "Mana Potion";
            }
            else if (level < 65)
            {
                item.Number = 75;
                item.Name = "Big Mana Potion";
            }
            else
            {
                item.Number = 150;
                item.Name = "Giant Mana Potion";
            }

            return item;
        }

        public InventoryItem CreateHealthPotion(int level)
        {
            var item = new InventoryItem
            {
                Action = nameof(RestoreHp),
            };

            if (level < 30)
            {
                item.Number = 50;
                item.Name = "Health Potion";
            }
            else if (level < 65)
            {
                item.Number = 300;
                item.Name = "Big Health Potion";
            }
            else
            {
                item.Number = 1500;
                item.Name = "Giant Health Potion";
            }

            return item;
        }

        public InventoryItem CreateFerrymansBribe()
        {
            var item = new InventoryItem
            {
                Action = nameof(Resurrect),
            };

            item.Number = 25;
            item.Name = "Ferryman's Bribe";

            return item;
        }


    }
}
