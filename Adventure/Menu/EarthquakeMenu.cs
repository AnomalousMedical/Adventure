using Adventure.Assets.SoundEffects;
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

internal class EarthquakeMenu
(
    IClockService clockService,
    IExplorationMenu explorationMenu,
    CameraMover cameraMover,
    ISoundEffectPlayer soundEffectPlayer,
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner
) : IExplorationSubMenu
{
    private const float ShakeDelta = 0.1f;
    private const float ShakeXOffset = 0.05f;

    private float time;
    private float duration;
    private float shakeTime;
    private Vector3 cameraPos;
    private Quaternion cameraRot;

    private TaskCompletionSource currentTask;

    private SharpButton skipButton = new SharpButton() { Text = "Skip" };

    public Task WaitForCurrentEffect()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource();
        }
        return currentTask.Task;
    }

    public void Show(in Vector3 cameraPos, in Quaternion cameraRot, GamepadId gamepad)
    {
        this.cameraPos = cameraPos;
        this.cameraRot = cameraRot;
        this.time = 0;
        this.duration = 3.0f;
        this.shakeTime = ShakeDelta;

        soundEffectPlayer.PlaySound(EarthquakeSoundEffect.Instance);

        explorationMenu.RequestSubMenu(this, gamepad);
    }

    public Task ShowAndWait(in Vector3 cameraPos, in Quaternion cameraRot, GamepadId gamepad)
    {
        Show(cameraPos, cameraRot, gamepad);
        return WaitForCurrentEffect();
    }

    public async Task ShowAndWaitAndClose(GamepadId gamepad)
    {
        await ShowAndWait(cameraMover.CameraPosition, cameraMover.CameraOrientation, gamepad);
        Close();
    }

    public async Task ShowAndWaitAndClose(Vector3 cameraPos, Quaternion cameraRot, GamepadId gamepad)
    {
        await ShowAndWait(cameraPos, cameraRot, gamepad);
        Close();
    }

    public void Close()
    {
        explorationMenu.RequestSubMenu(null, GamepadId.Pad1);
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        time += clockService.Clock.DeltaSeconds;
        if(time > shakeTime)
        {
            Vector3 offset;
            if(cameraMover.CameraPosition.x > cameraPos.x)
            {
                offset = new Vector3(-ShakeXOffset, 0f, 0f);
            }
            else
            {
                offset = new Vector3(ShakeXOffset, 0f, 0f);
            }
            cameraMover.SetPosition(cameraPos + offset, cameraRot);
            this.shakeTime += ShakeDelta;
        }

        var layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), skipButton);
        layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

        if (sharpGui.Button(skipButton, gamepadId))
        {
            time = duration + 1;
        }

        if (time > duration)
        {
            cameraMover.SetPosition(cameraPos, cameraRot);
            time = duration;
            AlertFadeComplete(menu, gamepadId);
        }
    }

    private void AlertFadeComplete(IExplorationMenu menu, GamepadId gamepadId)
    {
        var tempTask = currentTask;
        currentTask = null;
        tempTask?.SetResult();
    }
}
