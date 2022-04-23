using Engine;
using Engine.Platform;
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
        private readonly SkillMenu skillMenu;
        SharpButton skills = new SharpButton() { Text = "Skills" };
        SharpButton items = new SharpButton() { Text = "Items" };
        SharpButton debug = new SharpButton() { Text = "Debug" };

        public RootMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            ItemMenu itemMenu,
            SkillMenu skillMenu)
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.itemMenu = itemMenu;
            this.skillMenu = skillMenu;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu, GamepadId gamepad)
        {
            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(skills, items, debug) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(skills, navDown: items.Id, navUp: debug.Id))
            {
                explorationMenu.RequestSubMenu(skillMenu, gamepad);
            }
            else if (sharpGui.Button(items, navDown: debug.Id, navUp: skills.Id))
            {
                explorationMenu.RequestSubMenu(itemMenu, gamepad);
            }
            else if (sharpGui.Button(debug, navDown: skills.Id, navUp: items.Id))
            {
                explorationMenu.RequestSubMenu(explorationMenu.DebugGui, gamepad);
            }
            else if (sharpGui.IsStandardBackPressed())
            {
                explorationMenu.RequestSubMenu(null, gamepad);
            }
        }
    }
}
