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
            if (sharpGui.GamepadButtonEntered[0] == Engine.Platform.GamepadButtonCode.XInput_Y || sharpGui.KeyEntered == openMenuKeyboard)
            {
                RequestSubMenu(rootMenu, GamepadId.Pad1);
                handled = true;
            }
            else if (sharpGui.GamepadButtonEntered[1] == Engine.Platform.GamepadButtonCode.XInput_Y)
            {
                RequestSubMenu(rootMenu, GamepadId.Pad2);
                handled = true;
            }
            else if (sharpGui.GamepadButtonEntered[2] == Engine.Platform.GamepadButtonCode.XInput_Y)
            {
                RequestSubMenu(rootMenu, GamepadId.Pad3);
                handled = true;
            }
            else if (sharpGui.GamepadButtonEntered[3] == Engine.Platform.GamepadButtonCode.XInput_Y)
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
