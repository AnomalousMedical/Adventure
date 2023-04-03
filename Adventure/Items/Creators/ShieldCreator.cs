using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
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

        public InventoryItem CreateNormal(int level, string adjective, bool quickGuard, string sprite = nameof(ShieldOfReflection))
        {
            var shield = new Equipment
            {
                Name = $"{adjective} Shield",
                Defense = equipmentCurve.GetDefense(level),
                MagicDefense = equipmentCurve.GetMDefense(level),
                Sprite = sprite,
                Skills = quickGuard ? new[] { nameof(QuickGuard) } : new[] { nameof(Guard) },
                AllowActiveBlock = true,
                ShowHand = false,
            };

            return CreateInventoryItem(shield);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }
    }
}
