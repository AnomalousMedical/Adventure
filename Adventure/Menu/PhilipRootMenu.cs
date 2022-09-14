using Adventure.Menu;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu
{
    class PhilipRootMenu : IExplorationSubMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly BuyMenu buyMenu;
        private readonly RestManager restManager;
        SharpButton buy = new SharpButton() { Text = "Buy" };
        SharpButton rest = new SharpButton() { Text = "Rest" };
        SharpButton goodbye = new SharpButton() { Text = "Goodbye" };

        public PhilipRootMenu
        (
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            BuyMenu buyMenu,
            RestManager restManager
        )
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.buyMenu = buyMenu;
            this.restManager = restManager;
            buyMenu.PreviousMenu = this;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu, GamepadId gamepad)
        {
            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(buy, rest, goodbye) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(buy, gamepad, navUp: goodbye.Id, navDown: rest.Id))
            {
                explorationMenu.RequestSubMenu(buyMenu, gamepad);
            }

            if (sharpGui.Button(rest, gamepad, navUp: buy.Id, navDown: goodbye.Id))
            {
                restManager.Rest(explorationGameState);
            }

            if (sharpGui.Button(goodbye, gamepad, navUp: rest.Id, navDown: buy.Id) || sharpGui.IsStandardBackPressed(gamepad))
            {
                explorationMenu.RequestSubMenu(null, gamepad);
            }
        }
    }
}
