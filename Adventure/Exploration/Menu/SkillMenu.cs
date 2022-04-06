using Adventure.Battle;
using Adventure.Items;
using Adventure.Services;
using Engine;
using RpgMath;
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
        public const float ChooseTargetLayer = 0.35f;

        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly ISkillFactory skillFactory;
        private readonly IDamageCalculator damageCalculator;
        private ButtonColumn itemButtons = new ButtonColumn(25, ItemButtonsLayer);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        private int currentSheet;

        private ButtonColumn characterButtons = new ButtonColumn(4, SkillMenu.ChooseTargetLayer);
        private List<ButtonColumnItem<Action>> characterChoices = null;
        private String selectedSpell;

        public SkillMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            ISkillFactory skillFactory,
            IDamageCalculator damageCalculator
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.skillFactory = skillFactory;
            this.damageCalculator = damageCalculator;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            var allowChanges = true;

            var choosingCharacter = characterChoices != null;

            if (choosingCharacter)
            {
                characterButtons.StealFocus(sharpGui);

                characterButtons.Margin = scaleHelper.Scaled(10);
                characterButtons.MaxWidth = scaleHelper.Scaled(900);
                characterButtons.Bottom = screenPositioner.ScreenSize.Height;
                var action = characterButtons.Show(sharpGui, characterChoices, characterChoices.Count, s => screenPositioner.GetCenterRect(s));
                if (action != null)
                {
                    action.Invoke();
                    characterChoices = null;
                    selectedSpell = null;
                    return;
                }

                if (sharpGui.IsStandardBackPressed())
                {
                    characterChoices = null;
                }
            }

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

            if (choosingCharacter)
            {
                var text = "";
                foreach (var character in persistence.Party.Members)
                {
                    text += @$"{character.CharacterSheet.Name}
HP:  {character.CharacterSheet.CurrentHp} / {character.CharacterSheet.Hp}
MP:  {character.CharacterSheet.CurrentMp} / {character.CharacterSheet.Mp}
  
";
                }
                info.Text = text;
            }
            else
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

            var newSelection = itemButtons.Show(sharpGui, characterData.CharacterSheet.Skills.Select(i => new ButtonColumnItem<String>(i, i)), characterData.Inventory.Items.Count, p => screenPositioner.GetCenterTopRect(p), navLeft: next.Id, navRight: previous.Id);
            if (allowChanges && newSelection != null)
            {
                selectedSpell = newSelection;
                characterChoices = persistence.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                {
                    var spell = skillFactory.CreateSkill(selectedSpell);
                    spell.Apply(damageCalculator, characterData.CharacterSheet, i.CharacterSheet);
                }))
                .ToList();
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
