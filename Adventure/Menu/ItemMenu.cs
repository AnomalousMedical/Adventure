using Adventure.Items;
using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu
{
    class UseItemMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly IInventoryFunctions inventoryFunctions;
        private readonly ILanguageService languageService;
        SharpButton use = new SharpButton() { Text = "Use", Layer = ItemMenu.UseItemMenuLayer };
        SharpButton transfer = new SharpButton() { Text = "Transfer", Layer = ItemMenu.UseItemMenuLayer };
        SharpButton discard = new SharpButton() { Text = "Discard", Layer = ItemMenu.UseItemMenuLayer };
        SharpButton cancel = new SharpButton() { Text = "Cancel", Layer = ItemMenu.UseItemMenuLayer };
        SharpText itemPrompt = new SharpText() { Color = Color.White };
        SharpText characterChooserPrompt = new SharpText() { Color = Color.White };
        SharpText swapItemPrompt = new SharpText() { Color = Color.White };
        private List<ButtonColumnItem<Action>> characterChoices = null;
        private List<ButtonColumnItem<Action>> swapItemChoices = null;

        private ButtonColumn characterButtons = new ButtonColumn(5, ItemMenu.ChooseTargetLayer);
        private ButtonColumn replaceButtons = new ButtonColumn(25, ItemMenu.ReplaceButtonsLayer);

        public InventoryItem SelectedItem { get; set; }
        private String itemName;

        public event Action Closed;

        public UseItemMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            IInventoryFunctions inventoryFunctions,
            ILanguageService languageService
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.inventoryFunctions = inventoryFunctions;
            this.languageService = languageService;
        }

        public void Update(Persistence.CharacterData characterData, GamepadId gamepadId)
        {
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
                characterButtons.StealFocus(sharpGui);

                characterButtons.Margin = scaleHelper.Scaled(10);
                characterButtons.MaxWidth = scaleHelper.Scaled(900);
                characterButtons.Bottom = screenPositioner.ScreenSize.Height;
                var action = characterButtons.Show(sharpGui, characterChoices, characterChoices.Count, s => screenPositioner.GetCenterTopRect(s), gamepadId, wrapLayout: l => new ColumnLayout(new KeepWidthCenterLayout(characterChooserPrompt), l) { Margin = new IntPad(scaleHelper.Scaled(10)) });
                if (action != null)
                {
                    action.Invoke();
                    characterChoices = null;
                    if (swapItemChoices == null)
                    {
                        Close();
                        return;
                    }
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    characterChoices = null;
                }

                sharpGui.Text(characterChooserPrompt);
            }
            else
            {

                if (replacingItem)
                {
                    replaceButtons.StealFocus(sharpGui);

                    replaceButtons.Margin = scaleHelper.Scaled(10);
                    replaceButtons.MaxWidth = scaleHelper.Scaled(900);
                    replaceButtons.Bottom = screenPositioner.ScreenSize.Height;

                    var swapItem = replaceButtons.Show(sharpGui, swapItemChoices, swapItemChoices.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId, wrapLayout: l => new ColumnLayout(new KeepWidthCenterLayout(swapItemPrompt), l) { Margin = new IntPad(scaleHelper.Scaled(10)) });
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

                    sharpGui.Text(swapItemPrompt);
                }
                else
                {
                    var layout =
                       new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                       new MaxWidthLayout(scaleHelper.Scaled(600),
                       new ColumnLayout(new KeepWidthCenterLayout(itemPrompt), use, transfer, discard, cancel) { Margin = new IntPad(scaleHelper.Scaled(10)) }
                    ));

                    var desiredSize = layout.GetDesiredSize(sharpGui);
                    layout.SetRect(screenPositioner.GetCenterTopRect(desiredSize));

                    use.Text = SelectedItem.Equipment != null ? "Equip" : "Use";

                    sharpGui.Text(itemPrompt);

                    if (sharpGui.Button(use, gamepadId, navUp: cancel.Id, navDown: transfer.Id))
                    {
                        if (!choosingCharacter)
                        {
                            IsTransfer = false;
                            if (SelectedItem.Equipment != null)
                            {
                                inventoryFunctions.Use(SelectedItem, characterData.Inventory, characterData.CharacterSheet, characterData.CharacterSheet);
                                Close();
                            }
                            else
                            {
                                characterChooserPrompt.Text = "Use the " + itemName + " on...";
                                characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                                {
                                    inventoryFunctions.Use(SelectedItem, characterData.Inventory, characterData.CharacterSheet, i.CharacterSheet);
                                }))
                                .Append(new ButtonColumnItem<Action>("Cancel", () => { }))
                                .ToList();
                            }
                        }
                    }
                    if (sharpGui.Button(transfer, gamepadId, navUp: use.Id, navDown: discard.Id))
                    {
                        if (!choosingCharacter)
                        {
                            IsTransfer = true;
                            characterChooserPrompt.Text = "Transfer the " + itemName + " to...";
                            characterChoices = persistence.Current.Party.Members
                                .Where(i => i != characterData)
                                .Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                                {
                                    if (i.HasRoom)
                                    {
                                        characterData.RemoveItem(SelectedItem);
                                        i.Inventory.Items.Add(SelectedItem);
                                    }
                                    else
                                    {
                                        SwapTarget = i;
                                        swapItemChoices = i.Inventory.Items.Select(swapTarget => new ButtonColumnItem<Action>(languageService.Current.Items.GetText(swapTarget.InfoId), () =>
                                        {
                                            characterData.RemoveItem(SelectedItem);
                                            i.Inventory.Items.Add(SelectedItem);

                                            i.RemoveItem(swapTarget);
                                            characterData.Inventory.Items.Add(swapTarget);
                                        })).Append(new ButtonColumnItem<Action>("Cancel", () => { })).ToList();
                                    }
                                }))
                                .Append(new ButtonColumnItem<Action>("Cancel", () => { }))
                            .ToList();
                        }
                    }
                    if (sharpGui.Button(discard, gamepadId, navUp: transfer.Id, navDown: cancel.Id))
                    {
                        if (!choosingCharacter)
                        {
                            //TODO: Add confirmation for this
                            characterData.RemoveItem(SelectedItem);
                            Close();
                        }
                    }
                    if (sharpGui.Button(cancel, gamepadId, navUp: discard.Id, navDown: use.Id) || sharpGui.IsStandardBackPressed(gamepadId))
                    {
                        if (!choosingCharacter)
                        {
                            Close();
                        }
                    }
                }
            }
        }

        public bool IsChoosingCharacters => this.SelectedItem != null && this.characterChoices != null;

        public bool IsSwappingItems => this.SelectedItem != null && this.swapItemChoices != null;

        public Persistence.CharacterData SwapTarget { get; set; }

        public bool IsTransfer { get; set; }

        private void Close()
        {
            this.SelectedItem = null;
            itemPrompt.Text = null;
            Closed?.Invoke();
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
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        List<SharpText> descriptions = null;
        private int currentSheet;
        private UseItemMenu useItemMenu;
        private readonly ILanguageService languageService;
        private readonly CharacterMenuPositionService characterMenuPositionService;
        private readonly CameraMover cameraMover;
        private readonly EquipmentTextService equipmentTextService;
        private List<ButtonColumnItem<InventoryItem>> currentItems;

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
            EquipmentTextService equipmentTextService
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
            useItemMenu.Closed += UseItemMenu_Closed;
        }

        public void Dispose()
        {
            useItemMenu.Closed -= UseItemMenu_Closed;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepad)
        {
            bool allowChanges = useItemMenu.SelectedItem == null;

            if (currentSheet >= persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var characterData = persistence.Current.Party.Members[currentSheet];

            if (characterMenuPositionService.TryGetEntry(characterData.CharacterSheet, out var characterMenuPosition))
            {
                cameraMover.SetInterpolatedGoalPosition(characterMenuPosition.Position, characterMenuPosition.CameraRotation);
                characterMenuPosition.FaceCamera();
            }

            if (useItemMenu.IsChoosingCharacters)
            {
                ShowVitalStats();
            }
            else
            {
                ShowFullStats(characterData);
            }

            if (descriptions == null)
            {
                if (currentItems != null)
                {
                    var descriptionIndex = itemButtons.FocusedIndex(sharpGui);
                    if (descriptionIndex < currentItems.Count)
                    {
                        var item = currentItems[descriptionIndex];
                        var description = new SharpText() { Color = Color.White };
                        description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(item.Item.InfoId), scaleHelper.Scaled(520), sharpGui);

                        descriptions = new List<SharpText>();
                        descriptions.Add(description);
                        if (item.Item.Equipment != null)
                        {
                            descriptions.Add(new SharpText(" \n") { Color = Color.White });
                            descriptions.AddRange(equipmentTextService.BuildEquipmentText(item.Item));
                            descriptions.Add(new SharpText(" \nStat Changes") { Color = Color.White });
                            descriptions.AddRange(equipmentTextService.GetComparisonText(item.Item, characterData));
                        }
                    }
                }
            }

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new KeepWidthLeftLayout(previous), info) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            IEnumerable<ILayoutItem> columnItems = new[] { new KeepWidthRightLayout(next) };
            if (descriptions != null)
            {
                columnItems = columnItems.Concat(descriptions.Select(i => new KeepWidthRightLayout(i)));
            }

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(columnItems)
               {
                   Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
               }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), back);
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            sharpGui.Text(info);
            if (descriptions != null)
            {
                foreach (var description in descriptions)
                {
                    sharpGui.Text(description);
                }
            }

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            useItemMenu.Update(characterData, gamepad);

            if (allowChanges)
            {
                if (currentItems == null)
                {
                    currentItems = characterData.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(languageService.Current.Items.GetText(i.InfoId), i)).ToList();
                }
                var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
                var newSelection = itemButtons.Show(sharpGui, currentItems, currentItems.Count, p => screenPositioner.GetCenterTopRect(p), gamepad, navLeft: previous.Id, navRight: next.Id);
                if (lastItemIndex != itemButtons.FocusedIndex(sharpGui))
                {
                    descriptions = null;
                }
                useItemMenu.SelectedItem = newSelection;

                var hasItems = characterData.Inventory.Items.Any();

                if (sharpGui.Button(previous, gamepad, navUp: back.Id, navDown: back.Id, navLeft: next.Id, navRight: hasItems ? itemButtons.TopButton : next.Id) || sharpGui.IsStandardPreviousPressed(gamepad))
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
                        itemButtons.FocusTop(sharpGui);
                    }
                }
                if (sharpGui.Button(next, gamepad, navUp: back.Id, navDown: back.Id, navLeft: hasItems ? itemButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed(gamepad))
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
                        itemButtons.FocusTop(sharpGui);
                    }
                }
                if (sharpGui.Button(back, gamepad, navUp: next.Id, navDown: next.Id, navLeft: hasItems ? itemButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardBackPressed(gamepad))
                {
                    if (allowChanges)
                    {
                        currentItems = null;
                        descriptions = null;
                        menu.RequestSubMenu(menu.RootMenu, gamepad);
                    }
                }
            }
        }

        private void ShowVitalStats()
        {
            var text = "";
            foreach (var character in persistence.Current.Party.Members)
            {
                text += $"{character.CharacterSheet.Name}";
                if (useItemMenu.IsTransfer)
                {
                    text += $@"
Items:  {character.Inventory.Items.Count} / {character.CharacterSheet.InventorySize}
  
";
                }
                else
                {
                    text += $@"
HP:  {character.CharacterSheet.CurrentHp} / {character.CharacterSheet.Hp}
MP:  {character.CharacterSheet.CurrentMp} / {character.CharacterSheet.Mp}
  
";
                }
            }
            info.Text = text;
        }

        private void ShowFullStats(Persistence.CharacterData characterData)
        {
            var characterSheetDisplay = characterData;
            if (useItemMenu.IsSwappingItems)
            {
                characterSheetDisplay = useItemMenu.SwapTarget;
            }

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

        private void UseItemMenu_Closed()
        {
            descriptions = null;
            currentItems = null;
        }
    }
}
