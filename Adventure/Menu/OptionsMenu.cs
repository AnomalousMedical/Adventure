﻿using Anomalous.OSPlatform;
using Engine;
using Engine.Platform;
using SharpGui;

namespace Adventure.Menu;

internal class OptionsMenu
(
    IScaleHelper scaleHelper,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    App app,
    PlayerMenu playerMenu,
    CreditsMenu creditsMenu,
    GraphicsOptionsMenu graphicsOptionsMenu
) : IExplorationSubMenu
{
    public const float LoadButtonsLayer = 0.15f;

    private readonly SharpButton players = new SharpButton() { Text = "Players" };
    private readonly SharpButton graphicsOptions = new SharpButton() { Text = "Graphics" };
    private readonly SharpButton exitGame = new SharpButton() { Text = "Exit Game" };
    private readonly SharpButton credits = new SharpButton() { Text = "Credits" };
    private readonly SharpButton back = new SharpButton() { Text = "Back" };

    private const int NoSelectedCharacter = -1;
    private int selectedCharacter = NoSelectedCharacter;

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
    {
        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(players, graphicsOptions, exitGame, credits, back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        if (sharpGui.Button(players, gamepadId, navDown: graphicsOptions.Id, navUp: back.Id))
        {
            playerMenu.PreviousMenu = this;
            menu.RequestSubMenu(playerMenu, gamepadId);
        }

        if (sharpGui.Button(graphicsOptions, gamepadId, navUp: players.Id, navDown: exitGame.Id))
        {
            graphicsOptionsMenu.PreviousMenu = this;
            menu.RequestSubMenu(graphicsOptionsMenu, gamepadId);
        }

        if (sharpGui.Button(exitGame, gamepadId, navUp: graphicsOptions.Id, navDown: credits.Id))
        {
            app.Exit();
        }

        if(sharpGui.Button(credits, gamepadId, navUp: exitGame.Id, navDown: back.Id))
        {
            creditsMenu.Previous = this;
            menu.RequestSubMenu(creditsMenu, gamepadId);
        }

        if (sharpGui.Button(back, gamepadId, navUp: credits.Id, navDown: players.Id) || sharpGui.IsStandardBackPressed(gamepadId))
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
