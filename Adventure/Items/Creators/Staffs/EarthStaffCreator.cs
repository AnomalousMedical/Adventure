using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using RpgMath;
using System;
using System.Collections.Generic;

namespace Adventure.Items.Creators
{
    class EarthStaffCreator : BaseStaffCreator
    {
        public EarthStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Earth", nameof(EarthStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Earth);
            if(level > SpellLevels.Blast)
            {
                yield return nameof(EarthBlast);
            }
            if (level > SpellLevels.Lash)
            {
                yield return nameof(EarthLash);
            }
            if (level > SpellLevels.Tempest)
            {
                yield return nameof(EarthTempest);
            }
        }
    }
}
