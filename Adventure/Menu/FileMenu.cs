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
    FadeScreenMenu fadeScreenMenu,
    IBackgroundMusicPlayer backgroundMusicPlayer
) : IExplorationSubMenu
{
    public const float LoadButtonsLayer = 0.15f;

    private readonly SharpButton newGame = new SharpButton() { Text = "New" };
    private readonly SharpButton load = new SharpButton() { Text = "Load" };
    private readonly SharpButton delete = new SharpButton() { Text = "Delete" };
    private readonly SharpButton back = new SharpButton() { Text = "Back" };
    private ButtonColumn saveFileButtons = new ButtonColumn(25, LoadButtonsLayer);

    record SaveInfo(string FileName, DateTime SaveTime, String Description);
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
            saveFileButtons.ScrollBarWidth = scaleHelper.Scaled(25);
            saveFileButtons.ScrollMargin = scaleHelper.Scaled(5);

            var newSelection = saveFileButtons.Show(sharpGui, saveFiles, saveFiles.Count, 
                p => screenPositioner.GetTopRightRect(p), 
                gamepadId,
                wrapLayout: l => new MarginLayout(new IntPad(0, scaleHelper.Scaled(10), scaleHelper.Scaled(20), 0), l),
                navUp: back.Id,
                navDown: back.Id);
            if (newSelection != null)
            {
                SaveSelectedAction(newSelection, menu, gamepadId);
            }

            if (sharpGui.Button(back, gamepadId, navUp: saveFileButtons.BottomButton, navDown: saveFileButtons.TopButton)
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
                if (await confirmMenu.ShowAndWait("Are you sure you want to start a new game?", this, gamepadId))
                {
                    persistenceWriter.Save();
                    options.CurrentSave = persistenceWriter.CreateSaveFileName();
                    backgroundMusicPlayer.SetBackgroundSong(null);

                    await fadeScreenMenu.ShowAndWait(0.0f, 1.0f, 0.6f, GamepadId.Pad1);

                    gameStateRequestor.RequestGameState(resetGameState);
                }
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
            if (await confirmMenu.ShowAndWait($"Are you sure you want to load this save?\n \n{newSelection.Description}", this, gamepadId))
            {
                persistenceWriter.Save();
                options.CurrentSave = newSelection.FileName;
                backgroundMusicPlayer.SetBackgroundSong(null);

                await fadeScreenMenu.ShowAndWait(0.0f, 1.0f, 0.6f, GamepadId.Pad1);

                gameStateRequestor.RequestGameState(resetGameState);
                saveFiles = null;
            }
        });
    }

    private void DeleteSave(SaveInfo saveInfo, IExplorationMenu menu, GamepadId gamepadId)
    {
        coroutineRunner.RunTask(async () =>
        {
            if (await confirmMenu.ShowAndWait($"Are you sure you want to delete this save?\n \n{saveInfo.Description}", this, gamepadId))
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
                var message = GetName(save);

                if (save.GameState.Time.ClearTime != null)
                {
                    time = TimeSpan.FromMilliseconds(save.GameState.Time.ClearTime.Value * Clock.MicroToMilliseconds);
                    message += $"\nGame Cleared: {(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";
                }
                else
                {
                    message += $"\nAreas Cleared: {save.GameState.World.CompletedAreaLevels.Count}";
                }

                message += $"\nTime: {timeText}";

                message += $"\nSaved: {save.GameState.SaveTime}";

                var buttonColumnItem = new ButtonColumnItem<SaveInfo>(message, new SaveInfo(save.FileName, save.GameState.SaveTime, message));

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

    private static string GetName(SaveDataInfo save)
    {
        String message;
        var firstCharacter = save.GameState.Party.Members.FirstOrDefault();
        if (firstCharacter != null)
        {
            message = $"{firstCharacter.CharacterSheet?.Name}";

            var prefix = ": ";
            if (save.GameState.Party.Undefeated)
            {
                message += $"{prefix}Undefeated";
                prefix = ", ";
            }

            if (save.GameState.Party.OldSchool)
            {
                message += $"{prefix}Old School";
            }
        }
        else
        {
            message = "No Party";
        }

        return message;
    }
}
