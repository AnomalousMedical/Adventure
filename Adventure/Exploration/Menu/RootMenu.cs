using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    interface IRootMenu : IExplorationSubMenu
    {

    }

    class RootMenu : IRootMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly ItemMenu itemMenu;
        SharpButton debug = new SharpButton() { Text = "Debug" };
        SharpButton items = new SharpButton() { Text = "Items" };

        public RootMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            ItemMenu itemMenu)
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.itemMenu = itemMenu;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu)
        {
            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(items, debug) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(items, navDown: debug.Id, navUp: debug.Id))
            {
                explorationMenu.RequestSubMenu(itemMenu);
            }
            else if (sharpGui.Button(debug, navDown: items.Id, navUp: items.Id))
            {
                explorationMenu.RequestSubMenu(explorationMenu.DebugGui);
            }
            else if (sharpGui.IsStandardBackPressed())
            {
                explorationMenu.RequestSubMenu(null);
            }
        }
    }
}
