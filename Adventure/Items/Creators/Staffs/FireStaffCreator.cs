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
    class FireStaffCreator : BaseStaffCreator
    {
        public FireStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Fire", equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Fire);
            if(level > 18)
            {
                yield return nameof(FireBlast);
            }
            if (level > 28)
            {
                yield return nameof(FireLash);
            }
            if (level > 38)
            {
                yield return nameof(FireTempest);
            }
        }
    }
}
