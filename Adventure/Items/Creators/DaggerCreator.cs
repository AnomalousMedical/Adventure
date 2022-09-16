using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using Adventure.Items.Actions;
using Adventure.Menu;
using RpgMath;
using System;
using System.Collections.Generic;

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

            return new ShopEntry($"{name.Adjective} Dagger", name.Cost, () => CreateNormal(name.Level));
        }

        public InventoryItem CreateNormal(int level)
        {
            var name = nameGenerator.GetLevelName(level);

            var sword = new Equipment
            {
                Name = $"{name.Adjective} Dagger",
                Attack = equipmentCurve.GetAttack(name.Level),
                Sprite = nameof(DaggerNew),
                Skills = GetSkills(level),
                AllowActiveBlock = level > SpellLevels.Superior //Flawless gets active block
            };

            if(level > SpellLevels.Superior)
            {
                //Flawless and above gets counter
                sword.SpecialEffects = new[] { BattleSpecialEffects.Counterattack };
            }

            return CreateInventoryItem(sword);
        }

        private IEnumerable<String> GetSkills(int level)
        {
            yield return nameof(Steal);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }
    }
}
