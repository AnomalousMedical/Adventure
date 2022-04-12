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
    class EarthStaffCreator : BaseStaffCreator
    {
        public EarthStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Earth", nameof(EarthStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Earth);
            if(level > 18)
            {
                yield return nameof(EarthBlast);
            }
            if (level > 28)
            {
                yield return nameof(EarthLash);
            }
            if (level > 38)
            {
                yield return nameof(EarthTempest);
            }
        }
    }
}
