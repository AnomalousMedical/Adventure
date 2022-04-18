using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Creators
{
    class ArmorCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public ArmorCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public InventoryItem CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var armor = new Equipment
            {
                Name = $"{name.Adjective} Armor",
                Defense = equipmentCurve.GetDefense(name.Level),
                MagicDefense = equipmentCurve.GetMDefense(name.Level)
            };

            return CreateInventoryItem(armor);
        }

        public InventoryItem CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var armor = new Equipment
            {
                Name = $"{name.Adjective} Epic Armor",
                Defense = equipmentCurve.GetDefense(name.Level + 6),
                MagicDefense = equipmentCurve.GetMDefense(name.Level + 6)
            };

            return CreateInventoryItem(armor);
        }

        public InventoryItem CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var armor = new Equipment
            {
                Name = $"{name.Adjective} Legendary Armor",
                Defense = equipmentCurve.GetDefense(name.Level + 12),
                MagicDefense = equipmentCurve.GetMDefense(name.Level + 12)
            };

            return CreateInventoryItem(armor);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipBody));
        }
    }
}
