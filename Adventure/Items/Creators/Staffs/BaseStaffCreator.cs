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
    abstract class BaseStaffCreator
    {
        private readonly string typeName;
        private readonly string sprite;
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        protected BaseStaffCreator(String typeName, String sprite, IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.typeName = typeName;
            this.sprite = sprite;
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} {this.typeName} Staff", name.Cost * 2, () => new InventoryItem(CreateNormal(name.Level), nameof(EquipMainHand)));
        }

        public Equipment CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{name.Adjective} {typeName} Staff",
                MagicAttack = equipmentCurve.GetAttack(name.Level),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(name.Level) / 3,
                AttackPercent = 35,
                Sprite = sprite,
                Skills = GetSpells(level),
                TwoHanded = true,
                AttackElements = new[] { Element.Bludgeoning }
            };

            return staff;
        }

        public Equipment CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{name.Adjective} Epic {typeName} Staff",
                MagicAttack = equipmentCurve.GetAttack(name.Level + 6),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(name.Level) / 3,
                AttackPercent = 35,
                Sprite = sprite,
                Skills = GetSpells(level),
                TwoHanded = true,
                AttackElements = new[] { Element.Bludgeoning }
            };

            return staff;
        }

        public Equipment CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{name.Adjective} Legendary {typeName} Staff",
                MagicAttack = equipmentCurve.GetAttack(name.Level + 12),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(name.Level) / 3,
                AttackPercent = 35,
                Sprite = sprite,
                Skills = GetSpells(level),
                TwoHanded = true,
                AttackElements = new[] { Element.Bludgeoning }
            };

            return staff;
        }

        protected abstract IEnumerable<String> GetSpells(int level);
    }
}
