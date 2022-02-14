using Adventure.Assets.Equipment;
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

        public Equipment CreateNormal(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Armor",
                Defense = equipmentCurve.GetDefense(level),
                MagicDefense = equipmentCurve.GetMDefense(level)
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Epic Armor",
                Defense = equipmentCurve.GetDefense(level + 6),
                MagicDefense = equipmentCurve.GetMDefense(level + 6)
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Legendary Armor",
                Defense = equipmentCurve.GetDefense(level + 9),
                MagicDefense = equipmentCurve.GetMDefense(level + 9)
            };

            return sword;
        }
    }
}
