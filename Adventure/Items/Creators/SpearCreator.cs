using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using Adventure.Menu;
using RpgMath;

namespace Adventure.Items.Creators
{
    class SpearCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public SpearCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Spear", name.Cost, () => CreateNormal(name.Level));
        }

        public InventoryItem CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var spear = new Equipment
            {
                Name = $"{name.Adjective} Spear",
                Attack = equipmentCurve.GetAttack(name.Level),
                AttackPercent = 100,
                Sprite = nameof(Spear2Old),
                AttackElements = new[] { Element.Piercing }
            };

            return CreateInventoryItem(spear);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipMainHand));
        }
    }
}
