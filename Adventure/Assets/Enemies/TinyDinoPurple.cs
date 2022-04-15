using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class TinyDinoPurple : TinyDino
    {
        public TinyDinoPurple()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Skin, 0xff9105bd },
                { Eye, 0xff2ccdca }
            };
        }
    }
}
