using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
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
    class DaggerCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public DaggerCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Dagger", name.Cost, () => new InventoryItem(CreateNormal(name.Level), nameof(EquipOffHand)));
        }

        public Equipment CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Dagger",
                Attack = equipmentCurve.GetAttack(name.Level),
                Sprite = nameof(DaggerNew),
                Skills = GetSkills(level)
            };

            return sword;
        }

        public Equipment CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Epic Dagger",
                Attack = equipmentCurve.GetAttack(name.Level + 6),
                Sprite = nameof(DaggerNew),
                Skills = GetSkills(level)
            };

            return sword;
        }

        public Equipment CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Legendary Dagger",
                Attack = equipmentCurve.GetAttack(name.Level + 12),
                Sprite = nameof(DaggerNew),
                Skills = GetSkills(level),
            };

            return sword;
        }

        private IEnumerable<String> GetSkills(int level)
        {
            yield return nameof(Steal);
        }
    }
}
