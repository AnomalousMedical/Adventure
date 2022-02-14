using Adventure.Exploration.Menu;
using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu.Asimov
{
    class AsimovRootMenu : IExplorationSubMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly LevelUpMenu levelUpMenu;
        private readonly BuyMenu buyMenu;
        private readonly RestManager restManager;
        SharpButton levelUp = new SharpButton() { Text = "Level Up" };
        SharpButton buy = new SharpButton() { Text = "Buy" };
        SharpButton rest = new SharpButton() { Text = "Rest" };
        SharpButton goodbye = new SharpButton() { Text = "Goodbye" };

        public AsimovRootMenu
        (
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            LevelUpMenu levelUpMenu,
            BuyMenu buyMenu,
            RestManager restManager
        )
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.levelUpMenu = levelUpMenu;
            this.buyMenu = buyMenu;
            this.restManager = restManager;
            levelUpMenu.PreviousMenu = this;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu)
        {
            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(levelUp, buy, rest, goodbye) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(levelUp, navUp: goodbye.Id, navDown: buy.Id))
            {
                explorationMenu.RequestSubMenu(levelUpMenu);
            }

            if (sharpGui.Button(buy, navUp: levelUp.Id, navDown: rest.Id))
            {
                explorationMenu.RequestSubMenu(buyMenu);
            }

            if (sharpGui.Button(rest, navUp: buy.Id, navDown: goodbye.Id))
            {
                restManager.Rest(explorationGameState);
            }

            if (sharpGui.Button(goodbye, navUp: rest.Id, navDown: levelUp.Id) || sharpGui.IsStandardBackPressed())
            {
                explorationMenu.RequestSubMenu(null);
            }
        }
    }
}
