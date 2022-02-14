using Adventure.Items.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Creators
{
    class PotionCreator
    {
        public ITreasure CreateManaPotion(int level)
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

            return new Treasure(item);
        }

        public ITreasure CreateHealthPotion(int level)
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

            return new Treasure(item);
        }

        public ITreasure CreateFerrymansBribe()
        {
            var item = new InventoryItem
            {
                Action = nameof(Resurrect),
            };

            item.Number = 25;
            item.Name = "Ferryman's Bribe";

            return new Treasure(item);
        }
    }
}
