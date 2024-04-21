using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;

namespace Adventure.Items.Creators
{
    class SwordCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public SwordCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, string adjective, string sprite)
        {
            var sword = new Equipment
            {
                Name = $"{adjective} Sword",
                Attack = equipmentCurve.GetAttack(level),
                AttackPercent = 100,
                Sprite = sprite,
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
