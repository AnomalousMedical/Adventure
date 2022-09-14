using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using Adventure.Menu;
using RpgMath;

namespace Adventure.Items.Creators
{
    class SwordCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public SwordCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Sword", name.Cost, () => CreateNormal(name.Level));
        }

        public InventoryItem CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Sword",
                Attack = equipmentCurve.GetAttack(name.Level),
                AttackPercent = 100,
                Sprite = nameof(Greatsword01),
                AttackElements = new[] { Element.Slashing }
            };

            return CreateInventoryItem(sword);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipMainHand));
        }
    }
}
