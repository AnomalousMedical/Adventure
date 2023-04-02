using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;

namespace Adventure.Items.Creators
{
    class SpearCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public SpearCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, string adjective, string sprite = nameof(Spear2Old))
        {
            var spear = new Equipment
            {
                Name = $"{adjective} Spear",
                Attack = equipmentCurve.GetAttack(level),
                AttackPercent = 100,
                Sprite = sprite,
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
