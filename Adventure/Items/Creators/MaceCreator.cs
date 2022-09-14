using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using Adventure.Menu;
using RpgMath;

namespace Adventure.Items.Creators
{
    class MaceCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public MaceCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Mace", name.Cost, () => CreateNormal(name.Level));
        }

        public InventoryItem CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var mace = new Equipment
            {
                Name = $"{name.Adjective} Mace",
                Attack = equipmentCurve.GetAttack(name.Level),
                AttackPercent = 100,
                Sprite = nameof(MaceLarge2New),
                AttackElements = new[] { Element.Bludgeoning }
            };

            return CreateInventoryItem(mace);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipMainHand));
        }
    }
}
