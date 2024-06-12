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
    ConfirmMenu confirmMenu
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
    SharpText itemInfo = new SharpText() { Color = Color.White };
    SharpText currentCharacter = new SharpText() { Color = Color.White };
    SharpText inventoryInfo = new SharpText() { Color = Color.White };
    List<SharpText> infos;
    List<SharpText> descriptions;
    SharpText promptText = new SharpText() { Color = Color.White };
    private int currentSheet;
    private bool replacingItem = false;
    private bool equippingItem = false;
    private DateTime allowPickupTime;
    private List<ButtonColumnItem<Action>> characterChoices = null;

    private ButtonColumn replaceButtons = new ButtonColumn(25, ReplaceButtonsLayer);
    private ButtonColumn characterButtons = new ButtonColumn(5, ChooseTargetLayer);

    public delegate void UseCallback(ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState);
    private UseCallback useCallback;
    private Action<Persistence.CharacterData> activeCharacterChanged;

    private bool layoutNavigationRight = true;

    public void GatherTreasures(IEnumerable<ITreasure> treasure, TimeSpan pickupDelay, UseCallback useCallback, Action<Persistence.CharacterData> activeCharacterChanged = null, bool layoutNavigationRight = true)
    {
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

            var description = new SharpText() { Color = Color.White };
            description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(treasure.InfoId), scaleHelper.Scaled(520), sharpGui);
            descriptions.Add(description);

            if (treasure.CanEquipOnPickup && treasure.Item != null)
            {
                descriptions.Add(new SharpText(" \n") { Color = Color.White });
                descriptions.AddRange(equipmentTextService.BuildEquipmentText(treasure.Item));
                descriptions.Add(new SharpText(" \nStat Changes") { Color = Color.White });
                descriptions.AddRange(equipmentTextService.GetComparisonText(treasure.Item, sheet));
            }
            description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(treasure.InfoId), scaleHelper.Scaled(520), sharpGui);
        }

        ILayoutItem layout;

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(600),
           new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
        ));
        layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

        layout =
            new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
            new MaxWidthLayout(scaleHelper.Scaled(600),
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

        if (equippingItem)
        {
            layout =
              new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
              new MaxWidthLayout(scaleHelper.Scaled(600),
              new ColumnLayout(
                  new CenterHorizontalLayout(currentCharacter),
                  new CenterHorizontalLayout(inventoryInfo),
                  new CenterHorizontalLayout(itemInfo),
                  new CenterHorizontalLayout(equip),
                  new CenterHorizontalLayout(continueButton))
              { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
        }
        else
        {
            if (treasure.CanUseOnPickup)
            {
                layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                   new MaxWidthLayout(scaleHelper.Scaled(600),
                   new ColumnLayout(
                       new CenterHorizontalLayout(currentCharacter),
                       new CenterHorizontalLayout(inventoryInfo),
                       new CenterHorizontalLayout(itemInfo),
                       new CenterHorizontalLayout(take),
                       new CenterHorizontalLayout(use),
                       new CenterHorizontalLayout(discard))
                   { Margin = new IntPad(scaleHelper.Scaled(10)) }
                ));
            }
            else
            {
                layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                   new MaxWidthLayout(scaleHelper.Scaled(600),
                   new ColumnLayout(
                       new CenterHorizontalLayout(currentCharacter),
                       new CenterHorizontalLayout(inventoryInfo),
                       new CenterHorizontalLayout(itemInfo),
                       new CenterHorizontalLayout(take),
                       new CenterHorizontalLayout(discard))
                   { Margin = new IntPad(scaleHelper.Scaled(10)) }
                ));
            }
        }

        layout.SetRect(screenPositioner.GetCenterTopRect(layout.GetDesiredSize(sharpGui)));

        if (!replacingItem)
        {
            sharpGui.Text(currentCharacter);
            sharpGui.Text(inventoryInfo);
            sharpGui.Text(itemInfo);
        }

        foreach (var description in descriptions)
        {
            sharpGui.Text(description);
        }

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
                        confirmMenu.Setup($"Remove {languageService.Current.Items.GetText(removeItem.InfoId)} and take {languageService.Current.Items.GetText(treasure.InfoId)}?",
                            yes: () =>
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
                                treasure.GiveTo(sheet.Inventory, persistence.Current);
                            },
                            no: () => { },
                            parentSubMenu);
                        menu.RequestSubMenu(confirmMenu, gamepadId);
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
                    confirmMenu.Setup($"Are you sure you want to discard the {languageService.Current.Items.GetText(currentTreasure.Peek().InfoId)}",
                        yes: () =>
                        {
                            NextTreasure();
                            if (treasure.Item?.Unique == true)
                            {
                                persistence.Current.ItemVoid.Add(treasure.Item);
                            }
                        },
                        no: () => { },
                        parentSubMenu);
                    menu.RequestSubMenu(confirmMenu, gamepadId);
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

    private void NextTreasure()
    {
        currentTreasure.Pop();
        descriptions = null;
        infos = null;
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
