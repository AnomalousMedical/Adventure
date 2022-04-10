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

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Sword", name.Cost, () => new InventoryItem(CreateNormal(name.Level), nameof(EquipMainHand)));
        }

        public Equipment CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Sword",
                Attack = equipmentCurve.GetAttack(name.Level),
                AttackPercent = 100,
                Sprite = nameof(Greatsword01),
                AttackElements = new[] { Element.Slashing }
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Epic Sword",
                Attack = equipmentCurve.GetAttack(name.Level + 6),
                AttackPercent = 100,
                Sprite = nameof(Greatsword01),
                AttackElements = new[] { Element.Slashing }
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Legendary Sword",
                Attack = equipmentCurve.GetAttack(name.Level + 12),
                AttackPercent = 100,
                Sprite = nameof(Greatsword01),
                AttackElements = new[] { Element.Slashing }
            };

            return sword;
        }
    }
}
