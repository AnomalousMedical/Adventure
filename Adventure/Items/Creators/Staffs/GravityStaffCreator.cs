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
    class GravityStaffCreator : BaseStaffCreator
    {
        public GravityStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Gravity", nameof(GravityStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Gravity);
            if (level > SpellLevels.Blast)
            {
                yield return nameof(GravityBlast);
            }
            if (level > SpellLevels.Lash)
            {
                yield return nameof(GravityLash);
            }
            if (level > SpellLevels.Tempest)
            {
                yield return nameof(GravityTempest);
            }
        }
    }
}
