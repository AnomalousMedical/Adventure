using Adventure.Services;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using Engine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

class ChooseCharacterMenu
(
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    IScaleHelper scaleHelper,
    PartyMemberTriggerManager partyMemberTriggerManager,
    CameraMover cameraMover,
    FileMenu fileMenu,
    OptionsMenu optionsMenu,
    ConfirmMenu confirmMenu,
    CharacterStatsTextService characterStatsTextService,
    ILanguageService languageService,
    UserInputMenu userInputMenu,
    IScopedCoroutine coroutine,
    Persistence persistence,
    FontLoader fontLoader
) : IExplorationSubMenu
{
    private SharpText title = new SharpText("Anomalous Adventure") { Color = Color.White, Font = fontLoader.TitleFont };
    private SharpText prompt = new SharpText("Choose your character...") { Color = Color.White };

    private SharpButton choose = new SharpButton() { Text = "Choose" };
    private SharpButton next = new SharpButton() { Text = "Next" };
    private SharpButton previous = new SharpButton() { Text = "Previous" };
    private SharpButton files = new SharpButton() { Text = "Files" };
    private SharpButton options = new SharpButton() { Text = "Options" };

    private int currentPlayerIndex = 0;
    private bool instantMoveCamera = true;

    private List<SharpText> infos = null;
    private List<SharpText> descriptions = null;

    public void Reset()
    {
        descriptions = null;
        infos = null;
        instantMoveCamera = true;
        currentPlayerIndex = 0;
    }

    public void MoveCameraToCurrentTrigger()
    {
        if (partyMemberTriggerManager.Count > 0)
        {
            var currentTrigger = partyMemberTriggerManager.Get(currentPlayerIndex);
            cameraMover.SetPosition(currentTrigger.CameraPosition, currentTrigger.CameraAngle);
        }
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        ILayoutItem layout;

        if (partyMemberTriggerManager.Count > 0)
        {
            layout = new MarginLayout(scaleHelper.Scaled(new IntPad(10)), prompt);
            layout.SetRect(screenPositioner.GetCenterTopRect(layout.GetDesiredSize(sharpGui)));
            sharpGui.Text(prompt);

            var currentTrigger = partyMemberTriggerManager.Get(currentPlayerIndex);

            if (infos == null)
            {
                infos = characterStatsTextService.GetFullStats(currentTrigger.CharacterData).ToList();
            }

            if (descriptions == null)
            {
                descriptions = new List<SharpText>()
                {
                    new SharpText("Skills") { Color = Color.White }
                };
                foreach (var skill in currentTrigger.CharacterData.CharacterSheet.Skills)
                {
                    descriptions.Add(new SharpText(languageService.Current.Skills.GetText(skill)) { Color = Color.White });
                }
                if (currentTrigger.CharacterData.CharacterSheet.CanBlock)
                {
                    descriptions.Add(new SharpText("Block") { Color = Color.White });
                }
            }

            if (instantMoveCamera)
            {
                instantMoveCamera = false;
                cameraMover.SetPosition(currentTrigger.CameraPosition, currentTrigger.CameraAngle);
            }
            else
            {
                cameraMover.SetInterpolatedGoalPosition(currentTrigger.CameraPosition, currentTrigger.CameraAngle);
            }

            layout = new RowLayout(previous, next, choose, files, options) { Margin = new IntPad(scaleHelper.Scaled(10)) };
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            if (sharpGui.Button(previous, gamepadId, navLeft: options.Id, navRight: next.Id) || sharpGui.IsStandardPreviousPressed(gamepadId))
            {
                descriptions = null;
                infos = null;
                --currentPlayerIndex;
                if (currentPlayerIndex < 0)
                {
                    currentPlayerIndex = partyMemberTriggerManager.Count - 1;
                }
            }
            if (sharpGui.Button(next, gamepadId, navLeft: previous.Id, navRight: choose.Id) || sharpGui.IsStandardNextPressed(gamepadId))
            {
                descriptions = null;
                infos = null;
                ++currentPlayerIndex;
                currentPlayerIndex %= partyMemberTriggerManager.Count;
            }
            if (sharpGui.Button(choose, gamepadId, navLeft: next.Id, navRight: files.Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                coroutine.RunTask(async () =>
                {
                    var nameResult = await userInputMenu.ShowAndWait("Enter your name.", this, gamepadId,
                    currentText: currentTrigger.CharacterData.CharacterSheet.Name,
                    yesText: "Confirm",
                    noText: "Cancel");

                    if (nameResult.Confirmed)
                    {
                        var confirmChoice = await confirmMenu.ShowAndWait($"Are you sure you want to choose {nameResult.Value}?", this, menu, gamepadId);
                        if (confirmChoice)
                        {
                            persistence.Current.Player.Position = currentTrigger.SpawnPosition;
                            persistence.Current.Player.Started = true;
                            currentTrigger.CharacterData.CharacterSheet.Name = nameResult.Value;
                            currentTrigger.AddToParty();
                            menu.RequestSubMenu(null, gamepadId);
                        }
                    }
                });
            }
            if (sharpGui.Button(files, gamepadId, navLeft: choose.Id, navRight: options.Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                fileMenu.PreviousMenu = this;
                menu.RequestSubMenu(fileMenu, gamepadId);
            }
            if (sharpGui.Button(options, gamepadId, navLeft: files.Id, navRight: previous.Id))
            {
                optionsMenu.PreviousMenu = this;
                menu.RequestSubMenu(optionsMenu, gamepadId); //Have to fix party menu somehow
            }
        }
        else
        {
            layout = new MarginLayout(scaleHelper.Scaled(new IntPad(10)), title);
            layout.SetRect(screenPositioner.GetCenterRect(layout.GetDesiredSize(sharpGui)));
            sharpGui.Text(title);
        }

        if (infos != null)
        {
            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            foreach (var info in infos)
            {
                sharpGui.Text(info);
            }
        }

        if (descriptions != null)
        {
            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(descriptions.Select(i => new KeepWidthRightLayout(i))) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            foreach (var description in descriptions)
            {
                sharpGui.Text(description);
            }
        }
    }
}
