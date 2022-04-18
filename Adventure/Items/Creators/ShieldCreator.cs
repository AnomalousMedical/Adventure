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
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Shield", name.Cost, () => CreateNormal(name.Level));
        }

        public InventoryItem CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{name.Adjective} Shield",
                Defense = equipmentCurve.GetDefense(name.Level),
                MagicDefense = equipmentCurve.GetMDefense(name.Level),
                Sprite = nameof(ShieldOfReflection)
            };

            return CreateInventoryItem(shield);
        }

        public InventoryItem CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{name.Adjective} Epic Shield",
                Defense = equipmentCurve.GetDefense(name.Level + 6),
                MagicDefense = equipmentCurve.GetMDefense(name.Level + 6),
                Sprite = nameof(ShieldOfReflection)
            };

            return CreateInventoryItem(shield);
        }

        public InventoryItem CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var shield = new Equipment
            {
                Name = $"{name.Adjective} Legendary Shield",
                Defense = equipmentCurve.GetDefense(name.Level + 12),
                MagicDefense = equipmentCurve.GetMDefense(name.Level + 12),
                Sprite = nameof(ShieldOfReflection)
            };

            return CreateInventoryItem(shield);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }
    }
}
