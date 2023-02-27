using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using Adventure.Items.Actions;
using Adventure.Menu;
using RpgMath;
using System;
using System.Collections.Generic;

namespace Adventure.Items.Creators
{
    class DaggerCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public DaggerCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, string adjective)
        {
            var sword = new Equipment
            {
                Name = $"{adjective} Dagger",
                Attack = equipmentCurve.GetAttack(level),
                Sprite = nameof(DaggerNew),
                Skills = GetSkills(level),
                AllowActiveBlock = level > SpellLevels.Superior //Flawless gets active block
            };

            if(level > SpellLevels.Superior)
            {
                //Flawless and above gets counter
                sword.CounterPercent = 40L;
            }

            return CreateInventoryItem(sword);
        }

        private IEnumerable<String> GetSkills(int level)
        {
            yield return nameof(Steal);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }
    }
}
