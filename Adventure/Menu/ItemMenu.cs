using Adventure.Items;
using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu
{
    class UseItemMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        SharpButton use = new SharpButton() { Text = "Use", Layer = ItemMenu.UseItemMenuLayer };
        SharpButton transfer = new SharpButton() { Text = "Transfer", Layer = ItemMenu.UseItemMenuLayer };
        SharpButton discard = new SharpButton() { Text = "Discard", Layer = ItemMenu.UseItemMenuLayer };
        SharpButton cancel = new SharpButton() { Text = "Cancel", Layer = ItemMenu.UseItemMenuLayer };
        private List<ButtonColumnItem<Action>> characterChoices = null;

        private ButtonColumn characterButtons = new ButtonColumn(4, ItemMenu.ChooseTargetLayer);

        public InventoryItem SelectedItem { get; set; }

        public UseItemMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
        }

        public void Update(Persistence.CharacterData characterData, GamepadId gamepadId)
        {
            if(SelectedItem == null) { return; }

            var choosingCharacter = characterChoices != null;

            if (!choosingCharacter 
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
                    SelectedItem = null;
                    return;
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    characterChoices = null;
                }
            }

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
                    if(SelectedItem.Equipment != null)
                    {
                        characterData.Inventory.Use(SelectedItem, characterData.CharacterSheet, characterData.CharacterSheet);
                        SelectedItem = null;
                    }
                    else
                    {
                        characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                        {
                            characterData.Inventory.Use(SelectedItem, characterData.CharacterSheet, i.CharacterSheet);
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
                        .Where(i => i != characterData && i.HasRoom)
                        .Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                        {
                            characterData.RemoveItem(SelectedItem);
                            i.Inventory.Items.Add(SelectedItem);
                        }))
                    .ToList();
                }
            }
            if(sharpGui.Button(discard, gamepadId, navUp: transfer.Id, navDown: cancel.Id))
            {
                //TODO: Add confirmation for this
                characterData.RemoveItem(SelectedItem);
                this.SelectedItem = null;
            }
            if (sharpGui.Button(cancel, gamepadId, navUp: discard.Id, navDown: use.Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                if (!choosingCharacter)
                {
                    this.SelectedItem = null;
                }
            }
        }

        public bool IsChoosingCharacters => this.SelectedItem != null && this.characterChoices != null;

        public bool IsTransfer { get; set; }
    }

    class ItemMenu : IExplorationSubMenu
    {
        public const float ItemButtonsLayer = 0.15f;
        public const float UseItemMenuLayer = 0.25f;
        public const float ChooseTargetLayer = 0.35f;

        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private ButtonColumn itemButtons = new ButtonColumn(25, ItemButtonsLayer);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        private int currentSheet;
        private UseItemMenu useItemMenu;

        public ItemMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            UseItemMenu useItemMenu
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.useItemMenu = useItemMenu;
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
                foreach(var character in persistence.Current.Party.Members)
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
                info.Text =
    $@"{characterData.CharacterSheet.Name}
 
Lvl: {characterData.CharacterSheet.Level}

Items:  {characterData.Inventory.Items.Count} / {characterData.CharacterSheet.InventorySize}

HP:  {characterData.CharacterSheet.CurrentHp} / {characterData.CharacterSheet.Hp}
MP:  {characterData.CharacterSheet.CurrentMp} / {characterData.CharacterSheet.Mp}
 
Att:   {characterData.CharacterSheet.Attack}
Att%:  {characterData.CharacterSheet.AttackPercent}
MAtt:  {characterData.CharacterSheet.MagicAttack}
MAtt%: {characterData.CharacterSheet.MagicAttackPercent}
Def:   {characterData.CharacterSheet.Defense}
Def%:  {characterData.CharacterSheet.DefensePercent}
MDef:  {characterData.CharacterSheet.MagicDefense}
MDef%: {characterData.CharacterSheet.MagicDefensePercent}
Item%: {characterData.CharacterSheet.TotalItemUsageBonus * 100f}
 
Str: {characterData.CharacterSheet.TotalStrength}
Mag: {characterData.CharacterSheet.TotalMagic}
Vit: {characterData.CharacterSheet.TotalVitality}
Spr: {characterData.CharacterSheet.TotalSpirit}
Dex: {characterData.CharacterSheet.TotalDexterity}
Lck: {characterData.CharacterSheet.TotalLuck}
 ";

                foreach (var item in characterData.CharacterSheet.EquippedItems())
                {
                    info.Text += $@"
{item.Name}";
                }
            }

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(previous, info) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(next) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), back);
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            sharpGui.Text(info);

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            useItemMenu.Update(characterData, gamepad);

            var newSelection = itemButtons.Show(sharpGui, characterData.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(i.Name, i)), characterData.Inventory.Items.Count, p => screenPositioner.GetCenterTopRect(p), gamepad, navLeft: previous.Id, navRight: next.Id);
            if (allowChanges)
            {
                useItemMenu.SelectedItem = newSelection;
            }

            var hasItems = characterData.Inventory.Items.Any();

            if (sharpGui.Button(previous, gamepad, navUp: back.Id, navDown: back.Id, navLeft: next.Id, navRight: hasItems ? itemButtons.TopButton : next.Id) || sharpGui.IsStandardPreviousPressed(gamepad))
            {
                if (allowChanges)
                {
                    --currentSheet;
                    if (currentSheet < 0)
                    {
                        currentSheet = persistence.Current.Party.Members.Count - 1;
                    }
                }
            }
            if (sharpGui.Button(next, gamepad, navUp: back.Id, navDown: back.Id, navLeft: hasItems ? itemButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed(gamepad))
            {
                if (allowChanges)
                {
                    ++currentSheet;
                    if (currentSheet >= persistence.Current.Party.Members.Count)
                    {
                        currentSheet = 0;
                    }
                }
            }
            if (sharpGui.Button(back, gamepad, navUp: next.Id, navDown: next.Id, navLeft: hasItems ? itemButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardBackPressed(gamepad))
            {
                if (allowChanges)
                {
                    menu.RequestSubMenu(menu.RootMenu, gamepad);
                }
            }
        }
    }
}
