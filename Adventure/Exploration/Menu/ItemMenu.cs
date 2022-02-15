using Adventure.Items;
using Adventure.Services;
using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    class UseItemMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        SharpButton use = new SharpButton() { Text = "Use", Layer = ItemMenu.UseItemMenuLayer };
        SharpButton transfer = new SharpButton() { Text = "Transfer", Layer = ItemMenu.UseItemMenuLayer };
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

        public void Update(Persistence.CharacterData characterData)
        {
            if(SelectedItem == null) { return; }

            var choosingCharacter = characterChoices != null;

            if (!choosingCharacter 
               && sharpGui.FocusedItem != transfer.Id
               && sharpGui.FocusedItem != cancel.Id
               && sharpGui.FocusedItem != use.Id)
            {
                sharpGui.StealFocus(use.Id);
            }

            bool clearChoices = false;
            if (choosingCharacter)
            {
                characterButtons.StealFocus(sharpGui);

                characterButtons.Margin = scaleHelper.Scaled(10);
                characterButtons.MaxWidth = scaleHelper.Scaled(900);
                characterButtons.Bottom = screenPositioner.ScreenSize.Height;
                var action = characterButtons.Show(sharpGui, characterChoices, characterChoices.Count, s => screenPositioner.GetCenterRect(s));
                clearChoices = sharpGui.IsStandardBackPressed();
                if (action != null)
                {
                    action.Invoke();
                    clearChoices = true;
                }
            }

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(use, transfer, cancel) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetCenterRect(desiredSize));

            use.Text = SelectedItem.Equipment != null ? "Equip" : "Use";

            if (sharpGui.Button(use, navUp: cancel.Id, navDown: transfer.Id))
            {
                if (!choosingCharacter)
                {
                    if(SelectedItem.Equipment != null)
                    {
                        characterData.Inventory.Use(SelectedItem, characterData.CharacterSheet);
                        SelectedItem = null;
                    }
                    else
                    {
                        characterChoices = persistence.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                        {
                            characterData.Inventory.Use(SelectedItem, i.CharacterSheet);
                        }))
                        .ToList();
                    }
                }
            }
            if (sharpGui.Button(transfer, navUp: use.Id, navDown: cancel.Id))
            {
                if (!choosingCharacter)
                {
                    characterChoices = persistence.Party.Members.Where(i => i != characterData).Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                    {
                        if (SelectedItem.Equipment != null)
                        {
                            var id = SelectedItem.Equipment.Id;
                            if (id.HasValue)
                            {
                                characterData.CharacterSheet.RemoveEquipment(id.Value);
                            }
                        }
                        characterData.Inventory.Items.Remove(SelectedItem);
                        i.Inventory.Items.Add(SelectedItem);
                    }))
                    .ToList();
                }
            }
            if (sharpGui.Button(cancel, navUp: transfer.Id, navDown: use.Id) || sharpGui.IsStandardBackPressed())
            {
                if (!choosingCharacter)
                {
                    this.SelectedItem = null;
                }
            }

            if (clearChoices)
            {
                characterChoices = null;
                SelectedItem = null;
            }
        }
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

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            if (currentSheet > persistence.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var characterData = persistence.Party.Members[currentSheet];

            info.Rect = new IntRect(scaleHelper.Scaled(10), scaleHelper.Scaled(10), scaleHelper.Scaled(500), scaleHelper.Scaled(500));

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new RowLayout(previous, next), back) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            sharpGui.Text(info);
            info.Text =
$@"{characterData.CharacterSheet.Name}
 
Lvl: {characterData.CharacterSheet.Level}

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
 
Str: {characterData.CharacterSheet.BaseStrength}
Mag: {characterData.CharacterSheet.BaseMagic}
Vit: {characterData.CharacterSheet.BaseVitality}
Spr: {characterData.CharacterSheet.BaseSpirit}
Dex: {characterData.CharacterSheet.BaseDexterity}
Lck: {characterData.CharacterSheet.Luck}";

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            bool allowChanges = useItemMenu.SelectedItem == null;

            useItemMenu.Update(characterData);

            var newSelection = itemButtons.Show(sharpGui, characterData.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(i.Name, i)), characterData.Inventory.Items.Count, p => screenPositioner.GetCenterTopRect(p), navLeft: next.Id, navRight: previous.Id);
            if (allowChanges)
            {
                useItemMenu.SelectedItem = newSelection;
            }

            var hasItems = characterData.Inventory.Items.Count > 0;

            if (sharpGui.Button(previous, navUp: back.Id, navDown: back.Id, navLeft: hasItems ? itemButtons.TopButton : next.Id, navRight: next.Id) || sharpGui.IsStandardPreviousPressed())
            {
                if (allowChanges)
                {
                    --currentSheet;
                    if (currentSheet < 0)
                    {
                        currentSheet = persistence.Party.Members.Count - 1;
                    }
                }
            }
            if (sharpGui.Button(next, navUp: back.Id, navDown: back.Id, navLeft: previous.Id, navRight: hasItems ? itemButtons.TopButton : previous.Id) || sharpGui.IsStandardNextPressed())
            {
                if (allowChanges)
                {
                    ++currentSheet;
                    if (currentSheet >= persistence.Party.Members.Count)
                    {
                        currentSheet = 0;
                    }
                }
            }
            if (sharpGui.Button(back, navUp: previous.Id, navDown: previous.Id, navLeft: hasItems ? itemButtons.TopButton : back.Id, navRight: hasItems ? itemButtons.TopButton : back.Id) || sharpGui.IsStandardBackPressed())
            {
                if (allowChanges)
                {
                    menu.RequestSubMenu(menu.RootMenu);
                }
            }
        }
    }
}
