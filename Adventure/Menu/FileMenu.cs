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
    ICoroutineRunner coroutineRunner,
    ConfirmMenu confirmMenu,
    FadeScreenMenu fadeScreenMenu
) : IExplorationSubMenu
{
    public const float LoadButtonsLayer = 0.15f;

    private readonly SharpButton newGame = new SharpButton() { Text = "New" };
    private readonly SharpButton load = new SharpButton() { Text = "Load" };
    private readonly SharpButton delete = new SharpButton() { Text = "Delete" };
    private readonly SharpButton back = new SharpButton() { Text = "Back" };
    private ButtonColumn saveFileButtons = new ButtonColumn(25, LoadButtonsLayer);

    record SaveInfo(string FileName, DateTime SaveTime);
    private List<ButtonColumnItem<SaveInfo>> saveFiles = null;

    public IExplorationSubMenu PreviousMenu { get; set; }

    private Action<SaveInfo, IExplorationMenu, GamepadId> SaveSelectedAction;

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        if (saveFiles != null)
        {
            saveFileButtons.Margin = scaleHelper.Scaled(10);
            saveFileButtons.MaxWidth = scaleHelper.Scaled(900);
            saveFileButtons.Bottom = screenPositioner.ScreenSize.Height;

            var newSelection = saveFileButtons.Show(sharpGui, saveFiles, saveFiles.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId, navLeft: back.Id, navRight: back.Id);
            if (newSelection != null)
            {
                SaveSelectedAction(newSelection, menu, gamepadId);
            }

            if (sharpGui.Button(back, gamepadId, navLeft: saveFileButtons.TopButton, navRight: saveFileButtons.TopButton)
                || sharpGui.IsStandardBackPressed(gamepadId))
            {
                saveFiles = null;
            }

            return;
        }

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(newGame, load, delete, back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        if (sharpGui.Button(newGame, gamepadId, navUp: back.Id, navDown: load.Id))
        {
            coroutineRunner.RunTask(async () =>
            {
                persistenceWriter.Save();
                options.CurrentSave = persistenceWriter.CreateSaveFileName();

                await fadeScreenMenu.ShowAndWait(0.0f, 1.0f, 0.6f, GamepadId.Pad1);

                gameStateRequestor.RequestGameState(resetGameState);
            });
        }

        if (sharpGui.Button(load, gamepadId, navUp: newGame.Id, navDown: delete.Id))
        {
            SaveSelectedAction = LoadSave;
            LoadSaveFileInfo();
        }

        if (sharpGui.Button(delete, gamepadId, navUp: load.Id, navDown: back.Id))
        {
            SaveSelectedAction = DeleteSave;
            LoadSaveFileInfo();
        }

        if (sharpGui.Button(back, gamepadId, navUp: delete.Id, navDown: newGame.Id) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            menu.RequestSubMenu(PreviousMenu, gamepadId);
        }
    }

    private void LoadSave(SaveInfo newSelection, IExplorationMenu menu, GamepadId gamepadId)
    {
        coroutineRunner.RunTask(async () =>
        {
            persistenceWriter.Save();
            options.CurrentSave = newSelection.FileName;

            await fadeScreenMenu.ShowAndWait(0.0f, 1.0f, 0.6f, GamepadId.Pad1);

            gameStateRequestor.RequestGameState(resetGameState);
            saveFiles = null;
        });
    }

    private void DeleteSave(SaveInfo saveInfo, IExplorationMenu menu, GamepadId gamepadId)
    {
        coroutineRunner.RunTask(async () =>
        {
            if (await confirmMenu.ShowAndWait($"Are you sure you want to delete the save {saveInfo.SaveTime}?", this, gamepadId))
            {
                if (options.CurrentSave == saveInfo.FileName)
                {
                    options.CurrentSave = null;
                }
                persistenceWriter.DeleteFile(saveInfo.FileName);
                saveFiles = null;
            }
        });
    }

    private void LoadSaveFileInfo()
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

                var time = TimeSpan.FromMilliseconds(save.GameState.Time.Total * Clock.MicroToMilliseconds);
                var timeText = $"{(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";

                var message =
@$"{save.GameState.Party.Members.FirstOrDefault()?.CharacterSheet?.Name}
Areas Cleared: {save.GameState.World.CompletedAreaLevels.Count}
Level: {save.GameState.World.Level}
Time: {timeText}";

                if(save.GameState.Time.ClearTime != null)
                {
                    time = TimeSpan.FromMilliseconds(save.GameState.Time.ClearTime.Value * Clock.MicroToMilliseconds);
                    message += $"\nCleared: {(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";   
                }

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
}
