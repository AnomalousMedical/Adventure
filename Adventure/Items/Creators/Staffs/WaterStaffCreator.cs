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
    class WaterStaffCreator : BaseStaffCreator
    {
        public WaterStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Water", nameof(WaterStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Water);
            if(level > 18)
            {
                yield return nameof(WaterBlast);
            }
            if (level > 28)
            {
                yield return nameof(WaterLash);
            }
            if (level > 38)
            {
                yield return nameof(WaterTempest);
            }
        }
    }
}
