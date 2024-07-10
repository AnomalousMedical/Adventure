using Adventure.Items;
using Adventure.Services;
using Adventure.Skills;
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
    CharacterStatsTextService characterStatsTextService,
    CharacterStyleService characterStyleService,
    IObjectResolverFactory objectResolverFactory,
    IScopedCoroutine coroutine,
    IClockService clockService,
    ISoundEffectPlayer soundEffectPlayer
) : IExplorationSubMenu, IDisposable
{
    public const float SkillButtonsLayer = 0.15f;
    public const float ChooseTargetLayer = 0.35f;
    private readonly IClockService clockService = clockService;
    private readonly ISoundEffectPlayer soundEffectPlayer = soundEffectPlayer;
    private IObjectResolver objectResolver = objectResolverFactory.Create();

    private ButtonColumn skillButtons = new ButtonColumn(25, SkillButtonsLayer);
    SharpButton next = new SharpButton() { Text = "Next" };
    SharpButton previous = new SharpButton() { Text = "Previous" };
    SharpButton close = new SharpButton() { Text = "Close" };
    List<SharpText> infos;
    List<SharpText> descriptions;
    private int currentSheet;
    private ISkillEffect currentEffect;
    private SharpPanel descriptionPanel = new SharpPanel();
    private SharpPanel infoPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

    private ButtonColumn characterButtons = new ButtonColumn(5, SkillMenu.ChooseTargetLayer);
    private List<ButtonColumnItem<Action>> characterChoices = null;
    private List<ButtonColumnItem<String>> currentPlayerSkills = null;

    public void Dispose()
    {
        objectResolver.Dispose();
    }

    public void Update(IExplorationMenu menu, GamepadId gamepad)
    {
        if(currentEffect != null)
        {
            currentEffect.Update(clockService.Clock);
            if (currentEffect.Finished)
            {
                currentEffect = null;
            }
            return;
        }

        var choosingCharacter = characterChoices != null;

        if (currentSheet >= persistence.Current.Party.Members.Count)
        {
            currentSheet = 0;
        }
        var characterData = persistence.Current.Party.Members[currentSheet];
        var currentCharacterStyle = characterStyleService.GetCharacterStyle(characterData.StyleIndex);

        if (choosingCharacter)
        {
            characterButtons.StealFocus(sharpGui);

            characterButtons.Margin = scaleHelper.Scaled(10);
            characterButtons.MaxWidth = scaleHelper.Scaled(900);
            characterButtons.Bottom = screenPositioner.ScreenSize.Height;
            var action = characterButtons.Show(sharpGui, characterChoices.Append(new ButtonColumnItem<Action>("Cancel", () => { })), characterChoices.Count + 1, s => screenPositioner.GetCenterRect(s), gamepad, style: currentCharacterStyle);
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

        if (currentPlayerSkills == null)
        {
            descriptions = null;
            currentPlayerSkills = characterData.CharacterSheet.Skills
                .Select(i => new ButtonColumnItem<String>(languageService.Current.Skills.GetText(i), i))
                .ToList();
        }

        if (characterMenuPositionService.TryGetEntry(characterData.CharacterSheet, out var characterMenuPosition))
        {
            cameraMover.SetInterpolatedGoalPosition(characterMenuPosition.CameraPosition, characterMenuPosition.CameraRotation);
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
                var skill = skillFactory.CreateSkill(item.Item);
                var mpCost = skill.GetMpCost(false, false);
                if (mpCost != 0)
                {
                    descriptions.Add(new SharpText($"MP: {mpCost}") { Color = mpCost > characterData.CharacterSheet.CurrentMp ? Color.Red : Color.White });
                }
            }
        }

        ILayoutItem layout;

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new PanelLayout(infoPanel,
           new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
        ));
        layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

        IEnumerable<ILayoutItem> columnItems = Enumerable.Empty<ILayoutItem>();
        if (descriptions != null)
        {
            columnItems = columnItems.Concat(descriptions.Select(i => new KeepWidthRightLayout(i)));
        }

        var descriptionLayout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new PanelLayout(descriptionPanel,
           new ColumnLayout(columnItems)
           {
               Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
           }
        ));
        var descriptionNeedsLayout = true;

        layout = new RowLayout(previous, next, close) { Margin = new IntPad(scaleHelper.Scaled(10)) };
        var backButtonLayoutRect = screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui));
        layout.SetRect(backButtonLayoutRect);

        var currentInfos = infos;
        var currentDescriptions = descriptions;

        if (!choosingCharacter)
        {
            var skillCount = currentPlayerSkills.Count;
            var hasSkills = skillCount > 0;

            if (hasSkills)
            {
                skillButtons.Margin = scaleHelper.Scaled(10);
                skillButtons.MaxWidth = scaleHelper.Scaled(900);
                skillButtons.Bottom = backButtonLayoutRect.Top;

                var lastSkillIndex = skillButtons.FocusedIndex(sharpGui);
                var newSelection = skillButtons.Show(sharpGui, currentPlayerSkills, skillCount, p => screenPositioner.GetTopRightRect(p), gamepad, navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle, wrapLayout: l => new RowLayout(new KeepHeightLayout(descriptionLayout), l) { Margin = new IntPad(scaleHelper.Scaled(10)) }, navUp: close.Id, navDown: close.Id);
                descriptionNeedsLayout = false; //Layout happens as part of showing the skill buttons
                if (lastSkillIndex != skillButtons.FocusedIndex(sharpGui))
                {
                    descriptions = null;
                }
                if (newSelection != null)
                {
                    if (characterData.CharacterSheet.CurrentHp > 0)
                    {
                        var selectedSkill = skillFactory.CreateSkill(newSelection);
                        if (selectedSkill.UseInField)
                        {
                            characterChoices = persistence.Current.Party.Members.Select(i => new ButtonColumnItem<Action>(i.CharacterSheet.Name, () =>
                            {
                                currentEffect = selectedSkill.Apply(damageCalculator, characterData.CharacterSheet, i.CharacterSheet, characterMenuPositionService, objectResolver, coroutine, cameraMover, soundEffectPlayer);
                                infos = null;
                                descriptions = null;
                            }))
                            .ToList();
                        }
                    }
                }
            }

            if (sharpGui.Button(next, gamepad, navUp: hasSkills ? skillButtons.BottomButton : next.Id, navDown: hasSkills ? skillButtons.TopButton : next.Id, navLeft: previous.Id, navRight: close.Id, style: currentCharacterStyle) || sharpGui.IsStandardNextPressed(gamepad))
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
            if (sharpGui.Button(previous, gamepad, navUp: hasSkills ? skillButtons.BottomButton : previous.Id, navDown: hasSkills ? skillButtons.TopButton : previous.Id, navLeft: close.Id, navRight: next.Id, style: currentCharacterStyle) || sharpGui.IsStandardPreviousPressed(gamepad))
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
            if (sharpGui.Button(close, gamepad, navUp: hasSkills ? skillButtons.BottomButton : close.Id, navDown: hasSkills ? skillButtons.TopButton : close.Id, navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepad))
            {
                currentPlayerSkills = null;
                menu.RequestSubMenu(menu.RootMenu, gamepad);
                infos = null;
            }
        }

        if(descriptionNeedsLayout)
        {
            descriptionLayout.SetRect(screenPositioner.GetTopRightRect(descriptionLayout.GetDesiredSize(sharpGui)));
        }

        if (currentInfos != null)
        {
            sharpGui.Panel(infoPanel, panelStyle);
            foreach (var info in currentInfos)
            {
                sharpGui.Text(info);
            }
        }

        if (currentDescriptions != null && currentDescriptions.Count > 0)
        {
            sharpGui.Panel(descriptionPanel, panelStyle);
            foreach (var description in currentDescriptions)
            {
                sharpGui.Text(description);
            }
        }
    }
}
