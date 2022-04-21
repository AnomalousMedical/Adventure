using Engine;
using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle.Skills
{
    class GravityBlast : BaseBlast
    {
        public GravityBlast()
            :base("Gravity Blast", Element.Gravity)
        {

        }
    }
}
