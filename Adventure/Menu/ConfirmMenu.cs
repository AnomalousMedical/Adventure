using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

class ConfirmMenu
(
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner
) : IExplorationSubMenu
{
    public enum Location
    {
        Top,
        Center
    }

    private SharpText message = new SharpText() { Color = Color.UIWhite };
    private SharpButton yesButton = new SharpButton() { Text = "Yes" };
    private SharpButton noButton = new SharpButton() { Text = "No" };
    private SharpPanel promptPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

    private IExplorationSubMenu previousMenu;
    private TaskCompletionSource<bool> currentTask;
    private bool confirmed = false;
    private IExplorationMenu explorationMenu;
    private Location location;

    public void Link(IExplorationMenu explorationMenu)
    {
        this.explorationMenu = explorationMenu;
    }

    public Task<bool> ShowAndWait(String message, IExplorationSubMenu previousMenu, GamepadId gamepadId, string confirmText = "Yes", string rejectText = "No", Location location = Location.Top)
    {
        this.yesButton.Text = confirmText;
        this.noButton.Text = rejectText;
        this.confirmed = false;
        this.message.Text = message;
        this.previousMenu = previousMenu;
        this.location = location;
        explorationMenu.RequestSubMenu(this, gamepadId);
        return WaitForCurrentInput();
    }

    public Task<bool> WaitForCurrentInput()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource<bool>();
        }
        return currentTask.Task;
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new ColumnLayout(new KeepWidthCenterLayout(new PanelLayout(promptPanel, message)),
               new KeepWidthCenterLayout(new ColumnLayout(yesButton, noButton) { Margin = new IntPad(10) }
        )));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        IntRect layoutRect;
        switch (location)
        {
            default:
            case Location.Top:
                layoutRect = screenPositioner.GetCenterTopRect(desiredSize);
                break;
            case Location.Center:
                layoutRect = screenPositioner.GetCenterRect(desiredSize);
                break;
        }
        layout.SetRect(layoutRect);

        sharpGui.Panel(promptPanel, panelStyle);
        sharpGui.Text(message);

        if (sharpGui.Button(yesButton, gamepadId, navUp: noButton.Id, navDown: noButton.Id))
        {
            confirmed = true;
            Close(menu, gamepadId);
        }
        else if (sharpGui.Button(noButton, gamepadId, navUp: yesButton.Id, navDown: yesButton.Id))
        {
            confirmed = false;
            Close(menu, gamepadId);
        }
    }

    private void Close(IExplorationMenu menu, GamepadId gamepadId)
    {
        menu.RequestSubMenu(previousMenu, gamepadId);
        var tempTask = currentTask;
        currentTask = null;
        tempTask?.SetResult(confirmed);
        sharpGui.StealFocus(Guid.Empty);
    }
}
