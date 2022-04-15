using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class OrcKnightOldBronze : OrcKnightOld
    {
        public OrcKnightOldBronze()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Armor, 0xffcb7117 }
            };
        }
    }
}
