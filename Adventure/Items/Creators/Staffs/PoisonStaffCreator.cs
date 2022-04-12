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
    class PoisonStaffCreator : BaseStaffCreator
    {
        public PoisonStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Poison", nameof(PoisonStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Poison);
            if(level > 18)
            {
                yield return nameof(PoisonBlast);
            }
            if (level > 28)
            {
                yield return nameof(PoisonLash);
            }
            if (level > 38)
            {
                yield return nameof(PoisonTempest);
            }
        }
    }
}
