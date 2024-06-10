using Adventure.Services;
using Anomalous.OSPlatform;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu;

internal class FileMenu
(
    IScaleHelper scaleHelper,
    GameOptions options,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    IGameStateRequestor gameStateRequestor,
    IPersistenceWriter persistenceWriter,
    IResetGameState resetGameState,
    ICoroutineRunner coroutineRunner
) : IExplorationSubMenu
{
    public const float LoadButtonsLayer = 0.15f;

    private readonly SharpButton load = new SharpButton() { Text = "Load" };
    private readonly SharpButton newGame = new SharpButton() { Text = "New Game" };
    private readonly SharpButton back = new SharpButton() { Text = "Back" };
    private ButtonColumn loadButtons = new ButtonColumn(25, LoadButtonsLayer);

    record SaveInfo(string FileName, DateTime SaveTime);
    private List<ButtonColumnItem<SaveInfo>> saveFiles = null;

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
    {
        if (saveFiles != null)
        {
            loadButtons.Margin = scaleHelper.Scaled(10);
            loadButtons.MaxWidth = scaleHelper.Scaled(900);
            loadButtons.Bottom = screenPositioner.ScreenSize.Height;

            var newSelection = loadButtons.Show(sharpGui, saveFiles, saveFiles.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId, navLeft: back.Id, navRight: back.Id);
            if (newSelection != null)
            {
                persistenceWriter.Save();
                options.CurrentSave = newSelection.FileName;
                menu.RequestSubMenu(null, gamepadId);
                gameStateRequestor.RequestGameState(resetGameState);
                saveFiles = null;
                menu.RequestSubMenu(null, gamepadId);
            }

            if (sharpGui.Button(back, gamepadId, navLeft: loadButtons.TopButton, navRight: loadButtons.TopButton)
                || sharpGui.IsStandardBackPressed(gamepadId))
            {
                saveFiles = null;
            }

            return;
        }

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(load, newGame, back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        if (sharpGui.Button(load, gamepadId, navUp: back.Id, navDown: newGame.Id))
        {
            saveFiles = new List<ButtonColumnItem<SaveInfo>>();
            var mySaveFiles = saveFiles;
            coroutineRunner.RunTask(async () =>
            {
                await foreach (var save in persistenceWriter.GetAllSaveData())
                {
                    if (saveFiles != mySaveFiles)
                    {
                        break;
                    }

                    var message =
@$"{save.GameState.Party.Members.FirstOrDefault()?.CharacterSheet?.Name}
Areas Cleared: {save.GameState.World.CompletedAreaLevels.Count}
Level: {save.GameState.World.Level}
Time: {TimeSpan.FromMicroseconds(save.GameState.Time.Total)}";

                    if (save.GameState.Party.Undefeated)
                    {
                        message += "\nUndefeated";
                    }

                    if (save.GameState.Party.OldSchool)
                    {
                        message += "\nOld School";
                    }

                    message += $"\nSaved: {save.GameState.SaveTime}";

                    var buttonColumnItem = new ButtonColumnItem<SaveInfo>(message, new SaveInfo(save.FileName, save.GameState.SaveTime));

                    if (mySaveFiles.Count == 0)
                    {
                        mySaveFiles.Add(buttonColumnItem);
                    }
                    else
                    {
                        var i = 0;
                        for (; i < mySaveFiles.Count && save.GameState.SaveTime < mySaveFiles[i].Item.SaveTime; i++) { }
                        mySaveFiles.Insert(i, buttonColumnItem);
                    }
                }
            });
        }

        if (sharpGui.Button(newGame, gamepadId, navUp: load.Id, navDown: back.Id))
        {
            persistenceWriter.Save();
            options.CurrentSave = persistenceWriter.CreateSaveFileName();
            gameStateRequestor.RequestGameState(resetGameState);
            menu.RequestSubMenu(null, gamepadId);
        }

        if (sharpGui.Button(back, gamepadId, navUp: newGame.Id, navDown: load.Id) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            menu.RequestSubMenu(PreviousMenu, gamepadId);
        }
    }
}
