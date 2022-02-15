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

        public ButtonColumnItem<ShopEntry> CreateShopEntry(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            return new ButtonColumnItem<ShopEntry>
            {
                Text = $"{adjective} Fire Staff",
                Item = new ShopEntry(100, () => new InventoryItem(CreateNormal(level), nameof(EquipMainHand)))
            };
        }

        public Equipment CreateNormal(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{adjective} Fire Staff",
                MagicAttack = equipmentCurve.GetAttack(level),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(level) / 3,
                AttackPercent = 35,
                Sprite = nameof(Staff07),
                Spells = GetFireSpells(level),
            };

            return staff;
        }

        public Equipment CreateEpic(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{adjective} Epic Fire Staff",
                MagicAttack = equipmentCurve.GetAttack(level + 6),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(level) / 3,
                AttackPercent = 35,
                Sprite = nameof(Staff07)
            };

            return staff;
        }

        public Equipment CreateLegendary(int level)
        {
            var adjective = nameGenerator.GetLevelName(level);

            var staff = new Equipment
            {
                Name = $"{adjective} Legendary Fire Staff",
                MagicAttack = equipmentCurve.GetAttack(level + 9),
                MagicAttackPercent = 100,
                Attack = equipmentCurve.GetAttack(level) / 3,
                AttackPercent = 35,
                Sprite = nameof(Staff07)
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
