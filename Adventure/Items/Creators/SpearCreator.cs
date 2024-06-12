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

        public InventoryItem CreateNormal(int level, string infoId, string sprite, bool unique)
        {
            var spear = new Equipment
            {
                InfoId = infoId,
                Attack = equipmentCurve.GetAttack(level),
                AttackPercent = 100,
                Sprite = sprite,
                AttackElements = new[] { Element.Piercing },
            };

            return CreateInventoryItem(spear, unique);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment, bool unique)
        {
            return new InventoryItem(equipment, nameof(EquipMainHand))
            {
                Unique = unique,
            };
        }
    }
}
