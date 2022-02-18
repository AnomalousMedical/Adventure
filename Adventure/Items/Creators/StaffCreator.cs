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
    class StaffCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public StaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public ShopEntry CreateShopEntry(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            return new ShopEntry($"{name.Adjective} Fire Staff", name.Cost * 2, () => new InventoryItem(CreateNormal(name.Level), nameof(EquipMainHand)));
        }

        public Equipment CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{name.Adjective} Fire Staff",
                MagicAttack = equipmentCurve.GetAttack(name.Level),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(name.Level) / 3,
                AttackPercent = 35,
                Sprite = nameof(Staff07),
                Spells = GetFireSpells(level),
                TwoHanded = true
            };

            return staff;
        }

        public Equipment CreateEpic(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{name.Adjective} Epic Fire Staff",
                MagicAttack = equipmentCurve.GetAttack(name.Level + 6),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(name.Level) / 3,
                AttackPercent = 35,
                Sprite = nameof(Staff07),
                Spells = GetFireSpells(level),
                TwoHanded = true
            };

            return staff;
        }

        public Equipment CreateLegendary(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{name.Adjective} Legendary Fire Staff",
                MagicAttack = equipmentCurve.GetAttack(name.Level + 12),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(name.Level) / 3,
                AttackPercent = 35,
                Sprite = nameof(Staff07),
                Spells = GetFireSpells(level),
                TwoHanded = true
            };

            return staff;
        }

        private IEnumerable<String> GetFireSpells(int level)
        {
            yield return nameof(Fir);
            if(level > 18)
            {
                yield return nameof(Fyre);
            }
            if(level > 32)
            {
                yield return nameof(Meltdown);
            }
        }
    }
}
