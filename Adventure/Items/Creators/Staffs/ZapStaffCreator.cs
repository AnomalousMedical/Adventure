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
            if(level > 15)
            {
                yield return nameof(ZapBlast);
            }
            if (level > 28)
            {
                yield return nameof(ZapLash);
            }
            if (level > 38)
            {
                yield return nameof(ZapTempest);
            }
        }
    }
}
