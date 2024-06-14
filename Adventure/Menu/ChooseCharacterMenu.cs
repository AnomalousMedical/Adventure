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
    PartyMemberManager partyMemberManager,
    IWorldDatabase worldDatabase,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    IScaleHelper scaleHelper,
    PartyMemberTriggerManager partyMemberTriggerManager,
    CameraMover cameraMover,
    FileMenu fileMenu,
    OptionsMenu optionsMenu,
    ConfirmMenu confirmMenu
) : IExplorationSubMenu
{
    private SharpText prompt = new SharpText() { Color = Color.White };

    SharpButton choose = new SharpButton() { Text = "Choose" };
    SharpButton next = new SharpButton() { Text = "Next" };
    SharpButton previous = new SharpButton() { Text = "Previous" };
    SharpButton files = new SharpButton() { Text = "Files" };
    SharpButton options = new SharpButton() { Text = "Options" };

    private int currentPlayerIndex = 0;
    private bool instantMoveCamera = true;

    public void Reset()
    {
        instantMoveCamera = true;
        currentPlayerIndex = 0;
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        ILayoutItem layout;

        if (partyMemberTriggerManager.Count > 0)
        {
            prompt.Text = "Choose your character...";
            layout = new MarginLayout(scaleHelper.Scaled(new IntPad(10)), prompt);
            layout.SetRect(screenPositioner.GetCenterTopRect(layout.GetDesiredSize(sharpGui)));

            var currentTrigger = partyMemberTriggerManager.Get(currentPlayerIndex);

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
                --currentPlayerIndex;
                if (currentPlayerIndex < 0)
                {
                    currentPlayerIndex = partyMemberTriggerManager.Count - 1;
                }
            }
            if (sharpGui.Button(next, gamepadId, navLeft: previous.Id, navRight: choose.Id) || sharpGui.IsStandardNextPressed(gamepadId))
            {
                ++currentPlayerIndex;
                currentPlayerIndex %= partyMemberTriggerManager.Count;
            }
            if (sharpGui.Button(choose, gamepadId, navLeft: next.Id, navRight: files.Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                confirmMenu.Setup($"Are you sure you want to choose {currentTrigger.Name}?",
                    yes: () =>
                    {
                        currentTrigger.AddToParty();
                        menu.RequestSubMenu(null, gamepadId);
                    },
                    no: () =>
                    {

                    },
                    this);
                menu.RequestSubMenu(confirmMenu, gamepadId);                
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
            prompt.Text = "Anomalous Adventure";
            layout = new MarginLayout(scaleHelper.Scaled(new IntPad(10)), prompt);
            layout.SetRect(screenPositioner.GetCenterRect(layout.GetDesiredSize(sharpGui)));
        }

        sharpGui.Text(prompt);
    }
}
