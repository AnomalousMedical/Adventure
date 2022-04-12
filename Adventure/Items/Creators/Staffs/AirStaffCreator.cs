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
    class AirStaffCreator : BaseStaffCreator
    {
        public AirStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Air", nameof(AirStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Air);
            if(level > 18)
            {
                yield return nameof(AirBlast);
            }
            if (level > 28)
            {
                yield return nameof(AirLash);
            }
            if (level > 38)
            {
                yield return nameof(AirTempest);
            }
        }
    }
}
