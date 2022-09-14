using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using RpgMath;
using System;
using System.Collections.Generic;

namespace Adventure.Items.Creators
{
    class FireStaffCreator : BaseStaffCreator
    {
        public FireStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Fire", nameof(FireStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Fire);
            if(level > SpellLevels.Blast)
            {
                yield return nameof(FireBlast);
            }
            if (level > SpellLevels.Lash)
            {
                yield return nameof(FireLash);
            }
            if (level > SpellLevels.Tempest)
            {
                yield return nameof(FireTempest);
            }
        }
    }
}
