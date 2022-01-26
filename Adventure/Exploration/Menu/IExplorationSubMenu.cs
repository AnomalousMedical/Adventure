using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
   interface IExplorationSubMenu
    {
        void Update(IExplorationGameState explorationGameState, IExplorationMenu menu);
    }
}
