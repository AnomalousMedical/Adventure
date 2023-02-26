using Adventure.Assets.Equipment;
using Adventure.Battle.Skills;
using RpgMath;
using System;
using System.Collections.Generic;

namespace Adventure.Items.Creators
{
    class ZapStaffCreator : BaseStaffCreator
    {
        public ZapStaffCreator(IEquipmentCurve equipmentCurve)
            :base("Zap", nameof(ZapStaff07), equipmentCurve)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Zap);
            if (level > SpellLevels.Blast)
            {
                yield return nameof(ZapBlast);
            }
            if (level > SpellLevels.Lash)
            {
                yield return nameof(ZapLash);
            }
            if (level > SpellLevels.Tempest)
            {
                yield return nameof(ZapTempest);
            }
        }
    }
}
