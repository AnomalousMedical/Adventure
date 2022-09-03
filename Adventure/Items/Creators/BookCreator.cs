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
    class BookCreator
    {
        private readonly IEquipmentCurve equipmentCurve;
        private readonly INameGenerator nameGenerator;

        public BookCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
        {
            this.equipmentCurve = equipmentCurve;
            this.nameGenerator = nameGenerator;
        }

        public InventoryItem CreateCure(int level)
        {
            var name = nameGenerator.GetBookLevelName(level);

            var book = new Equipment
            {
                Name = $"{name.Adjective} Restoration",
                MagicAttack = equipmentCurve.GetAttack(name.Level),
                Sprite = nameof(FancyBook),
                Skills = GetCureSpells(level)
            };

            return CreateInventoryItem(book);
        }

        private IEnumerable<String> GetCureSpells(int level)
        {
            yield return nameof(Cure);
            if (level > 40)
            {
                yield return nameof(MegaCure);
            }
            if (level > 60)
            {
                yield return nameof(UltraCure);
            }
        }

        public InventoryItem CreateReanimation(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var book = new Equipment
            {
                Name = $"{name.Adjective} Reanimation",
                MagicAttack = equipmentCurve.GetAttack(name.Level),
                Sprite = nameof(FancyBook),
                Skills = GetReanimationSpells(level)
            };

            return CreateInventoryItem(book);
        }

        private IEnumerable<String> GetReanimationSpells(int level)
        {
            yield return nameof(Reanimate);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }
    }
}
