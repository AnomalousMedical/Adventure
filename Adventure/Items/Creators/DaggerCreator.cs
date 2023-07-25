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

        public InventoryItem CreateNormal(int level, string adjective, params String[] skills)
        {
            return CreateNormal(nameof(DaggerNew), level, adjective, skills);
        }

        public InventoryItem CreateNormal(String spriteName, int level, string adjective, params String[] skills)
        {
            var sword = new Equipment
            {
                Name = $"{adjective} Dagger",
                Attack = equipmentCurve.GetAttack(level),
                Sprite = spriteName,
                Skills = skills.ToArray(),
                AllowTriggerAttack = true,
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
