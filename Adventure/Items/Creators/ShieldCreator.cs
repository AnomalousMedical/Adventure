using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using Adventure.Items.Actions;
using RpgMath;

namespace Adventure.Items.Creators
{
    class ShieldCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public ShieldCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public InventoryItem CreateNormal(int level, string adjective, long guardPercent)
        {
            var name = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{name.Adjective} Shield",
                Defense = equipmentCurve.GetDefense(name.Level),
                MagicDefense = equipmentCurve.GetMDefense(name.Level),
                Sprite = nameof(ShieldOfReflection),
                Skills = new[] { nameof(Guard) },
                GuardPercent = guardPercent,
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
