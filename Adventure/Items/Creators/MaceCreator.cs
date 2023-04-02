using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using Adventure.Menu;
using RpgMath;

namespace Adventure.Items.Creators
{
    class MaceCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public MaceCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, string adjective, string sprite = nameof(MaceLarge2New))
        {
            var mace = new Equipment
            {
                Name = $"{adjective} Mace",
                Attack = equipmentCurve.GetAttack(level),
                AttackPercent = 100,
                Sprite = sprite,
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
