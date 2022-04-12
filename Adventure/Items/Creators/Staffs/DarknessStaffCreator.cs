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
    class DarknessStaffCreator : BaseStaffCreator
    {
        public DarknessStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Darkness", nameof(DarknessStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Darkness);
            if(level > 18)
            {
                yield return nameof(DarknessBlast);
            }
            if (level > 28)
            {
                yield return nameof(DarknessLash);
            }
            if (level > 38)
            {
                yield return nameof(DarknessTempest);
            }
        }
    }
}
