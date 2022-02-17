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
    class SkillMenu : IExplorationSubMenu
    {
        public const float ItemButtonsLayer = 0.15f;
        public const float UseSkillMenuLayer = 0.25f;
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

        public SkillMenu
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

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            var allowChanges = true;

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

//            if (useItemMenu.IsChoosingCharacters)
//            {
//                var text = "";
//                foreach(var character in persistence.Party.Members)
//                {
//                    text += $"{character.CharacterSheet.Name}";
//                    if (useItemMenu.IsTransfer)
//                    {
//                        text += $@"
//Items:  {character.Inventory.Items.Count} / {character.Inventory.Size}
  
//";
//                    }
//                    else
//                    {
//                        text += $@"
//HP:  {character.CharacterSheet.CurrentHp} / {character.CharacterSheet.Hp}
//MP:  {character.CharacterSheet.CurrentMp} / {character.CharacterSheet.Mp}
  
//";
//                    }
//                }
//                info.Text = text;
//            }
//            else
            {
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
Lck: {characterData.CharacterSheet.Luck}
 ";

                foreach(var item in characterData.CharacterSheet.EquippedItems())
                {
                    info.Text += $@"
{item.Name}";
                }
            }

            sharpGui.Text(info);

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            var newSelection = itemButtons.Show(sharpGui, characterData.CharacterSheet.Spells.Select(i => new ButtonColumnItem<String>(i, i)), characterData.Inventory.Items.Count, p => screenPositioner.GetCenterTopRect(p), navLeft: next.Id, navRight: previous.Id);
            if (allowChanges)
            {
                
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
