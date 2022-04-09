using Adventure.Services;
using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    class TreasureMenu : IExplorationSubMenu
    {
        private readonly PickUpTreasureMenu pickUpTreasureMenu;

        public TreasureMenu
        (
            PickUpTreasureMenu pickUpTreasureMenu
        )
        {
            this.pickUpTreasureMenu = pickUpTreasureMenu;
        }

        public void GatherTreasures(IEnumerable<ITreasure> treasure)
        {
            pickUpTreasureMenu.GatherTreasures(treasure);
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            if (pickUpTreasureMenu.Update())
            {
                menu.RequestSubMenu(null);
                return;
            }
        }
    }
}
