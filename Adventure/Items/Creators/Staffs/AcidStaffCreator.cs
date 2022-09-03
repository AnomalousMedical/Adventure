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
    class AcidStaffCreator : BaseStaffCreator
    {
        public AcidStaffCreator(IEquipmentCurve equipmentCurve, INameGenerator nameGenerator)
            :base("Acid", nameof(AcidStaff07), equipmentCurve, nameGenerator)
        {
            
        }

        protected override IEnumerable<String> GetSpells(int level)
        {
            yield return nameof(Acid);
            if(level > SpellLevels.Blast)
            {
                yield return nameof(AcidBlast);
            }
            if (level > SpellLevels.Lash)
            {
                yield return nameof(AcidLash);
            }
            if (level > SpellLevels.Tempest)
            {
                yield return nameof(AcidTempest);
            }
        }
    }
}
