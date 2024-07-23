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
    Persistence persistence
) : IExplorationSubMenu
{
    private ButtonColumn chapters = new ButtonColumn(8);

    private readonly SharpButton back = new SharpButton() { Text = "Close" };
    private SharpPanel descriptionPanel = new SharpPanel();
    List<SharpText> descriptions = null;

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

    private void SetDescriptionText(SharpText description, string text)
    {
        description.Text = MultiLineTextBuilder.CreateMultiLineString(text, scaleHelper.Scaled(520), sharpGui);
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        if (descriptions == null)
        {
            if (chapterButtons != null)
            {
                var descriptionIndex = chapters.FocusedIndex(sharpGui);
                if (descriptionIndex < chapterButtons.Count)
                {
                    var item = chapterButtons[descriptionIndex];
                    var description = new SharpText() { Color = Color.UIWhite };

                    descriptions = new List<SharpText>();
                    descriptions.Add(description);

                    switch (item.Item)
                    {
                        case 1:
                            SetDescriptionText(description, languageService.Current.HelpBook.Chapter1);
                            break;
                        case 2:
                            SetDescriptionText(description, languageService.Current.HelpBook.Chapter2);
                            break;
                        case 3:
                            SetDescriptionText(description, languageService.Current.HelpBook.Chapter3);
                            break;
                        case 4:
                            if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter4))
                            {
                                SetDescriptionText(description, languageService.Current.HelpBook.Chapter4 + "\n \n" + languageService.Current.HelpBook.Chapter4Part2);
                            }
                            else
                            {
                                SetDescriptionText(description, languageService.Current.HelpBook.Chapter4 + "\n \n" + languageService.Current.HelpBook.Chapter4Part2Missing);
                            }
                            break;
                        case 5:
                            if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter5))
                            {
                                SetDescriptionText(description, languageService.Current.HelpBook.Chapter5 + "\n \n" + languageService.Current.HelpBook.Chapter5Part2);
                            }
                            else
                            {
                                SetDescriptionText(description, languageService.Current.HelpBook.Chapter5 + "\n \n" + languageService.Current.HelpBook.Chapter5Part2Missing);
                            }
                            break;
                        case 6:
                            if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter6))
                            {
                                SetDescriptionText(description, languageService.Current.HelpBook.Chapter6 + "\n \n" + languageService.Current.HelpBook.Chapter6Part2);
                            }
                            else
                            {
                                SetDescriptionText(description, languageService.Current.HelpBook.Chapter6 + "\n \n" + languageService.Current.HelpBook.Chapter6Part2Missing);
                            }
                            break;
                        case 7:
                            SetDescriptionText(description, languageService.Current.HelpBook.Chapter7);
                            break;
                        case 8:
                            SetDescriptionText(description, languageService.Current.HelpBook.Chapter8);
                            break;
                    }
                }
            }
        }

        IEnumerable<ILayoutItem> columnItems = Enumerable.Empty<ILayoutItem>();
        if (descriptions != null)
        {
            columnItems = columnItems.Concat(descriptions.Select(i => new KeepWidthRightLayout(i)));
        }

        var descriptionLayout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new PanelLayout(descriptionPanel,
           new ColumnLayout(columnItems)
           {
               Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
           }
        ));

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        var backButtonRect = screenPositioner.GetBottomRightRect(desiredSize);
        layout.SetRect(backButtonRect);

        chapters.Margin = scaleHelper.Scaled(10);
        chapters.MaxWidth = scaleHelper.Scaled(900);
        chapters.Bottom = backButtonRect.Top;
        chapters.ScrollBarWidth = scaleHelper.Scaled(25);
        chapters.ScrollMargin = scaleHelper.Scaled(5);

        var lastItemIndex = chapters.FocusedIndex(sharpGui);
        var selectedItem = chapters.Show<int?>(sharpGui, chapterButtons, 8, s => screenPositioner.GetTopRightRect(s), gamepadId, navDown: back.Id, navUp: back.Id, wrapLayout: l => new RowLayout(new KeepHeightLayout(descriptionLayout), l) { Margin = new IntPad(scaleHelper.Scaled(10)) });

        if (sharpGui.Button(back, gamepadId, navUp: chapters.BottomButton, navDown: chapters.TopButton, navLeft: chapters.TopButton, navRight: chapters.TopButton) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            menu.RequestSubMenu(PreviousMenu, gamepadId);
            PreviousMenu = null;

            var task = currentTask;
            currentTask = null;
            task?.SetResult();
        }

        if (descriptions != null)
        {
            sharpGui.Panel(descriptionPanel);
            foreach (var description in descriptions)
            {
                sharpGui.Text(description);
            }
        }

        if (lastItemIndex != chapters.FocusedIndex(sharpGui))
        {
            descriptions = null;
        }
    }
}
