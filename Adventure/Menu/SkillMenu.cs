﻿using Adventure.Items;
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
    ISoundEffectPlayer soundEffectPlayer,
    ItemMenu itemMenu,
    SelectedCharacterService selectedCharacterService
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
    SharpButton items = new SharpButton() { Text = "Items" };
    SharpButton close = new SharpButton() { Text = "Close" };
    SharpText noSkills = new SharpText() { Text = "No Skills", Color = Color.UIWhite };
    SharpPanel noSkillsPanel = new SharpPanel();
    List<SharpText> infos;
    List<SharpText> descriptions;
    private ISkillEffect currentEffect;
    private SharpPanel descriptionPanel = new SharpPanel();
    private SharpPanel infoPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.UITransparentBg };

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

        var characterData = persistence.Current.Party.Members[selectedCharacterService.SelectedCharacter];
        var currentCharacterStyle = characterStyleService.GetCharacterStyle(characterData.StyleIndex);

        if (choosingCharacter)
        {
            characterButtons.Margin = scaleHelper.Scaled(10);
            characterButtons.MaxWidth = scaleHelper.Scaled(900);
            characterButtons.Bottom = screenPositioner.ScreenSize.Height;
            characterButtons.ScrollBarWidth = scaleHelper.Scaled(25);
            characterButtons.ScrollMargin = scaleHelper.Scaled(5);

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
                descriptions.Add(new SharpText(description) { Color = Color.UIWhite });
                var skill = skillFactory.CreateSkill(item.Item);
                var mpCost = skill.GetMpCost(false, false);
                if (mpCost != 0)
                {
                    descriptions.Add(new SharpText($"MP: {mpCost}") { Color = mpCost > characterData.CharacterSheet.CurrentMp ? Color.UIRed : Color.UIWhite });
                }
            }
        }

        ILayoutItem layout;

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(20)),
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
           new MarginLayout(new IntPad(scaleHelper.Scaled(20)),
           new PanelLayout(descriptionPanel,
           new ColumnLayout(columnItems)
           {
               Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
           }
        ));
        var descriptionNeedsLayout = true;

        layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), new RowLayout(previous, next, items, close) { Margin = new IntPad(scaleHelper.Scaled(10)) });
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
                skillButtons.ScrollBarWidth = scaleHelper.Scaled(25);
                skillButtons.ScrollMargin = scaleHelper.Scaled(5);

                var lastSkillIndex = skillButtons.FocusedIndex(sharpGui);
                var newSelection = skillButtons.Show(sharpGui, currentPlayerSkills, skillCount, 
                    p => screenPositioner.GetTopRightRect(p), 
                    gamepad, 
                    navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle, 
                    wrapLayout: l => new RowLayout(new KeepHeightLayout(descriptionLayout), new MarginLayout(new IntPad(0, scaleHelper.Scaled(10), scaleHelper.Scaled(20), 0), l)), 
                    navUp: close.Id, navDown: close.Id);

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
                        else
                        {
                            soundEffectPlayer.PlaySound(Assets.SoundEffects.ErrorSoundEffect.Instance);
                        }
                    }
                    else
                    {
                        soundEffectPlayer.PlaySound(Assets.SoundEffects.ErrorSoundEffect.Instance);
                    }
                }
            }
            else
            {
                var noSkillsLayout = new MarginLayout(new IntPad(0, scaleHelper.Scaled(20), scaleHelper.Scaled(20), 0), 
                    new PanelLayout(noSkillsPanel, 
                    new MarginLayout(new IntPad(scaleHelper.Scaled(10)), 
                    noSkills
                )));
                noSkillsLayout.SetRect(screenPositioner.GetTopRightRect(noSkillsLayout.GetDesiredSize(sharpGui)));

                sharpGui.Panel(noSkillsPanel, panelStyle);
                sharpGui.Text(noSkills);
            }

            if (sharpGui.Button(previous, gamepad, navUp: hasSkills ? skillButtons.BottomButton : previous.Id, navDown: hasSkills ? skillButtons.TopButton : previous.Id, navLeft: close.Id, navRight: next.Id, style: currentCharacterStyle) || sharpGui.IsStandardPreviousPressed(gamepad))
            {
                selectedCharacterService.Previous();
                currentPlayerSkills = null;
                infos = null;
                skillButtons.FocusTop(sharpGui);
            }
            if (sharpGui.Button(next, gamepad, navUp: hasSkills ? skillButtons.BottomButton : next.Id, navDown: hasSkills ? skillButtons.TopButton : next.Id, navLeft: previous.Id, navRight: items.Id, style: currentCharacterStyle) || sharpGui.IsStandardNextPressed(gamepad))
            {
                selectedCharacterService.Next();
                currentPlayerSkills = null;
                infos = null;
                skillButtons.FocusTop(sharpGui);
            }
            if (sharpGui.Button(items, gamepad, navUp: hasSkills ? skillButtons.BottomButton : items.Id, navDown: hasSkills ? skillButtons.TopButton : items.Id, navLeft: next.Id, navRight: close.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepad))
            {
                currentPlayerSkills = null;
                menu.RequestSubMenu(itemMenu, gamepad);
                infos = null;
            }
            if (sharpGui.Button(close, gamepad, navUp: hasSkills ? skillButtons.BottomButton : close.Id, navDown: hasSkills ? skillButtons.TopButton : close.Id, navLeft: items.Id, navRight: previous.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepad))
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
