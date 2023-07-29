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

        public InventoryItem CreateStrengthBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(StrengthBoost),
                CanUseOnPickup = true,
            };

            item.Number = amount;
            item.Name = $"Strength Boost +{amount}";

            return item;
        }

        public InventoryItem CreateMagicBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(MagicBoost),
                CanUseOnPickup = true,
            };

            item.Number = amount;
            item.Name = $"Magic Boost +{amount}";

            return item;
        }

        public InventoryItem CreateSpiritBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(SpiritBoost),
                CanUseOnPickup = true,
            };

            item.Number = amount;
            item.Name = $"Spirit Boost +{amount}";

            return item;
        }

        public InventoryItem CreateVitalityBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(VitalityBoost),
                CanUseOnPickup = true,
            };

            item.Number = amount;
            item.Name = $"Vitality Boost +{amount}";

            return item;
        }

        public InventoryItem CreateDexterityBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(DexterityBoost),
                CanUseOnPickup = true,
            };

            item.Number = amount;
            item.Name = $"Dexterity Boost +{amount}";

            return item;
        }

        public InventoryItem CreateLuckBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(LuckBoost),
                CanUseOnPickup = true,
            };

            item.Number = amount;
            item.Name = $"Luck Boost +{amount}";

            return item;
        }

        public InventoryItem CreateLevelBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(LevelBoost),
                CanUseOnPickup = true,
            };

            item.Number = 5;
            item.Name = "Level Boost";

            return item;
        }
    }
}
