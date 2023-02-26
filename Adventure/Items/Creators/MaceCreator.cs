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

        public InventoryItem CreateNormal(int level, string adjective)
        {
            var mace = new Equipment
            {
                Name = $"{adjective} Mace",
                Attack = equipmentCurve.GetAttack(level),
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
