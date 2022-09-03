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
    class ZapStaffCreator : BaseStaffCreator
    {
        public ZapStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Zap", nameof(ZapStaff07), equipmentCurve, nameGenerator)
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
