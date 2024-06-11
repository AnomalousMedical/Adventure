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
    private SharpText message = new SharpText() { Color = Color.White };
    private SharpButton yesButton = new SharpButton() { Text = "Yes" };
    private SharpButton noButton = new SharpButton() { Text = "No" };

    private Action yesCallback;
    private Action noCallback;
    private IExplorationSubMenu previousMenu;

    public void Setup(String message, Action yes, Action no, IExplorationSubMenu previousMenu)
    {
        this.message.Text = message;
        this.yesCallback = yes;
        this.noCallback = no;
        this.previousMenu = previousMenu;
    }

    public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
    {
        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new ColumnLayout(new KeepWidthCenterLayout(message), yesButton, noButton) { Margin = new IntPad(10) }
        );

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetCenterRect(desiredSize));

        sharpGui.Text(message);

        if (sharpGui.Button(yesButton, gamepadId, navUp: noButton.Id, navDown: noButton.Id))
        {
            menu.RequestSubMenu(previousMenu, gamepadId);
            yesCallback();
        }
        else if (sharpGui.Button(noButton, gamepadId, navUp: yesButton.Id, navDown: yesButton.Id))
        {
            menu.RequestSubMenu(previousMenu, gamepadId);
            noCallback();
        }
    }
}
