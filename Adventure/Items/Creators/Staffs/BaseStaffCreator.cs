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
    interface IStaffCreator
    {
        InventoryItem CreateEpic(int level);
        InventoryItem CreateLegendary(int level);
        InventoryItem CreateNormal(int level);
        ShopEntry CreateShopEntry(int level);
    }

    abstract class BaseStaffCreator : IStaffCreator
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

            return new ShopEntry($"{name.Adjective} {this.typeName} Staff", name.Cost * 2, () => CreateNormal(name.Level));
        }

        public InventoryItem CreateNormal(int level)
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

            return CreateInventoryItem(staff);
        }

        public InventoryItem CreateEpic(int level)
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

            return CreateInventoryItem(staff);
        }

        public InventoryItem CreateLegendary(int level)
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

            return CreateInventoryItem(staff);
        }

        protected abstract IEnumerable<String> GetSpells(int level);

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipMainHand));
        }
    }
}
