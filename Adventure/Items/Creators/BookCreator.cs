using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using Adventure.Items.Actions;
using RpgMath;
using System;
using System.Collections.Generic;

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

        public InventoryItem CreateCure(int level, string adjective)
        {
            var book = new Equipment
            {
                Name = $"{adjective} Restoration",
                MagicAttack = equipmentCurve.GetAttack(level),
                Sprite = nameof(FancyBook),
                Skills = GetCureSpells(level),
                ShowHand = false,
                AllowActiveBlock = level > SpellLevels.Superior
            };

            return CreateInventoryItem(book);
        }

        private IEnumerable<String> GetCureSpells(int level)
        {
            yield return nameof(Cure);
            if (level > SpellLevels.MegaCure)
            {
                yield return nameof(MegaCure);
            }
            if (level > SpellLevels.UltraCure)
            {
                yield return nameof(UltraCure);
            }
        }

        public InventoryItem CreateReanimation(int level, string adjective)
        {
            var book = new Equipment
            {
                Name = $"{adjective} Reanimation",
                MagicAttack = equipmentCurve.GetAttack(level),
                Sprite = nameof(FancyBook),
                Skills = GetReanimationSpells(level),
                ShowHand = false,
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
