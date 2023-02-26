using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;
using System;
using System.Linq;

namespace Adventure.Items.Creators
{
    class ElementalStaffCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        protected ElementalStaffCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, String adjective, params string[] spells)
        {
            var staff = new Equipment
            {
                Name = $"{adjective} Staff",
                MagicAttack = equipmentCurve.GetAttack(level),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(level) / 3,
                AttackPercent = 35,
                Sprite = nameof(IceStaff07),
                Skills = spells.ToArray(),
                TwoHanded = true,
                AttackElements = new[] { Element.Bludgeoning }
            };

            return CreateInventoryItem(staff);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipMainHand));
        }
    }
}
