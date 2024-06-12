using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;

namespace Adventure.Items.Creators
{
    class ShieldCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public ShieldCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, string infoId, float damageReduction, string sprite)
        {
            var shield = new Equipment
            {
                InfoId = infoId,
                Defense = equipmentCurve.GetDefense(level),
                MagicDefense = equipmentCurve.GetMDefense(level),
                Sprite = sprite,
                AllowActiveBlock = true,
                ShowHand = false,
                BlockDamageReduction = damageReduction,
            };

            return CreateInventoryItem(shield);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand))
            {
                Unique = true
            };
        }
    }
}
