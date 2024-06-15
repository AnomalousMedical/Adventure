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
    IScreenPositioner screenPositioner
) : IExplorationSubMenu
{
    private SharpText message = new SharpText() { Color = Color.White };
    private SharpInput input = new SharpInput();
    private SharpButton yesButton = new SharpButton();
    private SharpButton noButton = new SharpButton();

    private Action<string> yesCallback;
    private Action noCallback;
    private IExplorationSubMenu previousMenu;

    public void Setup(String message, Action<string> yes, Action no, IExplorationSubMenu previousMenu, String currentText = null, String yesText = "Yes", String noText = "No")
    {
        this.message.Text = message;
        this.yesButton.Text = yesText;
        this.noButton.Text = noText;
        this.input.Text.Clear();
        if (currentText != null)
        {
            this.input.Text.Append(currentText);
        }
        this.yesCallback = yes;
        this.noCallback = no;
        this.previousMenu = previousMenu;
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new ColumnLayout(new KeepWidthCenterLayout(message), input, new KeepWidthCenterLayout(yesButton), new KeepWidthCenterLayout(noButton)) { Margin = new IntPad(10) }
        );

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetCenterRect(desiredSize));

        sharpGui.Text(message);

        sharpGui.Input(input, navUp: noButton.Id, navDown: yesButton.Id);

        if (sharpGui.Button(yesButton, gamepadId, navUp: input.Id, navDown: noButton.Id))
        {
            menu.RequestSubMenu(previousMenu, gamepadId);
            yesCallback(input.Text.ToString());
        }
        else if (sharpGui.Button(noButton, gamepadId, navUp: yesButton.Id, navDown: input.Id))
        {
            menu.RequestSubMenu(previousMenu, gamepadId);
            noCallback();
        }
    }
}
