using Adventure.Items.Actions;

namespace Adventure.Items.Creators
{
    class PotionCreator
    {
        public InventoryItem CreateManaPotion(int level)
        {
            var item = new InventoryItem
            {
                Action = nameof(RestoreMp),
            };

            if (level < SpellLevels.Regular)
            {
                item.Number = 25;
                item.Name = "Mana Potion";
            }
            else if (level < SpellLevels.Big)
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

            if (level < SpellLevels.Regular)
            {
                item.Number = 50;
                item.Name = "Health Potion";
            }
            else if (level < SpellLevels.Big)
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
                Action = nameof(Revive),
            };

            item.Number = 25;
            item.Name = "Ferryman's Bribe";

            return item;
        }

        public InventoryItem CreateStrengthBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(StrengthBoost),
                CanUseOnPickup = true,
            };

            item.Number = 1;
            item.Name = "Strength Boost";

            return item;
        }

        public InventoryItem CreateMagicBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(MagicBoost),
                CanUseOnPickup = true,
            };

            item.Number = 1;
            item.Name = "Magic Boost";

            return item;
        }

        public InventoryItem CreateSpiritBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(SpiritBoost),
                CanUseOnPickup = true,
            };

            item.Number = 1;
            item.Name = "Spirit Boost";

            return item;
        }

        public InventoryItem CreateVitalityBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(VitalityBoost),
                CanUseOnPickup = true,
            };

            item.Number = 1;
            item.Name = "Vitality Boost";

            return item;
        }

        public InventoryItem CreateDexterityBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(DexterityBoost),
                CanUseOnPickup = true,
            };

            item.Number = 1;
            item.Name = "Dexterity Boost";

            return item;
        }

        public InventoryItem CreateLuckBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(LuckBoost),
                CanUseOnPickup = true,
            };

            item.Number = 1;
            item.Name = "Luck Boost";

            return item;
        }
    }
}
