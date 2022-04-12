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
    class FireTempest : BaseTempest
    {
        public FireTempest()
            : base("Fire Tempest", Element.Fire, 52) //TODO: This could be double too, need to balance mp
        {

        }
    }
}
