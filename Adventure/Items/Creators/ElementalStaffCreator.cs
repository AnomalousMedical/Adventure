﻿using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;
using System;
using System.Linq;

namespace Adventure.Items.Creators
{
    class ElementalStaffCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public ElementalStaffCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, String adjective, params string[] spells)
        {
            return CreateNormal(nameof(IceStaff07), level, adjective, spells);
        }

        public InventoryItem CreateNormal(String sprite, int level, String adjective, params string[] spells)
        {
            var staff = new Equipment
            {
                Name = $"{adjective} Staff",
                MagicAttack = equipmentCurve.GetAttack(level),
                MagicAttackPercent = 50,
                Attack = equipmentCurve.GetAttack(level) / 3,
                AttackPercent = 35,
                Sprite = sprite,
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
