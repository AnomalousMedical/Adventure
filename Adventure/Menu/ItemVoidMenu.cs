using Adventure.Items;
using Adventure.Services;
using Adventure.Skills;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.Menu;

class ItemVoidMenu
(
    Persistence persistence,
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    ILanguageService languageService,
    EquipmentTextService equipmentTextService,
    CharacterStatsTextService characterStatsTextService,
    ConfirmMenu confirmMenu,
    PickUpTreasureMenu pickUpTreasureMenu,
    CharacterMenuPositionService characterMenuPositionService,
    CameraMover cameraMover,
    IObjectResolverFactory objectResolverFactory,
    IScopedCoroutine coroutine,
    ISoundEffectPlayer soundEffectPlayer,
    IClockService clockService
) : IExplorationSubMenu, IDisposable
{
    private ButtonColumn itemButtons = new ButtonColumn(25);
    private SharpButton close = new SharpButton() { Text = "Close" };
    private List<SharpText> infos = null;
    private List<SharpText> descriptions = null;
    private List<ButtonColumnItem<InventoryItem>> currentItems;

    private TaskCompletionSource menuClosed;
    private IObjectResolver objectResolver = objectResolverFactory.Create();
    private ISkillEffect currentEffect;
    private bool gatherTreasure = false;
    private SharpPanel descriptionPanel = new SharpPanel();
    private SharpPanel infoPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.UITransparentBg };

    public IExplorationSubMenu Previous { get; set; }

    public void Dispose()
    {
        objectResolver.Dispose();
    }

    public Task WaitForClose()
    {
        if (menuClosed == null)
        {
            menuClosed = new TaskCompletionSource();
        }
        return menuClosed.Task;
    }

    public void Update(IExplorationMenu menu, GamepadId gamepad)
    {
        if (currentEffect != null)
        {
            currentEffect.Update(clockService.Clock);
            if (currentEffect.Finished)
            {
                currentEffect = null;
            }
            return;
        }

        if (gatherTreasure)
        {
            if (pickUpTreasureMenu.Update(gamepad, menu, this)) //These are separate on purpose
            {
                //If the player rejects the item the pick up treasure menu will return it to the item void
                //Nothing is needed here to recover it
                Close(menu, gamepad);
            }
        }
        else
        {
            if (infos == null)
            {
                infos = characterStatsTextService.GetVitalStats(persistence.Current.Party.Members).ToList();
            }

            if (descriptions == null)
            {
                if (currentItems != null)
                {
                    var descriptionIndex = itemButtons.FocusedIndex(sharpGui);
                    if (descriptionIndex < currentItems.Count)
                    {
                        var item = currentItems[descriptionIndex];
                        var description = new SharpText() { Color = Color.UIWhite };
                        description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(item.Item.InfoId), scaleHelper.Scaled(520), sharpGui);

                        descriptions = new List<SharpText>();
                        descriptions.Add(description);
                        if (item.Item.Equipment != null)
                        {
                            descriptions.Add(new SharpText(" \n") { Color = Color.UIWhite });
                            descriptions.AddRange(equipmentTextService.BuildEquipmentText(item.Item));
                        }
                    }
                }
            }

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new PanelLayout(infoPanel,
               new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            var descriptionLayout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new PanelLayout(descriptionPanel,
               new ColumnLayout(descriptions?.Select(i => new KeepWidthRightLayout(i)) ?? Enumerable.Empty<ILayoutItem>())
               {
                   Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
               }
            ));

            layout = new RowLayout(close) { Margin = new IntPad(scaleHelper.Scaled(10)) };
            var backButtonRect = screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui));
            layout.SetRect(backButtonRect);

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = backButtonRect.Top;
            itemButtons.ScrollBarWidth = scaleHelper.Scaled(25);
            itemButtons.ScrollMargin = scaleHelper.Scaled(5);

            var currentInfos = infos;
            var currentDescriptions = descriptions;

            if (currentItems == null)
            {
                currentItems = persistence.Current.ItemVoid.Select(i => new ButtonColumnItem<InventoryItem>(languageService.Current.Items.GetText(i.InfoId), i)).ToList();
            }
            var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
            var newSelection = itemButtons.Show(sharpGui, currentItems, currentItems.Count, p => screenPositioner.GetTopRightRect(p), gamepad, navLeft: close.Id, navRight: close.Id, wrapLayout: l => new RowLayout(new KeepHeightLayout(descriptionLayout), l) { Margin = new IntPad(scaleHelper.Scaled(10)) }, navUp: close.Id, navDown: close.Id);
            if (lastItemIndex != itemButtons.FocusedIndex(sharpGui))
            {
                descriptions = null;
                infos = null;
            }
            if (newSelection != null)
            {
                coroutine.RunTask(async () =>
                {
                    if (await confirmMenu.ShowAndWait($"Do you want to recover {languageService.Current.Items.GetText(newSelection.InfoId)}?", this, gamepad))
                    {
                        //The item should only be given to the player if it is removed
                        //The pick up menu will return the item to the void, so it does not need special tracking
                        if (persistence.Current.ItemVoid.Remove(newSelection))
                        {
                            gatherTreasure = true;
                            pickUpTreasureMenu.GatherTreasures(new[] { new Treasure(newSelection, TreasureType.Weapon) }, TimeSpan.Zero,
                                (ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState) =>
                                {
                                    currentEffect = treasure.Use(inventory, user, inventoryFunctions, gameState, characterMenuPositionService, objectResolver, coroutine, cameraMover, soundEffectPlayer);
                                });
                        }
                    }
                });
            }

            var hasItems = currentItems.Any();

            if (sharpGui.Button(close, gamepad, navUp: hasItems ? itemButtons.BottomButton : close.Id, navDown: hasItems ? itemButtons.TopButton : close.Id, navLeft: itemButtons.TopButton, navRight: itemButtons.TopButton) || sharpGui.IsStandardBackPressed(gamepad))
            {
                Close(menu, gamepad);
            }

            if (currentInfos != null)
            {
                sharpGui.Panel(infoPanel, panelStyle);
                foreach (var info in currentInfos)
                {
                    sharpGui.Text(info);
                }
            }

            if (currentDescriptions != null)
            {
                sharpGui.Panel(descriptionPanel, panelStyle);
                foreach (var description in currentDescriptions)
                {
                    sharpGui.Text(description);
                }
            }
        }
    }

    private void Close(IExplorationMenu menu, GamepadId gamepad)
    {
        gatherTreasure = false;
        currentItems = null;
        descriptions = null;
        infos = null;
        var oldMenuClosed = menuClosed;
        menuClosed = null;
        menu.RequestSubMenu(Previous, gamepad);
        Previous = null;
        oldMenuClosed?.SetResult();
    }
}
