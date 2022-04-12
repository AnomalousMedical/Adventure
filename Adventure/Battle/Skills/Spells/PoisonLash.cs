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
    class PoisonLash : BaseLash
    {
        public PoisonLash()
            : base("Poison Lash", Element.Poison)
        {

        }
    }
}
