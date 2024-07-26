using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu;

class PlotItemMenu
(
    Persistence persistence,
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    ILanguageService languageService
): IExplorationSubMenu
{
    private ButtonColumn itemButtons = new ButtonColumn(25);
    SharpButton close = new SharpButton() { Text = "Close" };
    List<SharpText> descriptions = null;
    private List<ButtonColumnItem<PlotItems>> currentItems;
    private SharpPanel descriptionPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.UITransparentBg };

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        if (descriptions == null)
        {
            if (currentItems != null)
            {
                var descriptionIndex = itemButtons.FocusedIndex(sharpGui);
                if (descriptionIndex < currentItems.Count)
                {
                    var item = currentItems[descriptionIndex];
                    var description = new SharpText() { Color = Color.UIWhite };
                    description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.PlotItems.GetDescription(item.Item), scaleHelper.Scaled(520), sharpGui);

                    descriptions = new List<SharpText>();
                    descriptions.Add(description);
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

        var layout = new RowLayout(close) { Margin = new IntPad(scaleHelper.Scaled(20)) };
        var backButtonRect = screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui));
        layout.SetRect(backButtonRect);

        itemButtons.Margin = scaleHelper.Scaled(10);
        itemButtons.MaxWidth = scaleHelper.Scaled(900);
        itemButtons.Bottom = backButtonRect.Top;
        itemButtons.ScrollBarWidth = scaleHelper.Scaled(25);
        itemButtons.ScrollMargin = scaleHelper.Scaled(5);

        if (currentItems == null)
        {
            currentItems = persistence.Current.PlotItems.Select(i => new ButtonColumnItem<PlotItems>(languageService.Current.PlotItems.GetText(i), i)).ToList();
        }
        var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
        itemButtons.Show(sharpGui, currentItems, currentItems.Count, 
            p => screenPositioner.GetTopRightRect(p), 
            gamepadId, 
            wrapLayout: l => new RowLayout(new KeepHeightLayout(descriptionLayout), l) { Margin = new IntPad(0, scaleHelper.Scaled(10), scaleHelper.Scaled(20), 0) }, 
            navUp: close.Id, navDown: close.Id);

        if (sharpGui.Button(close, gamepadId, navUp: itemButtons.BottomButton, navDown: itemButtons.TopButton) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            currentItems = null;
            descriptions = null;
            menu.RequestSubMenu(PreviousMenu, gamepadId);
        }

        if (descriptions != null)
        {
            sharpGui.Panel(descriptionPanel, panelStyle);
            foreach (var description in descriptions)
            {
                sharpGui.Text(description);
            }
        }

        if(lastItemIndex != itemButtons.FocusedIndex(sharpGui))
        {
            descriptions = null;
        }
    }
}
