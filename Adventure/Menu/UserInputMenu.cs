using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

class UserInputMenu
(
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    IExplorationMenu explorationMenu
) : IExplorationSubMenu
{
    public record Result(bool Confirmed, string Value);

    private SharpText message = new SharpText() { Color = Color.White };
    private SharpInput input = new SharpInput();
    private SharpButton yesButton = new SharpButton();
    private SharpButton noButton = new SharpButton();

    private IExplorationSubMenu previousMenu;

    private TaskCompletionSource<Result> currentTask;
    private bool confirmed = false;

    public Task<Result> ShowAndWait(String message, IExplorationSubMenu previousMenu, GamepadId gamepad, String currentText = null, String yesText = "Yes", String noText = "No")
    {
        this.confirmed = false;
        this.message.Text = message;
        this.yesButton.Text = yesText;
        this.noButton.Text = noText;
        this.input.Text.Clear();
        if (currentText != null)
        {
            this.input.Text.Append(currentText);
        }
        this.previousMenu = previousMenu;
        explorationMenu.RequestSubMenu(this, gamepad);
        return WaitForCurrentInput();
    }

    public Task<Result> WaitForCurrentInput()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource<Result>();
        }
        return currentTask.Task;
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new ColumnLayout(new KeepWidthCenterLayout(message), new KeepWidthCenterLayout(new FixedWidthLayout(scaleHelper.Scaled(200), input)), new KeepWidthCenterLayout(yesButton), new KeepWidthCenterLayout(noButton)) { Margin = new IntPad(10) }
        );

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetCenterRect(desiredSize));

        sharpGui.Text(message);

        sharpGui.Input(input, navUp: noButton.Id, navDown: yesButton.Id);

        if (sharpGui.Button(yesButton, gamepadId, navUp: input.Id, navDown: noButton.Id))
        {
            this.confirmed = true;
            Close(menu, gamepadId);
        }
        else if (sharpGui.Button(noButton, gamepadId, navUp: yesButton.Id, navDown: input.Id))
        {
            this.confirmed = false;
            Close(menu, gamepadId);
        }
    }

    private void Close(IExplorationMenu menu, GamepadId gamepadId)
    {
        menu.RequestSubMenu(previousMenu, gamepadId);
        var tempTask = currentTask;
        currentTask = null;
        tempTask?.SetResult(new Result(confirmed, this.input.Text.ToString()));
    }
}
