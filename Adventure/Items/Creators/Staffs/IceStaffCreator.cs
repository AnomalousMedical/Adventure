using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using RpgMath;
using System;
using System.Collections.Generic;

namespace Adventure.Items.Creators
{
    class IceStaffCreator : BaseStaffCreator
    {
        public IceStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Ice", nameof(IceStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Ice);
            if (level > SpellLevels.Blast)
            {
                yield return nameof(IceBlast);
            }
            if (level > SpellLevels.Lash)
            {
                yield return nameof(IceLash);
            }
            if (level > SpellLevels.Tempest)
            {
                yield return nameof(IceTempest);
            }
        }
    }
}
