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
{
    public const float ChooseTargetLayer = 0.15f;
    public const float ReplaceButtonsLayer = 0.45f;
    private static readonly InventoryItem CancelInventoryItem = new InventoryItem();

    private enum SaveBlocker { Treasure }

    private readonly Persistence persistence;
    private readonly ISharpGui sharpGui;
    private readonly IScaleHelper scaleHelper;
    private readonly IScreenPositioner screenPositioner;
    private readonly IPersistenceWriter persistenceWriter;
    private readonly IInventoryFunctions inventoryFunctions;
    private readonly ILanguageService languageService;
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
    SharpText info = new SharpText() { Color = Color.White };
    SharpText description = new SharpText() { Color = Color.White };
    private int currentSheet;
    private bool replacingItem = false;
    private bool equippingItem = false;
    private DateTime allowPickupTime;
    private List<ButtonColumnItem<Action>> characterChoices = null;

    private ButtonColumn replaceButtons = new ButtonColumn(25, ReplaceButtonsLayer);
    private ButtonColumn characterButtons = new ButtonColumn(4, ChooseTargetLayer);

    public delegate void UseCallback(ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState);
    private UseCallback useCallback;

    public PickUpTreasureMenu
    (
        Persistence persistence,
        ISharpGui sharpGui,
        IScaleHelper scaleHelper,
        IScreenPositioner screenPositioner,
        IPersistenceWriter persistenceWriter,
        IInventoryFunctions inventoryFunctions,
        ILanguageService languageService
    )
    {
        this.persistence = persistence;
        this.sharpGui = sharpGui;
        this.scaleHelper = scaleHelper;
        this.screenPositioner = screenPositioner;
        this.persistenceWriter = persistenceWriter;
        this.inventoryFunctions = inventoryFunctions;
        this.languageService = languageService;
    }

    public void GatherTreasures(IEnumerable<ITreasure> treasure, TimeSpan pickupDelay, UseCallback useCallback)
    {
        this.useCallback = useCallback;
        currentTreasure = new Stack<ITreasure>(treasure);
        persistenceWriter.AddSaveBlock(SaveBlocker.Treasure);
        replacingItem = false;
        allowPickupTime = DateTime.Now + pickupDelay;
    }

    public bool Update(GamepadId gamepadId)
    {
        //Keep this block first so it exits in the section below
        var choosingCharacter = characterChoices != null;
        if (choosingCharacter)
        {
            characterButtons.StealFocus(sharpGui);

            characterButtons.Margin = scaleHelper.Scaled(10);
            characterButtons.MaxWidth = scaleHelper.Scaled(900);
            characterButtons.Bottom = screenPositioner.ScreenSize.Height;
            var action = characterButtons.Show(sharpGui, characterChoices, characterChoices.Count, s => screenPositioner.GetCenterRect(s), gamepadId);
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

        if (currentSheet > persistence.Current.Party.Members.Count)
        {
            currentSheet = 0;
        }
        var sheet = persistence.Current.Party.Members[currentSheet];

        currentCharacter.Text = sheet.CharacterSheet.Name;
        inventoryInfo.Text = equippingItem ? "Equip" : $"Items: {sheet.Inventory.Items.Count} / {sheet.CharacterSheet.InventorySize}";

        var treasure = currentTreasure.Peek();

        if (choosingCharacter)
        {
            ShowVitalStats();
        }
        else
        {
            ShowFullStats(sheet);
        }

        if (description.Text == null)
        {
            description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(treasure.InfoId), scaleHelper.Scaled(520), sharpGui);
        }

        ILayoutItem layout;

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(600),
           new ColumnLayout(new KeepWidthLeftLayout(previous), info) { Margin = new IntPad(scaleHelper.Scaled(10)) }
        ));
        layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

        layout =
            new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
            new MaxWidthLayout(scaleHelper.Scaled(600),
            new ColumnLayout(new KeepWidthRightLayout(next), description) { Margin = new IntPad(scaleHelper.Scaled(10)) }
        ));
        layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

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

        sharpGui.Text(currentCharacter);
        sharpGui.Text(inventoryInfo);
        sharpGui.Text(itemInfo);
        sharpGui.Text(description);
        sharpGui.Text(info);

        if (DateTime.Now < allowPickupTime)
        {
            return false;
        }

        if (equippingItem)
        {
            if (sharpGui.Button(equip, gamepadId, navUp: continueButton.Id, navDown: continueButton.Id))
            {
                treasure.Use(sheet.Inventory, sheet.CharacterSheet, inventoryFunctions, persistence.Current);
                currentTreasure.Pop();
                equippingItem = false;
            }
            if (sharpGui.Button(continueButton, gamepadId, navUp: equip.Id, navDown: equip.Id))
            {
                currentTreasure.Pop();
                equippingItem = false;
            }
        }
        else
        {
            if (sharpGui.Button(take, gamepadId, navUp: discard.Id, navDown: treasure.CanUseOnPickup ? use.Id : discard.Id, navLeft: previous.Id, navRight: next.Id))
            {
                if (hasInventoryRoom)
                {
                    equippingItem = treasure.CanEquipOnPickup;
                    if (!equippingItem)
                    {
                        currentTreasure.Pop();
                    }
                    treasure.GiveTo(sheet.Inventory, persistence.Current);
                }
                else
                {
                    replacingItem = true;
                    sharpGui.StealFocus(replaceButtons.TopButton);
                }
            }

            if (treasure.CanUseOnPickup)
            {
                if (sharpGui.Button(use, gamepadId, navUp: take.Id, navDown: discard.Id, navLeft: previous.Id, navRight: next.Id))
                {
                    characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                    {
                        currentTreasure.Pop();
                        useCallback(treasure, i.Inventory, i.CharacterSheet, inventoryFunctions, persistence.Current);
                    }))
                    .ToList();
                }
            }

            if (sharpGui.Button(discard, gamepadId, navUp: treasure.CanUseOnPickup ? use.Id : take.Id, navDown: take.Id, navLeft: previous.Id, navRight: next.Id))
            {
                currentTreasure.Pop();
            }

            if (sharpGui.Button(previous, gamepadId, navLeft: next.Id, navRight: replacingItem ? replaceButtons.TopButton : take.Id) || sharpGui.IsStandardPreviousPressed(gamepadId))
            {
                replacingItem = false;
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Current.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next, gamepadId, navLeft: replacingItem ? replaceButtons.TopButton : take.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed(gamepadId))
            {
                replacingItem = false;
                ++currentSheet;
                if (currentSheet >= persistence.Current.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }

            replaceButtons.Margin = scaleHelper.Scaled(10);
            replaceButtons.MaxWidth = scaleHelper.Scaled(900);
            replaceButtons.Bottom = screenPositioner.ScreenSize.Height;

            if (replacingItem)
            {
                var removeItem = replaceButtons.Show(sharpGui, sheet.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(languageService.Current.Items.GetText(i.InfoId), i)).Append(new ButtonColumnItem<InventoryItem>("Cancel", CancelInventoryItem)), sheet.Inventory.Items.Count + 1, p => screenPositioner.GetCenterTopRect(p), gamepadId, navLeft: previous.Id, navRight: next.Id);
                if (removeItem != null)
                {
                    replacingItem = false;
                    if (removeItem != CancelInventoryItem)
                    {
                        equippingItem = treasure.CanEquipOnPickup;
                        if (!equippingItem)
                        {
                            currentTreasure.Pop();
                        }
                        sheet.RemoveItem(removeItem);
                        treasure.GiveTo(sheet.Inventory, persistence.Current);
                    }
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    replacingItem = false;
                }
            }
        }

        return false;
    }

    private void ShowVitalStats()
    {
        var text = "";
        foreach (var character in persistence.Current.Party.Members)
        {
            text += $@"{character.CharacterSheet.Name}
HP:  {character.CharacterSheet.CurrentHp} / {character.CharacterSheet.Hp}
MP:  {character.CharacterSheet.CurrentMp} / {character.CharacterSheet.Mp}
  
";
        }
        info.Text = text;
    }

    private void ShowFullStats(Persistence.CharacterData characterData)
    {
        var characterSheetDisplay = characterData;

        info.Text =
$@"{characterSheetDisplay.CharacterSheet.Name}
 
Lvl: {characterSheetDisplay.CharacterSheet.Level}

Items:  {characterSheetDisplay.Inventory.Items.Count} / {characterSheetDisplay.CharacterSheet.InventorySize}

HP:  {characterSheetDisplay.CharacterSheet.CurrentHp} / {characterSheetDisplay.CharacterSheet.Hp}
MP:  {characterSheetDisplay.CharacterSheet.CurrentMp} / {characterSheetDisplay.CharacterSheet.Mp}
 
Att:   {characterSheetDisplay.CharacterSheet.Attack}
Att%:  {characterSheetDisplay.CharacterSheet.AttackPercent}
MAtt:  {characterSheetDisplay.CharacterSheet.MagicAttack}
MAtt%: {characterSheetDisplay.CharacterSheet.MagicAttackPercent}
Def:   {characterSheetDisplay.CharacterSheet.Defense}
Def%:  {characterSheetDisplay.CharacterSheet.DefensePercent}
MDef:  {characterSheetDisplay.CharacterSheet.MagicDefense}
MDef%: {characterSheetDisplay.CharacterSheet.MagicDefensePercent}
Item%: {characterSheetDisplay.CharacterSheet.TotalItemUsageBonus * 100f + 100f}
Heal%: {characterSheetDisplay.CharacterSheet.TotalHealingBonus * 100f + 100f}
 
Str: {characterSheetDisplay.CharacterSheet.TotalStrength}
Mag: {characterSheetDisplay.CharacterSheet.TotalMagic}
Vit: {characterSheetDisplay.CharacterSheet.TotalVitality}
Spr: {characterSheetDisplay.CharacterSheet.TotalSpirit}
Dex: {characterSheetDisplay.CharacterSheet.TotalDexterity}
Lck: {characterSheetDisplay.CharacterSheet.TotalLuck}
 ";

        foreach (var item in characterSheetDisplay.CharacterSheet.EquippedItems())
        {
            info.Text += $@"
{languageService.Current.Items.GetText(item.InfoId)}";
        }

        foreach (var item in characterSheetDisplay.CharacterSheet.Buffs)
        {
            info.Text += $@"
{item.Name}";
        }

        foreach (var item in characterSheetDisplay.CharacterSheet.Effects)
        {
            info.Text += $@"
{item.StatusEffect}";
        }
    }
}
