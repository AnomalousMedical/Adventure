using Adventure.Assets.Equipment;
using Adventure.Exploration.Menu;
using Adventure.Items.Actions;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Creators
{
    class ShieldCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public ShieldCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{adjective} Shield", 100, () => new InventoryItem(CreateNormal(level), nameof(EquipOffHand)));
        }

        public Equipment CreateNormal(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{adjective} Shield",
                Defense = equipmentCurve.GetDefense(level),
                MagicDefense = equipmentCurve.GetMDefense(level),
                Sprite = nameof(ShieldOfReflection)
            };

            return shield;
        }

        public Equipment CreateEpic(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{adjective} Epic Shield",
                Defense = equipmentCurve.GetDefense(level + 6),
                MagicDefense = equipmentCurve.GetMDefense(level + 6),
                Sprite = nameof(ShieldOfReflection)
            };

            return shield;
        }

        public Equipment CreateLegendary(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{adjective} Legendary Shield",
                Defense = equipmentCurve.GetDefense(level + 9),
                MagicDefense = equipmentCurve.GetMDefense(level + 9),
                Sprite = nameof(ShieldOfReflection)
            };

            return shield;
        }
    }
}
