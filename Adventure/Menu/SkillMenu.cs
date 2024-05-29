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

namespace Adventure.Menu;

class SkillMenu
(
    Persistence persistence,
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    ISkillFactory skillFactory,
    IDamageCalculator damageCalculator,
    ILanguageService languageService,
    CharacterMenuPositionService characterMenuPositionService,
    CameraMover cameraMover,
    CharacterStatsTextService characterStatsTextService
) : IExplorationSubMenu
{
    public const float SkillButtonsLayer = 0.15f;
    public const float ChooseTargetLayer = 0.35f;

    private ButtonColumn skillButtons = new ButtonColumn(25, SkillButtonsLayer);
    SharpButton next = new SharpButton() { Text = "Next" };
    SharpButton previous = new SharpButton() { Text = "Previous" };
    SharpButton back = new SharpButton() { Text = "Back" };
    List<SharpText> infos;
    List<SharpText> descriptions;
    private int currentSheet;

    private ButtonColumn characterButtons = new ButtonColumn(5, SkillMenu.ChooseTargetLayer);
    private List<ButtonColumnItem<Action>> characterChoices = null;
    private List<ButtonColumnItem<String>> currentPlayerSkills = null;

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
            }

            if (sharpGui.IsStandardBackPressed(gamepad))
            {
                characterChoices = null;
            }
        }

        if (currentSheet >= persistence.Current.Party.Members.Count)
        {
            currentSheet = 0;
        }
        var characterData = persistence.Current.Party.Members[currentSheet];

        if (currentPlayerSkills == null)
        {
            descriptions = null;
            currentPlayerSkills = characterData.CharacterSheet.Skills
                .Select(i => new ButtonColumnItem<String>(languageService.Current.Skills.GetText(i), i))
                .ToList();
        }

        if (characterMenuPositionService.TryGetEntry(characterData.CharacterSheet, out var characterMenuPosition))
        {
            cameraMover.SetInterpolatedGoalPosition(characterMenuPosition.Position, characterMenuPosition.CameraRotation);
            characterMenuPosition.FaceCamera();
        }

        if (infos == null)
        {
            infos = characterStatsTextService.GetVitalStats(persistence.Current.Party.Members).ToList();
        }

        if (descriptions == null)
        {
            descriptions = new List<SharpText>();
            var descriptionIndex = skillButtons.FocusedIndex(sharpGui);
            if (descriptionIndex < currentPlayerSkills.Count)
            {
                var item = currentPlayerSkills[descriptionIndex];
                var description = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Skills.GetDescription(item.Item), scaleHelper.Scaled(520), sharpGui);
                descriptions.Add(new SharpText(description) { Color = Color.White });
            }
        }

        ILayoutItem layout;

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(600),
           new ColumnLayout(new ILayoutItem[] { new KeepWidthLeftLayout(previous) }.Concat(infos)) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
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

        foreach (var info in infos)
        {
            sharpGui.Text(info);
        }

        foreach(var description in descriptions)
        {
            sharpGui.Text(description);
        }

        skillButtons.Margin = scaleHelper.Scaled(10);
        skillButtons.MaxWidth = scaleHelper.Scaled(900);
        skillButtons.Bottom = screenPositioner.ScreenSize.Height;

        var skillCount = currentPlayerSkills.Count;
        var lastSkillIndex = skillButtons.FocusedIndex(sharpGui);
        var newSelection = skillButtons.Show(sharpGui, currentPlayerSkills, skillCount, p => screenPositioner.GetCenterTopRect(p), gamepad, navLeft: previous.Id, navRight: next.Id);
        if (lastSkillIndex != skillButtons.FocusedIndex(sharpGui))
        {
            descriptions = null;
        }
        if (!choosingCharacter && newSelection != null)
        {
            if(characterData.CharacterSheet.CurrentHp > 0)
            {
                var selectedSkill = skillFactory.CreateSkill(newSelection);
                characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                {
                    selectedSkill.Apply(damageCalculator, characterData.CharacterSheet, i.CharacterSheet);
                    infos = null;
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
                infos = null;
                skillButtons.FocusTop(sharpGui);
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
                infos = null;
                skillButtons.FocusTop(sharpGui);
            }
        }
        if (sharpGui.Button(back, gamepad, navUp: next.Id, navDown: next.Id, navLeft: hasSkills ? skillButtons.TopButton : previous.Id, navRight: previous.Id) || sharpGui.IsStandardBackPressed(gamepad))
        {
            if (!choosingCharacter)
            {
                currentPlayerSkills = null;
                menu.RequestSubMenu(menu.RootMenu, gamepad);
                infos = null;
            }
        }
    }
}
