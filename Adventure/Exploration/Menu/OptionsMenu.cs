using Adventure.Services;
using Anomalous.OSPlatform;
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
    internal class OptionsMenu : IExplorationSubMenu
    {
        private readonly IScaleHelper scaleHelper;
        private readonly Options options;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly NativeOSWindow nativeOSWindow;
        private readonly App app;
        private readonly PlayerMenu playerMenu;

        private readonly SharpButton players = new SharpButton() { Text = "Players" };
        private readonly SharpButton toggleFullscreen = new SharpButton() { Text = "Fullscreen" };
        private readonly SharpButton exitGame = new SharpButton() { Text = "Exit Game" };
        private readonly SharpButton back = new SharpButton() { Text = "Back" };

        private const int NoSelectedCharacter = -1;
        private int selectedCharacter = NoSelectedCharacter;

        public OptionsMenu
        (
            IScaleHelper scaleHelper,
            Options options,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            NativeOSWindow nativeOSWindow,
            App app,
            PlayerMenu playerMenu
        )
        {
            this.scaleHelper = scaleHelper;
            this.options = options;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.nativeOSWindow = nativeOSWindow;
            this.app = app;
            this.playerMenu = playerMenu;
        }

        public IExplorationSubMenu PreviousMenu { get; set; }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
        {
            toggleFullscreen.Text = options.Fullscreen ? "Fullscreen" : "Windowed";

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(players, toggleFullscreen, exitGame, back) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(players, gamepadId, navDown: toggleFullscreen.Id, navUp: back.Id))
            {
                playerMenu.PreviousMenu = this;
                menu.RequestSubMenu(playerMenu, gamepadId);
            }

            if (sharpGui.Button(toggleFullscreen, gamepadId, navUp: players.Id, navDown: exitGame.Id))
            {
                options.Fullscreen = !options.Fullscreen;
                nativeOSWindow.toggleFullscreen();
            }

            if (sharpGui.Button(exitGame, gamepadId, navUp: toggleFullscreen.Id, navDown: back.Id))
            {
                app.Exit();
            }

            if (sharpGui.Button(back, gamepadId, navUp: exitGame.Id, navDown: players.Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                if (selectedCharacter != NoSelectedCharacter)
                {
                    selectedCharacter = NoSelectedCharacter;
                }
                else
                {
                    menu.RequestSubMenu(PreviousMenu, gamepadId);
                }
            }
        }
    }
}
