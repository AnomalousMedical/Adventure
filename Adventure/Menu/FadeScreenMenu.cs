using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

class FadeScreenMenu
(
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    IClockService clockService,
    IExplorationMenu explorationMenu
)
: IExplorationSubMenu
{
    private const float GreyColor = 0.0f;

    SharpPanel backgroundPanel = new SharpPanel();
    SharpStyle style = new SharpStyle()
    {
        Border = new IntPad(0),
        Background = new Color(GreyColor, GreyColor, GreyColor, 0.0f),
    };

    private float start;
    private float change;
    private float time;
    private float duration;

    private TaskCompletionSource currentTask;

    public Task WaitForCurrentEffect()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource();
        }
        return currentTask.Task;
    }

    public Task ShowAndWait(float fadeStart, float fadeEnd, float durationSeconds, GamepadId gamepad)
    {
        this.start = fadeStart;
        this.change = fadeEnd - fadeStart;
        this.time = 0;
        this.duration = durationSeconds;

        explorationMenu.RequestSubMenu(this, gamepad);

        return WaitForCurrentEffect();
    }

    public void Close()
    {
        explorationMenu.RequestSubMenu(null, GamepadId.Pad1);
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        time += clockService.Clock.DeltaSeconds;
        if(time > duration)
        {
            time = duration;
            AlertFadeComplete(menu, gamepadId);
        }

        var fade = EasingFunctions.None(start, change, time, duration);
        style.Background = new Color(GreyColor, GreyColor, GreyColor, fade);

        backgroundPanel.SetRect(0, 0, screenPositioner.ScreenSize.Width, screenPositioner.ScreenSize.Height);

        sharpGui.Panel(backgroundPanel, style);
    }

    private void AlertFadeComplete(IExplorationMenu menu, GamepadId gamepadId)
    {
        var tempTask = currentTask;
        currentTask = null;
        tempTask?.SetResult();
    }
}
