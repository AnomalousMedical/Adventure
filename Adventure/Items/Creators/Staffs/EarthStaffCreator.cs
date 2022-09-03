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
            if(level > SpellLevels.Blast)
            {
                yield return nameof(EarthBlast);
            }
            if (level > SpellLevels.Lash)
            {
                yield return nameof(EarthLash);
            }
            if (level > SpellLevels.Tempest)
            {
                yield return nameof(EarthTempest);
            }
        }
    }
}
