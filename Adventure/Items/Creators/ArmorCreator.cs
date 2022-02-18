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
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Armor",
                Defense = equipmentCurve.GetDefense(name.Level),
                MagicDefense = equipmentCurve.GetMDefense(name.Level)
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Epic Armor",
                Defense = equipmentCurve.GetDefense(name.Level + 6),
                MagicDefense = equipmentCurve.GetMDefense(name.Level + 6)
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Legendary Armor",
                Defense = equipmentCurve.GetDefense(name.Level + 9),
                MagicDefense = equipmentCurve.GetMDefense(name.Level + 9)
            };

            return sword;
        }
    }
}
