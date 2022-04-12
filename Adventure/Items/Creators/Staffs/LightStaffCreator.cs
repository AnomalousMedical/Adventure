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
    class LightStaffCreator : BaseStaffCreator
    {
        public LightStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Light", nameof(LightStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Light);
            if(level > 18)
            {
                yield return nameof(LightBlast);
            }
            if (level > 28)
            {
                yield return nameof(LightLash);
            }
            if (level > 38)
            {
                yield return nameof(LightTempest);
            }
        }
    }
}
