using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;
using System;
using System.Linq;

namespace Adventure.Items.Creators
{
    class DaggerCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public DaggerCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateNormal(int level, string adjective, bool allowTrigger, bool allowActiveBlock, params String[] skills)
        {
            var sword = new Equipment
            {
                Name = $"{adjective} Dagger",
                Attack = equipmentCurve.GetAttack(level),
                Sprite = nameof(DaggerNew),
                Skills = skills.ToArray(),
                AllowTriggerAttack = true,
                AllowActiveBlock = level > SpellLevels.Superior //Flawless gets active block
            };

            if(level > SpellLevels.Superior)
            {
                //Flawless and above gets counter
                sword.CounterPercent = 40L;
            }

            return CreateInventoryItem(sword);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }
    }
}
