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
        private List<ButtonColumnItem<Action>> characterChoices = null;
        private List<ButtonColumnItem<Action>> swapItemChoices = null;

        private ButtonColumn characterButtons = new ButtonColumn(4, ItemMenu.ChooseTargetLayer);
        private ButtonColumn replaceButtons = new ButtonColumn(25, ItemMenu.ReplaceButtonsLayer);

        public InventoryItem SelectedItem { get; set; }

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
                var action = characterButtons.Show(sharpGui, characterChoices, characterChoices.Count, s => screenPositioner.GetCenterRect(s), gamepadId);
                if (action != null)
                {
                    action.Invoke();
                    characterChoices = null;
                    if (swapItemChoices == null)
                    {
                        SelectedItem = null;
                        return;
                    }
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    characterChoices = null;
                }
            }

            if (replacingItem)
            {
                replaceButtons.StealFocus(sharpGui);

                replaceButtons.Margin = scaleHelper.Scaled(10);
                replaceButtons.MaxWidth = scaleHelper.Scaled(900);
                replaceButtons.Bottom = screenPositioner.ScreenSize.Height;

                var swapItem = replaceButtons.Show(sharpGui, swapItemChoices, swapItemChoices.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId);
                if (swapItem != null)
                {
                    swapItem.Invoke();
                    swapItemChoices = null;
                    SelectedItem = null;
                    SwapTarget = null;
                    return;
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    swapItemChoices = null;
                }
            }

            if (!replacingItem)
            {
                var layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                   new MaxWidthLayout(scaleHelper.Scaled(600),
                   new ColumnLayout(use, transfer, discard, cancel) { Margin = new IntPad(scaleHelper.Scaled(10)) }
                ));

                var desiredSize = layout.GetDesiredSize(sharpGui);
                layout.SetRect(screenPositioner.GetCenterRect(desiredSize));

                use.Text = SelectedItem.Equipment != null ? "Equip" : "Use";

                if (sharpGui.Button(use, gamepadId, navUp: cancel.Id, navDown: transfer.Id))
                {
                    if (!choosingCharacter)
                    {
                        IsTransfer = false;
                        if (SelectedItem.Equipment != null)
                        {
                            inventoryFunctions.Use(SelectedItem, characterData.Inventory, characterData.CharacterSheet, characterData.CharacterSheet);
                            SelectedItem = null;
                        }
                        else
                        {
                            characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                            {
                                inventoryFunctions.Use(SelectedItem, characterData.Inventory, characterData.CharacterSheet, i.CharacterSheet);
                            }))
                            .ToList();
                        }
                    }
                }
                if (sharpGui.Button(transfer, gamepadId, navUp: use.Id, navDown: discard.Id))
                {
                    if (!choosingCharacter)
                    {
                        IsTransfer = true;
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
                        .ToList();
                    }
                }
                if (sharpGui.Button(discard, gamepadId, navUp: transfer.Id, navDown: cancel.Id))
                {
                    if (!choosingCharacter)
                    {
                        //TODO: Add confirmation for this
                        characterData.RemoveItem(SelectedItem);
                        this.SelectedItem = null;
                    }
                }
                if (sharpGui.Button(cancel, gamepadId, navUp: discard.Id, navDown: use.Id) || sharpGui.IsStandardBackPressed(gamepadId))
                {
                    if (!choosingCharacter)
                    {
                        this.SelectedItem = null;
                    }
                }
            }
        }

        public bool IsChoosingCharacters => this.SelectedItem != null && this.characterChoices != null;

        public bool IsSwappingItems => this.SelectedItem != null && this.swapItemChoices != null;

        public Persistence.CharacterData SwapTarget { get; set; }

        public bool IsTransfer { get; set; }
    }

    class ItemMenu : IExplorationSubMenu
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
        SharpText description = new SharpText() { Color = Color.White };
        private int currentSheet;
        private UseItemMenu useItemMenu;
        private readonly ILanguageService languageService;
        private List<ButtonColumnItem<InventoryItem>> currentItems;

        public ItemMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            UseItemMenu useItemMenu,
            ILanguageService languageService
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.useItemMenu = useItemMenu;
            this.languageService = languageService;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepad)
        {
            bool allowChanges = useItemMenu.SelectedItem == null;

            if (currentSheet > persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var characterData = persistence.Current.Party.Members[currentSheet];

            if (useItemMenu.IsChoosingCharacters)
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
            else
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

            if (description.Text == null)
            {
                if (currentItems != null)
                {
                    var descriptionIndex = itemButtons.FocusedIndex(sharpGui);
                    if(descriptionIndex < currentItems.Count)
                    {
                        var item = currentItems[descriptionIndex];
                        description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(item.Item.InfoId), scaleHelper.Scaled(520), sharpGui);
                    }
                }
            }

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new KeepSizeLeftLayout(previous), info) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new KeepSizeRightLayout(next), description) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), back);
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            sharpGui.Text(info);
            sharpGui.Text(description);

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            useItemMenu.Update(characterData, gamepad);

            if (currentItems == null)
            {
                currentItems = characterData.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(languageService.Current.Items.GetText(i.InfoId), i)).ToList();
            }
            var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
            var newSelection = itemButtons.Show(sharpGui, currentItems, currentItems.Count, p => screenPositioner.GetCenterTopRect(p), gamepad, navLeft: previous.Id, navRight: next.Id);
            if(lastItemIndex != itemButtons.FocusedIndex(sharpGui))
            {
                description.Text = null;
            }
            if (allowChanges)
            {
                useItemMenu.SelectedItem = newSelection;
            }

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
                    description.Text = null;
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
                    description.Text = null;
                    itemButtons.FocusTop(sharpGui);
                }
            }
            if (sharpGui.Button(back, gamepad, navUp: next.Id, navDown: next.Id, navLeft: hasItems ? itemButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardBackPressed(gamepad))
            {
                if (allowChanges)
                {
                    currentItems = null;
                    description.Text = null;
                    menu.RequestSubMenu(menu.RootMenu, gamepad);
                }
            }
        }
    }
}
