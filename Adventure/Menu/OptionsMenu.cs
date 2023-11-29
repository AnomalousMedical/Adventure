using Adventure.Services;
using Anomalous.OSPlatform;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu
{
    internal class OptionsMenu : IExplorationSubMenu
    {
        public const float LoadButtonsLayer = 0.15f;

        private readonly IScaleHelper scaleHelper;
        private readonly GameOptions options;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly NativeOSWindow nativeOSWindow;
        private readonly App app;
        private readonly PlayerMenu playerMenu;
        private readonly IGameStateRequestor gameStateRequestor;
        private readonly ISetupGameState setupGameState;
        private readonly IPersistenceWriter persistenceWriter;
        private readonly SharpButton players = new SharpButton() { Text = "Players" };
        private readonly SharpButton toggleFullscreen = new SharpButton() { Text = "Fullscreen" };
        private readonly SharpButton load = new SharpButton() { Text = "Load" };
        private readonly SharpButton newGame = new SharpButton() { Text = "New Game" };
        private readonly SharpButton exitGame = new SharpButton() { Text = "Exit Game" };
        private readonly SharpButton back = new SharpButton() { Text = "Back" };
        private ButtonColumn loadButtons = new ButtonColumn(25, LoadButtonsLayer);

        private const int NoSelectedCharacter = -1;
        private int selectedCharacter = NoSelectedCharacter;
        private List<ButtonColumnItem<String>> saveFiles = null;

        public OptionsMenu
        (
            IScaleHelper scaleHelper,
            GameOptions options,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            NativeOSWindow nativeOSWindow,
            App app,
            PlayerMenu playerMenu,
            IGameStateRequestor gameStateRequestor,
            ISetupGameState setupGameState,
            IPersistenceWriter persistenceWriter
        )
        {
            this.scaleHelper = scaleHelper;
            this.options = options;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.nativeOSWindow = nativeOSWindow;
            this.app = app;
            this.playerMenu = playerMenu;
            this.gameStateRequestor = gameStateRequestor;
            this.setupGameState = setupGameState;
            this.persistenceWriter = persistenceWriter;
        }

        public IExplorationSubMenu PreviousMenu { get; set; }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
        {
            if (saveFiles != null)
            {
                loadButtons.Margin = scaleHelper.Scaled(10);
                loadButtons.MaxWidth = scaleHelper.Scaled(900);
                loadButtons.Bottom = screenPositioner.ScreenSize.Height;

                var newSelection = loadButtons.Show(sharpGui, saveFiles, saveFiles.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId, navLeft: back.Id, navRight: back.Id);
                if(newSelection != null)
                {
                    persistenceWriter.Save();
                    options.CurrentSave = newSelection;
                    menu.RequestSubMenu(null, gamepadId);
                    gameStateRequestor.RequestGameState(setupGameState);
                    saveFiles = null;
                }

                if (sharpGui.Button(back, gamepadId, navLeft: loadButtons.TopButton, navRight: loadButtons.TopButton) 
                    || sharpGui.IsStandardBackPressed(gamepadId))
                {
                    saveFiles = null;
                }

                return;
            }

            toggleFullscreen.Text = options.Fullscreen ? "Fullscreen" : "Windowed";
            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(players, toggleFullscreen, load, newGame, exitGame, back) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            if (sharpGui.Button(players, gamepadId, navDown: toggleFullscreen.Id, navUp: back.Id))
            {
                playerMenu.PreviousMenu = this;
                menu.RequestSubMenu(playerMenu, gamepadId);
            }

            if (sharpGui.Button(toggleFullscreen, gamepadId, navUp: players.Id, navDown: load.Id))
            {
                options.Fullscreen = !options.Fullscreen;
                nativeOSWindow.toggleFullscreen();
                if (!options.Fullscreen)
                {
                    nativeOSWindow.Maximized = true;
                }
            }

            if(sharpGui.Button(load, gamepadId, navUp: toggleFullscreen.Id, navDown: newGame.Id))
            {
                saveFiles = persistenceWriter.GetSaveFiles().Select(i => new ButtonColumnItem<string>(i, i)).ToList();
            }

            if (sharpGui.Button(newGame, gamepadId, navUp: load.Id, navDown: exitGame.Id))
            {
                persistenceWriter.Save();
                options.CurrentSave = persistenceWriter.CreateSaveFileName();
                gameStateRequestor.RequestGameState(setupGameState);
            }

            if (sharpGui.Button(exitGame, gamepadId, navUp: newGame.Id, navDown: back.Id))
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
