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

        public ArmorCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreatePlate(int level, string adjective, EquipmentTier tier)
        {
            var armor = new Equipment
            {
                Name = $"{adjective} Plate Armor",
                Defense = equipmentCurve.GetDefense(level),
                MagicDefense = equipmentCurve.GetMDefense(level, 1f / 3f),
                Tier = tier,
            };

            return CreateInventoryItem(armor);
        }

        public InventoryItem CreateLeather(int level, string adjective, EquipmentTier tier)
        {
            var armor = new Equipment
            {
                Name = $"{adjective} Leather Armor",
                Defense = equipmentCurve.GetDefense(level, 2f / 3f),
                MagicDefense = equipmentCurve.GetMDefense(level, 2f / 3f),
                InventorySlots = level / 10 + 1,
                Tier = tier,
            };

            return CreateInventoryItem(armor);
        }

        public InventoryItem CreateCloth(int level, string adjective, EquipmentTier tier)
        {
            var armor = new Equipment
            {
                Name = $"{adjective} Cloth Armor",
                Defense = equipmentCurve.GetDefense(level, 1f / 3f),
                MagicDefense = equipmentCurve.GetMDefense(level),
                MagicAttack = level / 10 + 1,
                MagicAttackPercent = 50,
                Tier = tier,
            };

            return CreateInventoryItem(armor);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipBody));
        }
    }
}
