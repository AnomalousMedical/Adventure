﻿using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu
{
   interface IExplorationSubMenu
    {
        void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId);
    }
}
