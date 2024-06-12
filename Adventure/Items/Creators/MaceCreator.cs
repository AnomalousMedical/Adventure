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

        public InventoryItem CreateNormal(int level, string infoId, string sprite, bool unique)
        {
            var mace = new Equipment
            {
                InfoId = infoId,
                Attack = equipmentCurve.GetAttack(level),
                AttackPercent = 100,
                Sprite = sprite,
                AttackElements = new[] { Element.Bludgeoning }
            };

            return CreateInventoryItem(mace, unique);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment, bool unique)
        {
            return new InventoryItem(equipment, nameof(EquipMainHand))
            {
                Unique = unique
            };
        }
    }
}
