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
        SharpButton levelUp = new SharpButton() { Text = "Level Up" };

        public AsimovRootMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            LevelUpMenu levelUpMenu)
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.levelUpMenu = levelUpMenu;
            levelUpMenu.PreviousMenu = this;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu)
        {
            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(levelUp) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(levelUp))
            {
                explorationMenu.RequestSubMenu(levelUpMenu);
            }
            else if (sharpGui.IsStandardBackPressed())
            {
                explorationMenu.RequestSubMenu(null);
            }
        }
    }
}
