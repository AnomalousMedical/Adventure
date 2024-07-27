using Adventure.Items;
using Adventure.Services;
using Adventure.Skills;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.Menu;

class UseItemMenu
(
    Persistence persistence,
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    IInventoryFunctions inventoryFunctions,
    ILanguageService languageService,
    CharacterMenuPositionService characterMenuPositionService,
    IObjectResolverFactory objectResolverFactory,
    IScopedCoroutine coroutine,
    CameraMover cameraMover,
    ISoundEffectPlayer soundEffectPlayer,
    IClockService clockService,
    ConfirmMenu confirmMenu
) : IDisposable
{
    IObjectResolver objectResolver = objectResolverFactory.Create();

    SharpButton use = new SharpButton() { Text = "Use", Layer = ItemMenu.UseItemMenuLayer };
    SharpButton transfer = new SharpButton() { Text = "Transfer", Layer = ItemMenu.UseItemMenuLayer };
    SharpButton discard = new SharpButton() { Text = "Discard", Layer = ItemMenu.UseItemMenuLayer };
    SharpButton cancel = new SharpButton() { Text = "Cancel", Layer = ItemMenu.UseItemMenuLayer };
    SharpText itemPrompt = new SharpText() { Color = Color.UIWhite };
    SharpText characterChooserPrompt = new SharpText() { Color = Color.UIWhite };
    SharpText swapItemPrompt = new SharpText() { Color = Color.UIWhite };
    private List<ButtonColumnItem<Action>> characterChoices = null;
    private List<ButtonColumnItem<Action>> swapItemChoices = null;
    private SharpPanel promptPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.UITransparentBg };

    private ButtonColumn characterButtons = new ButtonColumn(5, ItemMenu.ChooseTargetLayer);
    private ButtonColumn replaceButtons = new ButtonColumn(25, ItemMenu.ReplaceButtonsLayer);

    public InventoryItem SelectedItem { get; private set; }
    private IExplorationSubMenu previousMenu;
    private String itemName;
    private ISkillEffect currentEffect;

    public event Action Closed;
    public event Action ItemUsed;
    public event Action IsTransferStatusChanged;

    public void Setup(InventoryItem selectedItem, IExplorationSubMenu previousMenu)
    {
        this.SelectedItem = selectedItem;
        this.previousMenu = previousMenu;
    }

    public void Update(Persistence.CharacterData characterData, GamepadId gamepadId, SharpStyle style, IExplorationSubMenu parentSubMenu, IExplorationMenu menu)
    {
        if (currentEffect != null)
        {
            currentEffect.Update(clockService.Clock);
            if (currentEffect.Finished)
            {
                currentEffect = null;
                if (swapItemChoices == null)
                {
                    Close();
                }
            }
            return;
        }

        if (SelectedItem == null) { return; }

        if (itemPrompt.Text == null)
        {
            itemName = languageService.Current.Items.GetText(SelectedItem.InfoId);
            itemPrompt.Text = "What will you do with your " + itemName + "?";
            swapItemPrompt.Text = "Trade " + itemName + " with...";
        }

        var choosingCharacter = characterChoices != null;
        var replacingItem = swapItemChoices != null;

        if (!choosingCharacter && !replacingItem
           && sharpGui.FocusedItem != transfer.Id
           && sharpGui.FocusedItem != cancel.Id
           && sharpGui.FocusedItem != discard.Id
           && sharpGui.FocusedItem != use.Id)
        {
            sharpGui.StealFocus(use.Id);
        }

        if (choosingCharacter)
        {
            characterButtons.Margin = scaleHelper.Scaled(10);
            characterButtons.MaxWidth = scaleHelper.Scaled(900);
            characterButtons.Bottom = screenPositioner.ScreenSize.Height;
            characterButtons.ScrollBarWidth = scaleHelper.Scaled(25);
            characterButtons.ScrollMargin = scaleHelper.Scaled(5);

            var action = characterButtons.Show(sharpGui, characterChoices, characterChoices.Count, s => screenPositioner.GetCenterTopRect(s), gamepadId, wrapLayout: l => new ColumnLayout(new PanelLayout(promptPanel, new KeepWidthCenterLayout(characterChooserPrompt)), new KeepWidthCenterLayout(l)) { Margin = new IntPad(scaleHelper.Scaled(10)) }, style: style);
            if (action != null)
            {
                action.Invoke();
                characterChoices = null;
                if (swapItemChoices == null && currentEffect == null) //Keep this check after the action invoke, since currentEffect can get set in there
                {
                    Close();
                    return;
                }
            }

            if (sharpGui.IsStandardBackPressed(gamepadId))
            {
                characterChoices = null;
            }

            sharpGui.Panel(promptPanel, panelStyle);
            sharpGui.Text(characterChooserPrompt);
        }
        else
        {

            if (replacingItem)
            {
                replaceButtons.Margin = scaleHelper.Scaled(10);
                replaceButtons.MaxWidth = scaleHelper.Scaled(900);
                replaceButtons.Bottom = screenPositioner.ScreenSize.Height;
                replaceButtons.ScrollBarWidth = scaleHelper.Scaled(25);
                replaceButtons.ScrollMargin = scaleHelper.Scaled(5);

                var swapItem = replaceButtons.Show(sharpGui, swapItemChoices, swapItemChoices.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId, wrapLayout: l => new ColumnLayout(new PanelLayout(promptPanel, new KeepWidthCenterLayout(swapItemPrompt)), new KeepWidthCenterLayout(l)) { Margin = new IntPad(scaleHelper.Scaled(10)) }, style: style);
                if (swapItem != null)
                {
                    swapItem.Invoke();
                    swapItemChoices = null;
                    Close();
                    SwapTarget = null;
                    return;
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    swapItemChoices = null;
                }

                sharpGui.Panel(promptPanel, panelStyle);
                sharpGui.Text(swapItemPrompt);
            }
            else
            {
                var layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(20)),
                   new ColumnLayout(new KeepWidthCenterLayout(new PanelLayout(promptPanel, itemPrompt)), 
                       new KeepWidthCenterLayout(
                           new ColumnLayout(use, transfer, discard, cancel) 
                           { 
                               Margin = new IntPad(scaleHelper.Scaled(10)) 
                           }
                )));

                var desiredSize = layout.GetDesiredSize(sharpGui);
                layout.SetRect(screenPositioner.GetCenterTopRect(desiredSize));

                use.Text = SelectedItem.Equipment != null ? "Equip" : "Use";

                sharpGui.Panel(promptPanel, panelStyle);
                sharpGui.Text(itemPrompt);

                if (sharpGui.Button(use, gamepadId, navUp: cancel.Id, navDown: transfer.Id, style: style))
                {
                    if (!choosingCharacter)
                    {
                        IsTransfer = false;
                        IsTransferStatusChanged?.Invoke();
                        if (SelectedItem.Equipment != null)
                        {
                            inventoryFunctions.Use(SelectedItem, characterData.Inventory, characterData.CharacterSheet, characterData.CharacterSheet);
                            ItemUsed?.Invoke();
                            Close();
                        }
                        else
                        {
                            characterChooserPrompt.Text = "Use the " + itemName + " on...";
                            characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                            {
                                currentEffect = inventoryFunctions.Use(SelectedItem, characterData.Inventory, characterData.CharacterSheet, i.CharacterSheet, characterMenuPositionService, objectResolver, coroutine, cameraMover, soundEffectPlayer);
                                ItemUsed?.Invoke();
                            }))
                            .Append(new ButtonColumnItem<Action>("Cancel", () => { }))
                            .ToList();
                        }
                    }
                }
                if (sharpGui.Button(transfer, gamepadId, navUp: use.Id, navDown: discard.Id, style: style))
                {
                    if (!choosingCharacter)
                    {
                        IsTransfer = true;
                        IsTransferStatusChanged?.Invoke();
                        characterChooserPrompt.Text = "Transfer the " + itemName + " to...";
                        characterChoices = persistence.Current.Party.Members
                            .Where(i => i != characterData)
                            .Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                            {
                                var localSelectedItem = SelectedItem;
                                if (i.HasRoom)
                                {
                                    characterData.RemoveItem(localSelectedItem);
                                    i.Inventory.Items.Add(localSelectedItem);

                                    coroutine.RunTask(async () =>
                                    {
                                        await PromptEquip(localSelectedItem, i, gamepadId);
                                    });
                                }
                                else
                                {
                                    SwapTarget = i;
                                    swapItemChoices = i.Inventory.Items.Select(swapTarget => new ButtonColumnItem<Action>(languageService.Current.Items.GetText(swapTarget.InfoId), () =>
                                    {
                                        characterData.RemoveItem(localSelectedItem);
                                        i.Inventory.Items.Add(localSelectedItem);

                                        i.RemoveItem(swapTarget);
                                        characterData.Inventory.Items.Add(swapTarget);

                                        coroutine.RunTask(async () =>
                                        {
                                            await PromptEquip(localSelectedItem, i, gamepadId);
                                            await PromptEquip(swapTarget, characterData, gamepadId);
                                        });
                                    })).Append(new ButtonColumnItem<Action>("Cancel", () => { })).ToList();
                                }
                            }))
                            .Append(new ButtonColumnItem<Action>("Cancel", () => { }))
                        .ToList();
                    }
                }
                if (sharpGui.Button(discard, gamepadId, navUp: transfer.Id, navDown: cancel.Id, style: style))
                {
                    if (!choosingCharacter)
                    {
                        coroutine.RunTask(async () =>
                        {
                            if(await confirmMenu.ShowAndWait($"Discard the {languageService.Current.Items.GetText(SelectedItem.InfoId)}?", parentSubMenu, gamepadId))
                            {
                                characterData.RemoveItem(SelectedItem);
                                if (SelectedItem.Unique)
                                {
                                    persistence.Current.ItemVoid.Add(SelectedItem);
                                }
                                Close();
                            }
                        });
                    }
                }
                if (sharpGui.Button(cancel, gamepadId, navUp: discard.Id, navDown: use.Id, style: style) || sharpGui.IsStandardBackPressed(gamepadId))
                {
                    if (!choosingCharacter)
                    {
                        Close();
                    }
                }
            }
        }
    }

    private async Task PromptEquip(InventoryItem selectedItem, Persistence.CharacterData charData, GamepadId gamepadId)
    {
        if (selectedItem.Equipment != null)
        {
            if (characterMenuPositionService.TryGetEntry(charData.CharacterSheet, out var entry))
            {
                entry.FaceCamera();
                cameraMover.SetInterpolatedGoalPosition(entry.CameraPosition, entry.CameraRotation);
            }
            if (await confirmMenu.ShowAndWait("Should " + charData.CharacterSheet.Name + " equip the " + languageService.Current.Items.GetText(selectedItem.InfoId), previousMenu, gamepadId))
            {
                inventoryFunctions.Use(selectedItem, charData.Inventory, charData.CharacterSheet, charData.CharacterSheet);
            }
        }
    }

    public bool IsChoosingCharacters => this.SelectedItem != null && this.characterChoices != null;

    public bool HasEffect => this.currentEffect != null;

    public bool IsSwappingItems => this.SelectedItem != null && this.swapItemChoices != null;

    public Persistence.CharacterData SwapTarget { get; set; }

    public bool IsTransfer { get; set; }

    private void Close()
    {
        this.SelectedItem = null;
        itemPrompt.Text = null;
        Closed?.Invoke();
    }

    public void Dispose()
    {
        objectResolver.Dispose();
    }
}

class ItemMenu : IExplorationSubMenu, IDisposable
{
    public const float ItemButtonsLayer = 0.15f;
    public const float UseItemMenuLayer = 0.25f;
    public const float ChooseTargetLayer = 0.35f;
    public const float ReplaceButtonsLayer = 0.45f;

    private readonly Persistence persistence;
    private readonly ISharpGui sharpGui;
    private readonly IScaleHelper scaleHelper;
    private readonly IScreenPositioner screenPositioner;
    private ButtonColumn itemButtons = new ButtonColumn(25, ItemButtonsLayer);
    SharpButton next = new SharpButton() { Text = "Next" };
    SharpButton previous = new SharpButton() { Text = "Previous" };
    SharpButton plotItems = new SharpButton() { Text = "Plot Items" };
    SharpButton skills = new SharpButton() { Text = "Skills" };
    SharpButton close = new SharpButton() { Text = "Close" };
    List<SharpText> infos = null;
    List<SharpText> descriptions = null;
    private int currentSheet;
    private UseItemMenu useItemMenu;
    private readonly ILanguageService languageService;
    private readonly CharacterMenuPositionService characterMenuPositionService;
    private readonly CameraMover cameraMover;
    private readonly EquipmentTextService equipmentTextService;
    private readonly CharacterStatsTextService characterStatsTextService;
    private readonly CharacterStyleService characterStyleService;
    private readonly PlotItemMenu plotItemMenu;
    private SkillMenu skillMenu;
    private List<ButtonColumnItem<InventoryItem>> currentItems;
    private SharpPanel descriptionPanel = new SharpPanel();
    private SharpPanel infoPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.UITransparentBg };
    private SharpText noItems = new SharpText() { Text = "No Items", Color = Color.UIWhite };
    private SharpPanel noItemsPanel = new SharpPanel();

    public ItemMenu
    (
        Persistence persistence,
        ISharpGui sharpGui,
        IScaleHelper scaleHelper,
        IScreenPositioner screenPositioner,
        UseItemMenu useItemMenu,
        ILanguageService languageService,
        CharacterMenuPositionService characterMenuPositionService,
        CameraMover cameraMover,
        EquipmentTextService equipmentTextService,
        CharacterStatsTextService characterStatsTextService,
        CharacterStyleService characterStyleService,
        PlotItemMenu plotItemMenu
    )
    {
        this.persistence = persistence;
        this.sharpGui = sharpGui;
        this.scaleHelper = scaleHelper;
        this.screenPositioner = screenPositioner;
        this.useItemMenu = useItemMenu;
        this.languageService = languageService;
        this.characterMenuPositionService = characterMenuPositionService;
        this.cameraMover = cameraMover;
        this.equipmentTextService = equipmentTextService;
        this.characterStatsTextService = characterStatsTextService;
        this.characterStyleService = characterStyleService;
        this.plotItemMenu = plotItemMenu;
        useItemMenu.Closed += UseItemMenu_Closed;
        useItemMenu.IsTransferStatusChanged += UseItemMenu_IsTransferStatusChanged;
        useItemMenu.ItemUsed += UseItemMenu_ItemUsed;
    }

    public void Link(SkillMenu skillMenu)
    {
        this.skillMenu = skillMenu;
    }

    public void Dispose()
    {
        useItemMenu.Closed -= UseItemMenu_Closed;
        useItemMenu.IsTransferStatusChanged -= UseItemMenu_IsTransferStatusChanged;
        useItemMenu.ItemUsed -= UseItemMenu_ItemUsed;
    }

    public void Update(IExplorationMenu menu, GamepadId gamepad)
    {
        bool allowChanges = useItemMenu.SelectedItem == null;

        if (currentSheet >= persistence.Current.Party.Members.Count)
        {
            currentSheet = 0;
        }
        var characterData = persistence.Current.Party.Members[currentSheet];
        var currentCharacterStyle = characterStyleService.GetCharacterStyle(characterData.StyleIndex);

        if (characterMenuPositionService.TryGetEntry(characterData.CharacterSheet, out var characterMenuPosition))
        {
            cameraMover.SetInterpolatedGoalPosition(characterMenuPosition.CameraPosition, characterMenuPosition.CameraRotation);
            characterMenuPosition.FaceCamera();
        }

        if (infos == null)
        {
            if (useItemMenu.IsChoosingCharacters || useItemMenu.HasEffect)
            {
                if (useItemMenu.IsTransfer)
                {
                    infos = characterStatsTextService.GetInventorySpace(persistence.Current.Party.Members).ToList();
                }
                else
                {
                    infos = characterStatsTextService.GetVitalStats(persistence.Current.Party.Members).ToList();
                }
            }
            else
            {
                infos = characterStatsTextService.GetFullStats(characterData).ToList();
            }
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
                        descriptions.Add(new SharpText(" \nStat Changes") { Color = Color.UIWhite });
                        descriptions.AddRange(equipmentTextService.GetComparisonText(item.Item, characterData));
                    }
                }
            }
        }

        var currentInfos = infos;
        var currentDescriptions = descriptions;

        ILayoutItem layout;

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(20)),
           new PanelLayout(infoPanel,
           new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) } ));
        layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

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
           } ));

        layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), new RowLayout(previous, next, plotItems, skills, close) { Margin = new IntPad(scaleHelper.Scaled(10)) });
        var backButtonRect = screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui));
        layout.SetRect(backButtonRect);

        itemButtons.Margin = scaleHelper.Scaled(10);
        itemButtons.MaxWidth = scaleHelper.Scaled(900);
        itemButtons.Bottom = backButtonRect.Top;
        itemButtons.ScrollBarWidth = scaleHelper.Scaled(25);
        itemButtons.ScrollMargin = scaleHelper.Scaled(5);

        useItemMenu.Update(characterData, gamepad, currentCharacterStyle, this, menu);

        if (allowChanges)
        {
            if (currentItems == null)
            {
                currentItems = characterData.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(languageService.Current.Items.GetText(i.InfoId), i)).ToList();
            }

            var hasItems = characterData.Inventory.Items.Any();

            if (hasItems)
            {
                var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
                var newSelection = itemButtons.Show(sharpGui, currentItems, currentItems.Count, p => screenPositioner.GetTopRightRect(p), gamepad, navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle, wrapLayout: l => new RowLayout(new KeepHeightLayout(descriptionLayout), l) { Margin = new IntPad(0, scaleHelper.Scaled(10), scaleHelper.Scaled(20), 0) }, navUp: close.Id, navDown: close.Id);
                if (lastItemIndex != itemButtons.FocusedIndex(sharpGui))
                {
                    descriptions = null;
                    infos = null;
                }
                useItemMenu.Setup(newSelection, this);
            }
            else
            {
                var noSkillsLayout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                    new PanelLayout(noItemsPanel,
                    new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                    noItems
                )));
                noSkillsLayout.SetRect(screenPositioner.GetTopRightRect(noSkillsLayout.GetDesiredSize(sharpGui)));

                sharpGui.Panel(noItemsPanel, panelStyle);
                sharpGui.Text(noItems);
            }

            if (sharpGui.Button(previous, gamepad, navUp: hasItems ? itemButtons.BottomButton : previous.Id, navDown: hasItems ? itemButtons.TopButton : previous.Id, navLeft: close.Id, navRight: next.Id, style: currentCharacterStyle) || sharpGui.IsStandardPreviousPressed(gamepad))
            {
                if (allowChanges)
                {
                    itemButtons.ListIndex = 0;
                    --currentSheet;
                    if (currentSheet < 0)
                    {
                        currentSheet = persistence.Current.Party.Members.Count - 1;
                    }
                    currentItems = null;
                    descriptions = null;
                    infos = null;
                    itemButtons.FocusTop(sharpGui);
                }
            }
            if (sharpGui.Button(next, gamepad, navUp: hasItems ? itemButtons.BottomButton : next.Id, navDown: hasItems ? itemButtons.TopButton : next.Id, navLeft: previous.Id, navRight: plotItems.Id, style: currentCharacterStyle) || sharpGui.IsStandardNextPressed(gamepad))
            {
                if (allowChanges)
                {
                    itemButtons.ListIndex = 0;
                    ++currentSheet;
                    if (currentSheet >= persistence.Current.Party.Members.Count)
                    {
                        currentSheet = 0;
                    }
                    currentItems = null;
                    descriptions = null;
                    infos = null;
                    itemButtons.FocusTop(sharpGui);
                }
            }
            if (sharpGui.Button(plotItems, gamepad, navUp: hasItems ? itemButtons.BottomButton : close.Id, navDown: hasItems ? itemButtons.TopButton : close.Id, navLeft: next.Id, navRight: skills.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepad))
            {
                if (allowChanges)
                {
                    currentItems = null;
                    descriptions = null;
                    infos = null;
                    plotItemMenu.PreviousMenu = this;
                    menu.RequestSubMenu(plotItemMenu, gamepad);
                }
            }
            if (sharpGui.Button(skills, gamepad, navUp: hasItems ? itemButtons.BottomButton : close.Id, navDown: hasItems ? itemButtons.TopButton : close.Id, navLeft: plotItems.Id, navRight: close.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepad))
            {
                if (allowChanges)
                {
                    currentItems = null;
                    descriptions = null;
                    infos = null;
                    plotItemMenu.PreviousMenu = this;
                    menu.RequestSubMenu(skillMenu, gamepad);
                }
            }
            if (sharpGui.Button(close, gamepad, navUp: hasItems ? itemButtons.BottomButton : close.Id, navDown: hasItems ? itemButtons.TopButton : close.Id, navLeft: skills.Id, navRight: previous.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepad))
            {
                if (allowChanges)
                {
                    currentItems = null;
                    descriptions = null;
                    infos = null;
                    menu.RequestSubMenu(menu.RootMenu, gamepad);
                }
            }
        }
        else
        {
            descriptionLayout.SetRect(screenPositioner.GetTopRightRect(descriptionLayout.GetDesiredSize(sharpGui)));
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

    private void UseItemMenu_ItemUsed()
    {
        descriptions = null;
        infos = null;
    }

    private void UseItemMenu_Closed()
    {
        descriptions = null;
        infos = null;
        currentItems = null;
    }

    private void UseItemMenu_IsTransferStatusChanged()
    {
        infos = null;
    }
}
