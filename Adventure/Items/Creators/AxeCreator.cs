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

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Axe", name.Cost, () => new InventoryItem(CreateNormal(name.Level), nameof(EquipMainHand)));
        }

        public Equipment CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Axe",
                Attack = equipmentCurve.GetAttack(name.Level),
                AttackPercent = 75,
                Sprite = nameof(BattleAxe6),
                Spells = GetCureSpells(level),
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Epic Axe",
                Attack = equipmentCurve.GetAttack(name.Level + 6),
                AttackPercent = 75,
                Sprite = nameof(BattleAxe6),
                Spells = GetCureSpells(level),
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Legendary Axe",
                Attack = equipmentCurve.GetAttack(name.Level + 12),
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
