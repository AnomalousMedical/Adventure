﻿using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{
    class TinyDinoRed : TinyDino
    {
        public TinyDinoRed()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Skin, 0xffc12935 },
                { Spine, 0xff9105bd },
                { Eye, 0xffe28516 },
            };
        }
    }
}
