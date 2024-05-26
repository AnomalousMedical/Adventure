using Adventure.Battle;
using Adventure.Items;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu
{
    class SkillMenu : IExplorationSubMenu
    {
        public const float SkillButtonsLayer = 0.15f;
        public const float ChooseTargetLayer = 0.35f;

        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly ISkillFactory skillFactory;
        private readonly IDamageCalculator damageCalculator;
        private readonly ILanguageService languageService;
        private ButtonColumn skillButtons = new ButtonColumn(25, SkillButtonsLayer);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        private int currentSheet;

        private ButtonColumn characterButtons = new ButtonColumn(5, SkillMenu.ChooseTargetLayer);
        private List<ButtonColumnItem<Action>> characterChoices = null;
        private ISkill selectedSkill;
        private List<ButtonColumnItem<ISkill>> currentPlayerSkills = null;

        public SkillMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            ISkillFactory skillFactory,
            IDamageCalculator damageCalculator,
            ILanguageService languageService
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.skillFactory = skillFactory;
            this.damageCalculator = damageCalculator;
            this.languageService = languageService;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepad)
        {
            var choosingCharacter = characterChoices != null;

            if (choosingCharacter)
            {
                characterButtons.StealFocus(sharpGui);

                characterButtons.Margin = scaleHelper.Scaled(10);
                characterButtons.MaxWidth = scaleHelper.Scaled(900);
                characterButtons.Bottom = screenPositioner.ScreenSize.Height;
                var action = characterButtons.Show(sharpGui, characterChoices.Append(new ButtonColumnItem<Action>("Cancel", () => { })), characterChoices.Count + 1, s => screenPositioner.GetCenterRect(s), gamepad);
                if (action != null)
                {
                    action.Invoke();
                    characterChoices = null;
                    selectedSkill = null;
                }

                if (sharpGui.IsStandardBackPressed(gamepad))
                {
                    characterChoices = null;
                }
            }

            if (currentSheet > persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var characterData = persistence.Current.Party.Members[currentSheet];

            if (choosingCharacter)
            {
                var text = "";
                foreach (var character in persistence.Current.Party.Members)
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
Item%: {characterData.CharacterSheet.TotalItemUsageBonus * 100f + 100f}
Heal%: {characterData.CharacterSheet.TotalHealingBonus * 100f + 100f}
 
Str: {characterData.CharacterSheet.TotalStrength}
Mag: {characterData.CharacterSheet.TotalMagic}
Vit: {characterData.CharacterSheet.TotalVitality}
Spr: {characterData.CharacterSheet.TotalSpirit}
Dex: {characterData.CharacterSheet.TotalDexterity}
Lck: {characterData.CharacterSheet.TotalLuck}
 ";

                foreach(var item in characterData.CharacterSheet.EquippedItems())
                {
                    info.Text += $@"
{languageService.Current.Items.GetText(item.InfoId)}";
                }

                foreach (var item in characterData.CharacterSheet.Buffs)
                {
                    info.Text += $@"
{item.Name}";
                }

                foreach (var item in characterData.CharacterSheet.Effects)
                {
                    info.Text += $@"
{item.StatusEffect}";
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

            skillButtons.Margin = scaleHelper.Scaled(10);
            skillButtons.MaxWidth = scaleHelper.Scaled(900);
            skillButtons.Bottom = screenPositioner.ScreenSize.Height;

            if (currentPlayerSkills == null)
            {
                currentPlayerSkills = characterData.CharacterSheet.Skills.Select(i =>
                {
                    var skill = skillFactory.CreateSkill(i);
                    return new ButtonColumnItem<ISkill>(skill.Name, skill);
                }).ToList();
            }
            var skillCount = currentPlayerSkills.Count;
            var newSelection = skillButtons.Show(sharpGui, currentPlayerSkills, skillCount, p => screenPositioner.GetCenterTopRect(p), gamepad, navLeft: previous.Id, navRight: next.Id);
            if (!choosingCharacter && newSelection != null)
            {
                if(characterData.CharacterSheet.CurrentHp > 0)
                {
                    selectedSkill = newSelection;
                    characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                    {
                        selectedSkill.Apply(damageCalculator, characterData.CharacterSheet, i.CharacterSheet);
                    }))
                    .ToList();
                }
            }

            var hasSkills = skillCount > 0;

            if (sharpGui.Button(previous, gamepad, navUp: back.Id, navDown: back.Id, navLeft: next.Id, navRight: hasSkills ? skillButtons.TopButton : next.Id) || sharpGui.IsStandardPreviousPressed(gamepad))
            {
                if (!choosingCharacter)
                {
                    --currentSheet;
                    if (currentSheet < 0)
                    {
                        currentSheet = persistence.Current.Party.Members.Count - 1;
                    }
                    currentPlayerSkills = null;
                }
            }
            if (sharpGui.Button(next, gamepad, navUp: back.Id, navDown: back.Id, navLeft: hasSkills ? skillButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed(gamepad))
            {
                if (!choosingCharacter)
                {
                    ++currentSheet;
                    if (currentSheet >= persistence.Current.Party.Members.Count)
                    {
                        currentSheet = 0;
                    }
                    currentPlayerSkills = null;
                }
            }
            if (sharpGui.Button(back, gamepad, navUp: next.Id, navDown: next.Id, navLeft: hasSkills ? skillButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardBackPressed(gamepad))
            {
                if (!choosingCharacter)
                {
                    currentPlayerSkills = null;
                    menu.RequestSubMenu(menu.RootMenu, gamepad);
                }
            }
        }
    }
}
