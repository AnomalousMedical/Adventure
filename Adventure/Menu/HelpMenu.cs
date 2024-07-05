using Adventure.Assets.SoundEffects;
using Adventure.Items;
using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using SoundPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

internal class HelpMenu
(
    IScaleHelper scaleHelper,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    ILanguageService languageService,
    ICoroutineRunner coroutine,
    TextDialog textDialog,
    Persistence persistence
) : IExplorationSubMenu
{
    private ButtonColumn chapters = new ButtonColumn(8);

    private readonly SharpButton back = new SharpButton() { Text = "Back" };

    private TaskCompletionSource currentTask;

    public Task WaitForCurrentInput()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource();
        }
        return currentTask.Task;
    }

    public IExplorationSubMenu PreviousMenu { get; set; }

    private List<ButtonColumnItem<int?>> chapterButtons = new List<ButtonColumnItem<int?>>()
    {
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter1Title, 1),
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter2Title, 2),
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter3Title, 3),
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter4Title, 4),
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter5Title, 5),
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter6Title, 6),
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter7Title, 7),
        new ButtonColumnItem<int?>(languageService.Current.HelpBook.Chapter8Title, 8),
    };

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        chapters.Margin = scaleHelper.Scaled(10);
        chapters.MaxWidth = scaleHelper.Scaled(900);
        chapters.Bottom = screenPositioner.ScreenSize.Height;

        var selectedItem = chapters.Show<int?>(sharpGui, chapterButtons, 8, s => screenPositioner.GetTopRightRect(s), gamepadId, navRight: back.Id, navLeft: back.Id, navDown: back.Id, navUp: back.Id);
        if(selectedItem != null)
        {
            coroutine.RunTask(async () =>
            {
                switch (selectedItem)
                {
                    case 1:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter1, gamepadId);
                        break;
                    case 2:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter2, gamepadId);
                        break;
                    case 3:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter3, gamepadId);
                        break;
                    case 4:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter4, gamepadId);
                        if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter4))
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter4Part2, gamepadId);
                        }
                        else
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter4Part2Missing, gamepadId);
                        }
                        break;
                    case 5:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter5, gamepadId);
                        if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter5))
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter5Part2, gamepadId);
                        }
                        else
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter5Part2Missing, gamepadId);
                        }
                        break;
                    case 6:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter6, gamepadId);
                        if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter6))
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter6Part2, gamepadId);
                        }
                        else
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter6Part2Missing, gamepadId);
                        }
                        break;
                    case 7:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter7, gamepadId);
                        break;
                    case 8:
                        await textDialog.ShowTextAndWait(languageService.Current.HelpBook.Chapter8, gamepadId);
                        break;
                }

                menu.RequestSubMenu(this, gamepadId);
            });
        }

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        if (sharpGui.Button(back, gamepadId, navUp: chapters.BottomButton, navDown: chapters.TopButton, navLeft: chapters.TopButton, navRight: chapters.TopButton) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            menu.RequestSubMenu(PreviousMenu, gamepadId);
            PreviousMenu = null;

            var task = currentTask;
            currentTask = null;
            task?.SetResult();
        }
    }
}
