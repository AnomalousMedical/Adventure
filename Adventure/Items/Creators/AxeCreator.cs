using Adventure.Assets.Equipment;
using Adventure.Battle.Spells;
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
    //This is more of a holy hammer creator, but its axe art for now
    class AxeCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public AxeCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ButtonColumnItem<ShopEntry> CreateShopEntry(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            return new ButtonColumnItem<ShopEntry>
            {
                Text = $"{adjective} Axe",
                Item = new ShopEntry(100, () => new InventoryItem(CreateNormal(level), nameof(EquipMainHand)))
            };
        }

        public Equipment CreateNormal(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Axe",
                Attack = equipmentCurve.GetAttack(level),
                AttackPercent = 75,
                Sprite = nameof(BattleAxe6),
                Spells = GetCureSpells(level),
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Epic Axe",
                Attack = equipmentCurve.GetAttack(level + 6),
                AttackPercent = 75,
                Sprite = nameof(BattleAxe6),
                Spells = GetCureSpells(level),
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{adjective} Legendary Axe",
                Attack = equipmentCurve.GetAttack(level + 9),
                AttackPercent = 75,
                Sprite = nameof(BattleAxe6),
                Spells = GetCureSpells(level),
            };

            return sword;
        }

        private IEnumerable<String> GetCureSpells(int level)
        {
            yield return nameof(Cure);
        }
    }
}
