using Engine;
using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Platform;

namespace Adventure.Battle.Skills
{
    class GravityLash : BaseLash
    {
        public GravityLash()
            : base("Gravity Lash", Element.Gravity)
        {

        }
    }
}
