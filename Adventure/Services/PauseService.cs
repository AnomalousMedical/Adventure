using Engine;
using Engine.Platform;
using SharpGui;
using System;

namespace Adventure.Services;

class PauseService : IDisposable
{
    private KeyboardButtonCode pauseKey;
    private GamepadButtonCode[] pauseButton;

    private SharpText pause;
    private readonly UpdateTimer updateTimer;
    private readonly ISharpGui sharpGui;
    private readonly IScreenPositioner screenPositioner;
    private readonly KeybindService keybindService;

    public PauseService
    (
        UpdateTimer updateTimer, 
        ISharpGui sharpGui, 
        FontLoader fontLoader, 
        IScreenPositioner screenPositioner, 
        KeybindService keybindService
    )
    {
        this.updateTimer = updateTimer;
        this.sharpGui = sharpGui;
        this.screenPositioner = screenPositioner;
        this.keybindService = keybindService;

        pauseButton = keybindService.GetAllGamepadBindings(KeyBindings.Pause);
        pauseKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Pause).KeyboardButton.Value;
        pause = new SharpText("Pause") { Font = fontLoader.TitleFont, Color = Color.UIWhite };

        keybindService.KeybindChanged += KeybindService_KeybindChanged;
    }

    public void Dispose()
    {
        keybindService.KeybindChanged -= KeybindService_KeybindChanged;
    }

    public void UnpausedUpdate()
    {
        if (sharpGui.GamepadButtonEntered[0] == pauseButton[0]
         || sharpGui.GamepadButtonEntered[1] == pauseButton[1]
         || sharpGui.GamepadButtonEntered[2] == pauseButton[2]
         || sharpGui.GamepadButtonEntered[3] == pauseButton[3]
         || sharpGui.KeyEntered == pauseKey
        )
        {
            updateTimer.Live = false;
        }
    }

    public void PausedUpdate()
    {
        pause.SetRect(screenPositioner.GetCenterRect(pause.GetDesiredSize(sharpGui)));

        sharpGui.Text(pause);

        if (sharpGui.GamepadButtonEntered[0] == pauseButton[0]
         || sharpGui.GamepadButtonEntered[1] == pauseButton[1]
         || sharpGui.GamepadButtonEntered[2] == pauseButton[2]
         || sharpGui.GamepadButtonEntered[3] == pauseButton[3]
         || sharpGui.KeyEntered == pauseKey)
        {
            updateTimer.Live = true;
        }
    }

    private void KeybindService_KeybindChanged(KeybindService service, KeyBindings binding)
    {
        switch (binding)
        {
            case KeyBindings.Pause:
                pauseButton = keybindService.GetAllGamepadBindings(binding);
                pauseKey = keybindService.GetKeyboardMouseBinding(binding).KeyboardButton.Value;
                break;
        }
    }
}
