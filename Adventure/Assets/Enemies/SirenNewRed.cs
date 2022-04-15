using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{ 
    class SirenNewRed : SirenNew
    {
        public SirenNewRed()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Shirt, 0xffa30c46 },
                { Pants, 0xffa30c1b }
            };
        }
    }
}
