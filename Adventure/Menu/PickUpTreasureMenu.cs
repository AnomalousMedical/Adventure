using Adventure.Items;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu;

class PickUpTreasureMenu
(
    Persistence persistence,
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    IPersistenceWriter persistenceWriter,
    IInventoryFunctions inventoryFunctions,
    ILanguageService languageService,
    EquipmentTextService equipmentTextService,
    CharacterStatsTextService characterStatsTextService,
    CharacterStyleService characterStyleService,
    ConfirmMenu confirmMenu,
    IScopedCoroutine scopedCoroutine
)
{
    public const float ChooseTargetLayer = 0.15f;
    public const float ReplaceButtonsLayer = 0.45f;
    private static readonly InventoryItem CancelInventoryItem = new InventoryItem();

    private enum SaveBlocker { Treasure }

    private Stack<ITreasure> currentTreasure;
    SharpButton take = new SharpButton() { Text = "Take" };
    SharpButton use = new SharpButton() { Text = "Use" };
    SharpButton equip = new SharpButton() { Text = "Yes" };
    SharpButton continueButton = new SharpButton() { Text = "No" };
    SharpButton discard = new SharpButton() { Text = "Discard" };
    SharpButton next = new SharpButton() { Text = "Next" };
    SharpButton previous = new SharpButton() { Text = "Previous" };
    SharpText itemInfo = new SharpText() { Color = Color.UIWhite };
    SharpText currentCharacter = new SharpText() { Color = Color.UIWhite };
    SharpText inventoryInfo = new SharpText() { Color = Color.UIWhite };
    List<SharpText> infos;
    List<SharpText> descriptions;
    SharpText promptText = new SharpText() { Color = Color.UIWhite };
    private int currentSheet;
    private bool replacingItem = false;
    private bool equippingItem = false;
    private DateTime allowPickupTime;
    private List<ButtonColumnItem<Action>> characterChoices = null;
    private SharpPanel descriptionPanel = new SharpPanel();
    private SharpPanel infoPanel = new SharpPanel();
    private SharpPanel characterPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

    private ButtonColumn replaceButtons = new ButtonColumn(25, ReplaceButtonsLayer);
    private ButtonColumn characterButtons = new ButtonColumn(5, ChooseTargetLayer);

    public delegate void UseCallback(ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState);
    private UseCallback useCallback;
    private Action<Persistence.CharacterData> activeCharacterChanged;
    private Action<ITreasure> treasureGivenCallback;

    private bool layoutNavigationRight = true;

    public void GatherTreasures(IEnumerable<ITreasure> treasure, TimeSpan pickupDelay, UseCallback useCallback, Action<Persistence.CharacterData> activeCharacterChanged = null, bool layoutNavigationRight = true, Action<ITreasure> treasureGivenCallback = null)
    {
        this.treasureGivenCallback = treasureGivenCallback;
        this.layoutNavigationRight = layoutNavigationRight;
        this.useCallback = useCallback;
        this.activeCharacterChanged = activeCharacterChanged;
        currentTreasure = new Stack<ITreasure>(treasure);
        persistenceWriter.AddSaveBlock(SaveBlocker.Treasure);
        replacingItem = false;
        allowPickupTime = DateTime.Now + pickupDelay;

        FireActiveCharacterChanged();
    }

    public bool Update(GamepadId gamepadId, IExplorationMenu menu, IExplorationSubMenu parentSubMenu)
    {
        if (currentSheet > persistence.Current.Party.Members.Count)
        {
            currentSheet = 0;
        }
        var sheet = persistence.Current.Party.Members[currentSheet];
        var currentCharacterStyle = characterStyleService.GetCharacterStyle(sheet.StyleIndex);

        //Keep this block first so it exits in the section below
        var choosingCharacter = characterChoices != null;
        if (choosingCharacter)
        {
            characterButtons.StealFocus(sharpGui);

            characterButtons.Margin = scaleHelper.Scaled(10);
            characterButtons.MaxWidth = scaleHelper.Scaled(900);
            characterButtons.Bottom = screenPositioner.ScreenSize.Height;
            var action = characterButtons.Show(sharpGui, characterChoices, characterChoices.Count, s => screenPositioner.GetCenterRect(s), gamepadId, style: currentCharacterStyle);
            if (action != null)
            {
                action.Invoke();
                characterChoices = null;
            }

            if (sharpGui.IsStandardBackPressed(gamepadId))
            {
                characterChoices = null;
            }
        }

        if (currentTreasure == null || currentTreasure.Count == 0)
        {
            persistenceWriter.RemoveSaveBlock(SaveBlocker.Treasure);
            persistenceWriter.Save();
            useCallback = null;
            return true;
        }

        currentCharacter.Text = sheet.CharacterSheet.Name;
        inventoryInfo.Text = equippingItem ? "Equip" : $"Items: {sheet.Inventory.Items.Count} / {sheet.CharacterSheet.InventorySize}";

        var treasure = currentTreasure.Peek();

        if(infos == null)
        {
            if (treasure.CanEquipOnPickup && treasure.Item != null)
            {
                infos = characterStatsTextService.GetFullStats(sheet).ToList();
            }
            else
            {
                infos = characterStatsTextService.GetVitalStats(persistence.Current.Party.Members).ToList();
            }
        }

        if (descriptions == null)
        {
            descriptions = new List<SharpText>();

            var description = new SharpText() { Color = Color.UIWhite };
            description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(treasure.InfoId), scaleHelper.Scaled(520), sharpGui);
            descriptions.Add(description);

            if (treasure.CanEquipOnPickup && treasure.Item != null)
            {
                descriptions.Add(new SharpText(" \n") { Color = Color.UIWhite });
                descriptions.AddRange(equipmentTextService.BuildEquipmentText(treasure.Item));
                descriptions.Add(new SharpText(" \nStat Changes") { Color = Color.UIWhite });
                descriptions.AddRange(equipmentTextService.GetComparisonText(treasure.Item, sheet));
            }
            description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(treasure.InfoId), scaleHelper.Scaled(520), sharpGui);
        }

        ILayoutItem layout;

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new PanelLayout(infoPanel,
           new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
        ));
        layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

        layout =
            new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
            new PanelLayout(descriptionPanel,
            new ColumnLayout(descriptions)
            {
                Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
            }
        ));
        layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

        layout = new RowLayout(previous, next) { Margin = new IntPad(scaleHelper.Scaled(10)) };
        IntRect backButtonRect;
        if (layoutNavigationRight)
        {
            backButtonRect = screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui));
        }
        else
        {
            backButtonRect = screenPositioner.GetBottomLeftRect(layout.GetDesiredSize(sharpGui));
        }
        layout.SetRect(backButtonRect);

        var hasInventoryRoom = treasure.IsPlotItem || sheet.HasRoom;

        take.Text = hasInventoryRoom ? "Take" : "Replace";

        itemInfo.Text = languageService.Current.Items.GetText(treasure.InfoId);

        var colLayout = new ColumnLayout(
                new PanelLayout(characterPanel, new ColumnLayout(
                    new CenterHorizontalLayout(currentCharacter),
                    new CenterHorizontalLayout(inventoryInfo),
                    new CenterHorizontalLayout(itemInfo))
                { Margin = new IntPad(scaleHelper.Scaled(10)) }))
        { Margin = new IntPad(scaleHelper.Scaled(10)) };
        colLayout.Add(GetMenuItems(treasure));

        colLayout.SetRect(screenPositioner.GetCenterTopRect(colLayout.GetDesiredSize(sharpGui)));

        if (!replacingItem)
        {
            sharpGui.Panel(characterPanel, panelStyle);
            sharpGui.Text(currentCharacter);
            sharpGui.Text(inventoryInfo);
            sharpGui.Text(itemInfo);
        }

        sharpGui.Panel(descriptionPanel, panelStyle);
        foreach (var description in descriptions)
        {
            sharpGui.Text(description);
        }

        sharpGui.Panel(infoPanel, panelStyle);
        foreach (var info in infos)
        {
            sharpGui.Text(info);
        }

        if (DateTime.Now < allowPickupTime)
        {
            return false;
        }

        if (equippingItem)
        {
            if (sharpGui.Button(equip, gamepadId, navUp: continueButton.Id, navDown: continueButton.Id, style: currentCharacterStyle))
            {
                treasure.Use(sheet.Inventory, sheet.CharacterSheet, inventoryFunctions, persistence.Current);
                NextTreasure();
                equippingItem = false;
            }
            if (sharpGui.Button(continueButton, gamepadId, navUp: equip.Id, navDown: equip.Id, style: currentCharacterStyle))
            {
                NextTreasure();
                equippingItem = false;
            }
        }
        else if(!choosingCharacter)
        {
            replaceButtons.Margin = scaleHelper.Scaled(10);
            replaceButtons.MaxWidth = scaleHelper.Scaled(900);
            replaceButtons.Bottom = screenPositioner.ScreenSize.Height;

            if (replacingItem)
            {
                sharpGui.Text(promptText);
                var removeItem = replaceButtons.Show(sharpGui, sheet.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(languageService.Current.Items.GetText(i.InfoId), i)).Append(new ButtonColumnItem<InventoryItem>("Cancel", CancelInventoryItem)), sheet.Inventory.Items.Count + 1, p => screenPositioner.GetCenterTopRect(p), gamepadId, wrapLayout: l => new ColumnLayout(new KeepWidthCenterLayout(promptText), l) { Margin = new IntPad(scaleHelper.Scaled(10)) }, style: currentCharacterStyle);
                if (removeItem != null)
                {
                    replacingItem = false;
                    if (removeItem != CancelInventoryItem)
                    {
                        scopedCoroutine.RunTask(async () =>
                        {
                            if (await confirmMenu.ShowAndWait($"Remove {languageService.Current.Items.GetText(removeItem.InfoId)} and take {languageService.Current.Items.GetText(treasure.InfoId)}?", parentSubMenu, gamepadId))
                            {
                                equippingItem = treasure.CanEquipOnPickup;
                                if (!equippingItem)
                                {
                                    NextTreasure();
                                }
                                sheet.RemoveItem(removeItem);
                                if (removeItem.Unique)
                                {
                                    persistence.Current.ItemVoid.Add(removeItem);
                                }
                                treasureGivenCallback?.Invoke(treasure);
                                treasure.GiveTo(sheet.Inventory, persistence.Current);
                            }
                        });
                    }
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    replacingItem = false;
                }
            }
            else
            {
                if (sharpGui.Button(take, gamepadId, navUp: previous.Id, navDown: treasure.CanUseOnPickup ? use.Id : discard.Id, navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle))
                {
                    if (hasInventoryRoom)
                    {
                        equippingItem = treasure.CanEquipOnPickup;
                        if (!equippingItem)
                        {
                            NextTreasure();
                        }
                        treasureGivenCallback?.Invoke(treasure);
                        treasure.GiveTo(sheet.Inventory, persistence.Current);
                    }
                    else
                    {
                        replacingItem = true;
                        replaceButtons.ListIndex = 0;
                        promptText.Text = $"Take {languageService.Current.Items.GetText(treasure.InfoId)} and discard...";
                        sharpGui.StealFocus(replaceButtons.TopButton);
                    }
                }

                if (treasure.CanUseOnPickup)
                {
                    if (sharpGui.Button(use, gamepadId, navUp: take.Id, navDown: discard.Id, navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle))
                    {
                        infos = null;
                        characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                        {
                            NextTreasure();
                            useCallback(treasure, i.Inventory, i.CharacterSheet, inventoryFunctions, persistence.Current);
                        }))
                        .Append(new ButtonColumnItem<Action>("Cancel", () => { }))
                        .ToList();
                    }
                }

                if (sharpGui.Button(discard, gamepadId, navUp: treasure.CanUseOnPickup ? use.Id : take.Id, navDown: previous.Id, navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle))
                {
                    scopedCoroutine.RunTask(async () =>
                    {
                        if(await confirmMenu.ShowAndWait($"Are you sure you want to discard the {languageService.Current.Items.GetText(currentTreasure.Peek().InfoId)}", parentSubMenu, gamepadId))
                        {
                            NextTreasure();
                            if (treasure.Item?.Unique == true)
                            {
                                persistence.Current.ItemVoid.Add(treasure.Item);
                            }
                        }
                    });
                }

                if (sharpGui.Button(previous, gamepadId, navUp: replacingItem ? replaceButtons.BottomButton : discard.Id, navDown: replacingItem ? replaceButtons.TopButton : take.Id, navLeft: replacingItem ? replaceButtons.TopButton : take.Id, navRight: next.Id, style: currentCharacterStyle) || sharpGui.IsStandardPreviousPressed(gamepadId))
                {
                    replacingItem = false;
                    --currentSheet;
                    if (currentSheet < 0)
                    {
                        currentSheet = persistence.Current.Party.Members.Count - 1;
                    }
                    descriptions = null;
                    infos = null;
                    FireActiveCharacterChanged();
                }
                if (sharpGui.Button(next, gamepadId, navUp: replacingItem ? replaceButtons.BottomButton : discard.Id, navDown: replacingItem ? replaceButtons.TopButton : take.Id, navLeft: previous.Id, navRight: replacingItem ? replaceButtons.TopButton : take.Id, style: currentCharacterStyle) || sharpGui.IsStandardNextPressed(gamepadId))
                {
                    replacingItem = false;
                    ++currentSheet;
                    if (currentSheet >= persistence.Current.Party.Members.Count)
                    {
                        currentSheet = 0;
                    }
                    descriptions = null;
                    infos = null;
                    FireActiveCharacterChanged();
                }
            }
        }

        return false;
    }

    private IEnumerable<ILayoutItem> GetMenuItems(ITreasure treasure)
    {
        if (equippingItem)
        {
            yield return new CenterHorizontalLayout(equip);
            yield return new CenterHorizontalLayout(continueButton);
        }
        else
        {
            yield return new CenterHorizontalLayout(take);
            if (treasure.CanUseOnPickup)
            {
                yield return new CenterHorizontalLayout(use);
            }
            if (!treasure.IsPlotItem)
            {
                yield return new CenterHorizontalLayout(discard);
            }
        }
    }

    private void NextTreasure()
    {
        currentTreasure.Pop();
        descriptions = null;
        infos = null;
    }

    public bool HasMoreTreasure => currentTreasure?.Count > 0;

    public void RecenterCamera()
    {
        FireActiveCharacterChanged();
    }

    private void FireActiveCharacterChanged()
    {
        if (activeCharacterChanged != null)
        {
            if (currentSheet > persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Current.Party.Members[currentSheet];
            this.activeCharacterChanged.Invoke(sheet);
        }
    }
}
