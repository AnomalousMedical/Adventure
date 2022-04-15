using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class SalamanderFirebrandGreen : SalamanderFirebrand
    {
        public SalamanderFirebrandGreen()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Skin, 0xff1d7617 }
            };
        }
    }
}
