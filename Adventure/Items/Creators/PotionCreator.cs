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
                CanUseOnPickup = true,
            };

            if (level < SpellLevels.Regular)
            {
                item.Number = 45;
                item.InfoId = nameof(ItemText.Mana1);
            }
            else if (level < SpellLevels.Big)
            {
                item.Number = 75;
                item.InfoId = nameof(ItemText.Mana2);
            }
            else
            {
                item.Number = 150;
                item.InfoId = nameof(ItemText.Mana3);
            }

            return item;
        }

        public InventoryItem CreateHealthPotion(int level)
        {
            var item = new InventoryItem
            {
                Action = nameof(RestoreHp),
                CanUseOnPickup = true,
            };

            if (level < SpellLevels.Regular)
            {
                item.Number = 50;
                item.InfoId = nameof(ItemText.Health1);
            }
            else if (level < SpellLevels.Big)
            {
                item.Number = 300;
                item.InfoId = nameof(ItemText.Health2);
            }
            else
            {
                item.Number = 1500;
                item.InfoId = nameof(ItemText.Health3);
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
            item.InfoId = nameof(ItemText.FerrymansBribe);

            return item;
        }

        public InventoryItem CreateStrengthBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(StrengthBoost),
                CanUseOnPickup = true,
                Unique = true,
            };

            item.Number = amount;
            item.InfoId = nameof(ItemText.StrengthBoost);

            return item;
        }

        public InventoryItem CreateMagicBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(MagicBoost),
                CanUseOnPickup = true,
                Unique = true,
            };

            item.Number = amount;
            item.InfoId = nameof(ItemText.MagicBoost);

            return item;
        }

        public InventoryItem CreateSpiritBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(SpiritBoost),
                CanUseOnPickup = true,
                Unique = true,
            };

            item.Number = amount;
            item.InfoId = nameof(ItemText.SpiritBoost);

            return item;
        }

        public InventoryItem CreateVitalityBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(VitalityBoost),
                CanUseOnPickup = true,
                Unique = true,
            };

            item.Number = amount;
            item.InfoId = nameof(ItemText.VitalityBoost);

            return item;
        }

        public InventoryItem CreateDexterityBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(DexterityBoost),
                CanUseOnPickup = true,
                Unique = true,
            };

            item.Number = amount;
            item.InfoId = nameof(ItemText.DexterityBoost);

            return item;
        }

        public InventoryItem CreateLuckBoost(int amount)
        {
            var item = new InventoryItem
            {
                Action = nameof(LuckBoost),
                CanUseOnPickup = true,
                Unique = true,
            };

            item.Number = amount;
            item.InfoId = nameof(ItemText.LuckBoost);

            return item;
        }

        public InventoryItem CreateLevelBoost()
        {
            var item = new InventoryItem
            {
                Action = nameof(LevelBoost),
                CanUseOnPickup = true,
                Unique = true,
            };

            item.Number = 3;
            item.InfoId = nameof(ItemText.LevelBoost);

            return item;
        }
    }
}
