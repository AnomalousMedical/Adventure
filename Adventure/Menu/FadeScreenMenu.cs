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
    IClockService clockService
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

    private IExplorationMenu explorationMenu;

    private float start;
    private float change;
    private float time;
    private float duration;
    private IExplorationSubMenu wrappedMenu;
    private bool tickThisFrame; //This is used to force 1 frame at the start value

    private TaskCompletionSource currentTask;

    public void Link(IExplorationMenu explorationMenu)
    {
        this.explorationMenu = explorationMenu;
    }

    public Task WaitForCurrentEffect()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource();
        }
        return currentTask.Task;
    }

    public void Show(float fadeStart, float fadeEnd, float durationSeconds, GamepadId gamepad, IExplorationSubMenu wrappedMenu = null)
    {
        this.start = fadeStart;
        this.change = fadeEnd - fadeStart;
        this.time = 0;
        this.duration = durationSeconds;
        this.wrappedMenu = wrappedMenu;
        this.tickThisFrame = false;

        explorationMenu.RequestSubMenu(this, gamepad);
    }

    public Task ShowAndWait(float fadeStart, float fadeEnd, float durationSeconds, GamepadId gamepad, IExplorationSubMenu wrappedMenu = null)
    {
        Show(fadeStart, fadeEnd, durationSeconds, gamepad, wrappedMenu);
        return WaitForCurrentEffect();
    }

    public async Task ShowAndWaitAndClose(float fadeStart, float fadeEnd, float durationSeconds, GamepadId gamepad, IExplorationSubMenu wrappedMenu = null)
    {
        await ShowAndWait(fadeStart, fadeEnd, durationSeconds, gamepad, wrappedMenu);
        Close();
    }

    public void Close()
    {
        explorationMenu.RequestSubMenu(null, GamepadId.Pad1);
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        bool drawPanel = true;
        if (tickThisFrame)
        {
            time += clockService.Clock.DeltaSeconds;
            if (time > duration)
            {
                time = duration;
                AlertFadeComplete(menu, gamepadId);

                drawPanel = wrappedMenu == null;
            }
        }
        else
        {
            tickThisFrame = true;
        }

        if (drawPanel)
        {
            var fade = EasingFunctions.None(start, change, time, duration);
            style.Background = new Color(GreyColor, GreyColor, GreyColor, fade);

            backgroundPanel.SetRect(0, 0, screenPositioner.ScreenSize.Width, screenPositioner.ScreenSize.Height);

            sharpGui.Panel(backgroundPanel, style);
        }
        else
        {
            wrappedMenu.Update(menu, gamepadId);
        }
    }

    private void AlertFadeComplete(IExplorationMenu menu, GamepadId gamepadId)
    {
        var tempTask = currentTask;
        currentTask = null;
        tempTask?.SetResult();
    }
}
