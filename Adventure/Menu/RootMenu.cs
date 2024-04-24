using Adventure.Services;
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
        private readonly OptionsMenu optionsMenu;
        private readonly Persistence persistence;

        SharpButton skills = new SharpButton() { Text = "Skills" };
        SharpButton items = new SharpButton() { Text = "Items" };
        SharpButton options = new SharpButton() { Text = "Options" };
        SharpButton debug = new SharpButton() { Text = "Debug" };

        SharpText undefeated = new SharpText() { Text = "Undefeated", Color = Color.White };
        SharpText oldSchool = new SharpText() { Text = "Old School", Color = Color.White };
        SharpText timePlayed = new SharpText() { Color = Color.White };

        public RootMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            ItemMenu itemMenu,
            SkillMenu skillMenu,
            OptionsMenu optionsMenu,
            Persistence persistence
        )
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.itemMenu = itemMenu;
            this.skillMenu = skillMenu;
            this.optionsMenu = optionsMenu;
            this.persistence = persistence;
        }

        private IEnumerable<SharpButton> GetMenuItems()
        {
            yield return skills;
            yield return items;
            yield return options;
            yield return debug;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu, GamepadId gamepad)
        {
            var time = TimeSpan.FromMilliseconds(persistence.Current.Time.Total * Clock.MicroToMilliseconds);
            timePlayed.Text = $"{(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";

            var infoLayout =
              new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
              new MaxWidthLayout(scaleHelper.Scaled(300),
              new ColumnLayout(undefeated, oldSchool, timePlayed) { Margin = new IntPad(10) }
            ));
            var infoDesiredSize = infoLayout.GetDesiredSize(sharpGui);
            infoLayout.SetRect(screenPositioner.GetBottomLeftRect(infoDesiredSize));

            sharpGui.Text(timePlayed);
            if (persistence.Current.Party.Undefeated)
            {
                sharpGui.Text(undefeated);
            }
            if (persistence.Current.Party.OldSchool)
            {
                sharpGui.Text(oldSchool);
            }

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(GetMenuItems()) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(skills, gamepad, navDown: items.Id, navUp: debug.Id))
            {
                explorationMenu.RequestSubMenu(skillMenu, gamepad);
            }
            else if (sharpGui.Button(items, gamepad, navDown: options.Id, navUp: skills.Id))
            {
                explorationMenu.RequestSubMenu(itemMenu, gamepad);
            }
            else if (sharpGui.Button(options, gamepad, navDown: debug.Id, navUp: items.Id))
            {
                optionsMenu.PreviousMenu = this;
                explorationMenu.RequestSubMenu(optionsMenu, gamepad);
            }
            else if (sharpGui.Button(debug, gamepad, navDown: skills.Id, navUp: options.Id))
            {
                explorationMenu.RequestSubMenu(explorationMenu.DebugGui, gamepad);
            }
            else if (sharpGui.IsStandardBackPressed(gamepad))
            {
                explorationMenu.RequestSubMenu(null, gamepad);
            }
        }
    }
}
