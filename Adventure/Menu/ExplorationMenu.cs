using Adventure.Services;
using Engine.Platform;
using SharpGui;
using System;

namespace Adventure.Menu;

interface IExplorationMenu
{
    IDebugGui DebugGui { get; }
    IRootMenu RootMenu { get; }
    bool Handled { get; }

    void RequestSubMenu(IExplorationSubMenu subMenu, GamepadId gamepad);
    bool Update();
}

class ExplorationMenu : IExplorationMenu, IDisposable
{
    private readonly ISharpGui sharpGui;
    private readonly IDebugGui debugGui;
    private readonly IRootMenu rootMenu;
    private readonly KeybindService keybindService;

    private IExplorationSubMenu currentMenu = null;
    private GamepadId currentGamepad;
    private KeyboardButtonCode openMenuKeyboard;
    private GamepadButtonCode[] openMenuGamepad;

    public IDebugGui DebugGui => debugGui;

    public IRootMenu RootMenu => rootMenu;

    bool handled;
    public bool Handled => handled;

    public ExplorationMenu
    (
        ISharpGui sharpGui,
        IDebugGui debugGui,
        IRootMenu rootMenu,
        KeybindService keybindService
    )
    {
        this.sharpGui = sharpGui;
        this.debugGui = debugGui;
        this.rootMenu = rootMenu;
        this.keybindService = keybindService;

        openMenuKeyboard = keybindService.GetKeyboardMouseBinding(KeyBindings.OpenMenu).KeyboardButton.Value;
        openMenuGamepad = [
            keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad1),
            keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad2),
            keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad3),
            keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad4),
        ];

        keybindService.KeybindChanged += KeybindService_KeybindChanged;
    }

    public void Dispose()
    {
        keybindService.KeybindChanged -= KeybindService_KeybindChanged;
    }

    private void KeybindService_KeybindChanged(KeybindService service, KeyBindings binding)
    {
        switch (binding)
        {
            case KeyBindings.OpenMenu:
                openMenuKeyboard = service.GetKeyboardMouseBinding(binding).KeyboardButton.Value;
                openMenuGamepad[0] = keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad1);
                openMenuGamepad[1] = keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad2);
                openMenuGamepad[2] = keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad3);
                openMenuGamepad[3] = keybindService.GetGamepadBinding(KeyBindings.OpenMenu, GamepadId.Pad4);
                break;
        }
    }

    /// <summary>
    /// Update the menu. Returns true if something was done. False if nothing was done and the menu wasn't shown
    /// </summary>
    /// <returns></returns>
    public bool Update()
    {
        handled = false;
        if (currentMenu != null)
        {
            handled = true;
            currentMenu.Update(this, currentGamepad);
        }
        else
        {
            if (sharpGui.GamepadButtonEntered[0] == openMenuGamepad[0] || sharpGui.KeyEntered == openMenuKeyboard)
            {
                RequestSubMenu(rootMenu, GamepadId.Pad1);
                handled = true;
            }
            else if (sharpGui.GamepadButtonEntered[1] == openMenuGamepad[1])
            {
                RequestSubMenu(rootMenu, GamepadId.Pad2);
                handled = true;
            }
            else if (sharpGui.GamepadButtonEntered[2] == openMenuGamepad[2])
            {
                RequestSubMenu(rootMenu, GamepadId.Pad3);
                handled = true;
            }
            else if (sharpGui.GamepadButtonEntered[3] == openMenuGamepad[3])
            {
                RequestSubMenu(rootMenu, GamepadId.Pad4);
                handled = true;
            }
        }
        return handled;
    }

    public void RequestSubMenu(IExplorationSubMenu subMenu, GamepadId gamepad)
    {
        currentMenu = subMenu;
        currentGamepad = gamepad;
    }
}
