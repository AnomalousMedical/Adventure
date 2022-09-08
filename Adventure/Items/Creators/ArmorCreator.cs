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

        public InventoryItem CreatePlate(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var armor = new Equipment
            {
                Name = $"{name.Adjective} Plate Armor",
                Defense = equipmentCurve.GetDefense(name.Level),
                MagicDefense = equipmentCurve.GetMDefense(name.Level, 1f / 3f)
            };

            return CreateInventoryItem(armor);
        }

        public InventoryItem CreateLeather(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var armor = new Equipment
            {
                Name = $"{name.Adjective} Leather Armor",
                Defense = equipmentCurve.GetDefense(name.Level, 2f / 3f),
                MagicDefense = equipmentCurve.GetMDefense(name.Level, 2f / 3f),
                InventorySlots = level / 10 + 1
            };

            return CreateInventoryItem(armor);
        }

        public InventoryItem CreateCloth(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var armor = new Equipment
            {
                Name = $"{name.Adjective} Cloth Armor",
                Defense = equipmentCurve.GetDefense(name.Level, 1f / 3f),
                MagicDefense = equipmentCurve.GetMDefense(name.Level),
                MagicAttack = level / 10 + 1
            };

            return CreateInventoryItem(armor);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipBody));
        }
    }
}
