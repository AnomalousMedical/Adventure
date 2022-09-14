using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using Adventure.Items.Actions;
using Adventure.Menu;
using RpgMath;

namespace Adventure.Items.Creators
{
    class ShieldCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public ShieldCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Shield", name.Cost, () => CreateNormal(name.Level));
        }

        public InventoryItem CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{name.Adjective} Shield",
                Defense = equipmentCurve.GetDefense(name.Level),
                MagicDefense = equipmentCurve.GetMDefense(name.Level),
                Sprite = nameof(ShieldOfReflection),
                Skills = new[] { nameof(Guard) },
                GuardPercent = GetGuardPercent(level),
                AllowActiveBlock = true,
                ShowHand = false,
            };

            return CreateInventoryItem(shield);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }

        private long GetGuardPercent(int level)
        {
            if (level < SpellLevels.Busted)
            {
                return 60L;
            }
            else if (level < SpellLevels.Common)
            {
                return 70L;
            }
            else if (level < SpellLevels.Superior)
            {
                return 80L;
            }
            else
            {
                return 90L;
            }
        }
    }
}
