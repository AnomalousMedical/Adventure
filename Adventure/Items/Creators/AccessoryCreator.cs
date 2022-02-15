﻿using Adventure.Assets.Equipment;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Creators
{
    class AccessoryCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public AccessoryCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public Equipment CreateNormal(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Accessory"
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Epic Accessory"
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Legendary Accessory"
            };

            return sword;
        }
    }
}