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
    class SwordCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public SwordCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ButtonColumnItem<ShopEntry> CreateShopEntry(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            return new ButtonColumnItem<ShopEntry>
            {
                Text = $"{adjective} Sword",
                Item = new ShopEntry(100, () => new InventoryItem(CreateNormal(level), nameof(EquipMainHand)))
            };
        }

        public Equipment CreateNormal(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Sword",
                Attack = equipmentCurve.GetAttack(level),
                AttackPercent = 100,
                Sprite = nameof(Greatsword01)
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Epic Sword",
                Attack = equipmentCurve.GetAttack(level + 6),
                AttackPercent = 100,
                Sprite = nameof(Greatsword01)
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Legendary Sword",
                Attack = equipmentCurve.GetAttack(level + 9),
                AttackPercent = 100,
                Sprite = nameof(Greatsword01)
            };

            return sword;
        }
    }
}
