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
    class IceStaffCreator : BaseStaffCreator
    {
        public IceStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Ice", equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Ice);
            if(level > 18)
            {
                yield return nameof(IceBlast);
            }
            if (level > 28)
            {
                yield return nameof(IceLash);
            }
            if (level > 38)
            {
                yield return nameof(IceTempest);
            }
        }
    }
}
