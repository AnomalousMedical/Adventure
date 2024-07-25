using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class PauseService(UpdateTimer updateTimer, ISharpGui sharpGui, FontLoader fontLoader, IScreenPositioner screenPositioner)
{
    private SharpText pause = new SharpText("Pause") { Font = fontLoader.TitleFont, Color = Color.UIWhite };

    public void UnpausedUpdate()
    {
        if (sharpGui.GamepadButtonEntered[0] == GamepadButtonCode.XInput_Start)
        {
            updateTimer.Live = false;
        }
    }

    public void PausedUpdate()
    {
        pause.SetRect(screenPositioner.GetCenterRect(pause.GetDesiredSize(sharpGui)));

        sharpGui.Text(pause);

        if (sharpGui.GamepadButtonEntered[0] == GamepadButtonCode.XInput_Start)
        {
            updateTimer.Live = true;
        }
    }
}
