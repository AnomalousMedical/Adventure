using Anomalous.Interop;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Adventure.Services;

enum KeyBindings
{
    Confirm,
    Cancel,
    Previous,
    Next,
    SwitchCharacter,
    ActiveAction,
    Up,
    Down,
    Left,
    Right,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    OpenMenu
}

class KeybindService
{
    public event Action<KeybindService, KeyBindings> KeybindChanged;

    //This dictionary must have all KeyBindings entries to be correct
    private Dictionary<KeyBindings, KeyboardMouseBinding> defaultKeys = new Dictionary<KeyBindings, KeyboardMouseBinding>()
    {
        { KeyBindings.Confirm, new KeyboardMouseBinding(KeyboardButtonCode.KC_RETURN) },
        { KeyBindings.Cancel, new KeyboardMouseBinding(KeyboardButtonCode.KC_ESCAPE) },
        { KeyBindings.Previous, new KeyboardMouseBinding(KeyboardButtonCode.KC_PGDOWN) },
        { KeyBindings.Next, new KeyboardMouseBinding(KeyboardButtonCode.KC_PGUP) },
        { KeyBindings.SwitchCharacter, new KeyboardMouseBinding(KeyboardButtonCode.KC_LSHIFT) },
        { KeyBindings.ActiveAction, new KeyboardMouseBinding(KeyboardButtonCode.KC_SPACE) },
        { KeyBindings.Up, new KeyboardMouseBinding(KeyboardButtonCode.KC_UP) },
        { KeyBindings.Down, new KeyboardMouseBinding(KeyboardButtonCode.KC_DOWN) },
        { KeyBindings.Left, new KeyboardMouseBinding(KeyboardButtonCode.KC_LEFT) },
        { KeyBindings.Right, new KeyboardMouseBinding(KeyboardButtonCode.KC_RIGHT) },
        { KeyBindings.MoveUp, new KeyboardMouseBinding(KeyboardButtonCode.KC_W) },
        { KeyBindings.MoveDown, new KeyboardMouseBinding(KeyboardButtonCode.KC_S) },
        { KeyBindings.MoveLeft, new KeyboardMouseBinding(KeyboardButtonCode.KC_A) },
        { KeyBindings.MoveRight, new KeyboardMouseBinding(KeyboardButtonCode.KC_D) },
        { KeyBindings.OpenMenu, new KeyboardMouseBinding(KeyboardButtonCode.KC_TAB) },
    };

    private Dictionary<KeyBindings, GamepadButtonCode> defaultButtons = new Dictionary<KeyBindings, GamepadButtonCode>()
    {
        { KeyBindings.Confirm, GamepadButtonCode.XInput_A },
        { KeyBindings.Cancel, GamepadButtonCode.XInput_B },
        { KeyBindings.Previous, GamepadButtonCode.XInput_LTrigger },
        { KeyBindings.Next, GamepadButtonCode.XInput_RTrigger },
        { KeyBindings.SwitchCharacter, GamepadButtonCode.XInput_Y },
        { KeyBindings.ActiveAction, GamepadButtonCode.XInput_RTrigger },
        { KeyBindings.Up, GamepadButtonCode.XInput_DPadUp },
        { KeyBindings.Down, GamepadButtonCode.XInput_DPadDown },
        { KeyBindings.Left, GamepadButtonCode.XInput_DPadLeft },
        { KeyBindings.Right, GamepadButtonCode.XInput_DPadRight },
        //{ KeyBindings.MoveUp, GamepadButtonCode },
        //{ KeyBindings.MoveDown, GamepadButtonCode },
        //{ KeyBindings.MoveLeft, GamepadButtonCode },
        //{ KeyBindings.MoveRight, GamepadButtonCode },
        { KeyBindings.OpenMenu, GamepadButtonCode.XInput_Y },
    };
    private readonly GameOptions options;
    private readonly ISharpGui sharpGui;

    public KeybindService(GameOptions options, ISharpGui sharpGui)
    {
        this.options = options;
        this.sharpGui = sharpGui;

        sharpGui.OverrideStandardBack(GetKeyboardMouseBinding(KeyBindings.Cancel).KeyboardButton.Value, GetAllGamepadBindings(KeyBindings.Cancel));
        sharpGui.OverrideStandardPrevious(GetKeyboardMouseBinding(KeyBindings.Previous).KeyboardButton.Value, GetAllGamepadBindings(KeyBindings.Previous));
        sharpGui.OverrideStandardNext(GetKeyboardMouseBinding(KeyBindings.Next).KeyboardButton.Value, GetAllGamepadBindings(KeyBindings.Next));
        sharpGui.OverrideStandardAccept(GetKeyboardMouseBinding(KeyBindings.Confirm).KeyboardButton.Value, GetAllGamepadBindings(KeyBindings.Confirm));
    }

    public void SetBinding(KeyBindings binding, KeyboardMouseBinding key)
    {
        options.KeyboardBindings[binding] = key;
        FireKeybindChanged(binding);
    }

    public void SetBinding(KeyBindings binding, GamepadId gamepadId, GamepadButtonCode button)
    {
        options.GamepadBindings[(int)gamepadId][binding] = button;
        FireKeybindChanged(binding);
    }

    private void FireKeybindChanged(KeyBindings binding)
    {
        KeybindChanged?.Invoke(this, binding);
        switch (binding)
        {
            case KeyBindings.Cancel:
                sharpGui.OverrideStandardBack(GetKeyboardMouseBinding(binding).KeyboardButton.Value, GetAllGamepadBindings(binding));
                break;
            case KeyBindings.Previous:
                sharpGui.OverrideStandardPrevious(GetKeyboardMouseBinding(binding).KeyboardButton.Value, GetAllGamepadBindings(binding));
                break;
            case KeyBindings.Next:
                sharpGui.OverrideStandardNext(GetKeyboardMouseBinding(binding).KeyboardButton.Value, GetAllGamepadBindings(binding));
                break;
            case KeyBindings.Confirm:
                sharpGui.OverrideStandardAccept(GetKeyboardMouseBinding(binding).KeyboardButton.Value, GetAllGamepadBindings(binding));
                break;
        }
    }

    public KeyboardMouseBinding GetKeyboardMouseBinding(KeyBindings binding)
    {
        if (options.KeyboardBindings.TryGetValue(binding, out var keyBind))
        {
            return keyBind;
        }
        return defaultKeys[binding];
    }

    public KeyboardButtonCode[] GetKeyboardBinding(KeyBindings binding)
    {
        var keyBind = GetKeyboardMouseBinding(binding);
        if (keyBind.KeyboardButton != null)
        {
            return new KeyboardButtonCode[] { keyBind.KeyboardButton.Value };
        }
        return null;
    }

    public MouseButtonCode[] GetMouseBinding(KeyBindings binding)
    {
        var keyBind = GetKeyboardMouseBinding(binding);
        if (keyBind.MouseButton != null)
        {
            return new MouseButtonCode[] { keyBind.MouseButton.Value };
        }
        return null;
    }

    public GamepadButtonCode GetGamepadBinding(KeyBindings binding, GamepadId gamepadId)
    {
        if (options.GamepadBindings[(int)gamepadId].TryGetValue(binding, out var keyBind))
        {
            return keyBind;
        }
        return defaultButtons[binding];
    }

    public GamepadButtonCode[] GetGamepadBindingArray(KeyBindings binding, GamepadId gamepadId)
    {
        return new GamepadButtonCode[] { GetGamepadBinding(binding, gamepadId) };
    }

    public GamepadButtonCode[] GetAllGamepadBindings(KeyBindings binding)
    {
        return [
            GetGamepadBinding(binding, GamepadId.Pad1),
            GetGamepadBinding(binding, GamepadId.Pad2),
            GetGamepadBinding(binding, GamepadId.Pad3),
            GetGamepadBinding(binding, GamepadId.Pad4),
        ];
    }

    public IEnumerable<KeyBindings> GetKeyBindings()
    {
        yield return KeyBindings.Confirm;
        yield return KeyBindings.Cancel;
        yield return KeyBindings.Previous;
        yield return KeyBindings.Next;
        yield return KeyBindings.SwitchCharacter;
        yield return KeyBindings.ActiveAction;
        yield return KeyBindings.Up;
        yield return KeyBindings.Down;
        yield return KeyBindings.Left;
        yield return KeyBindings.Right;
        yield return KeyBindings.MoveUp;
        yield return KeyBindings.MoveDown;
        yield return KeyBindings.MoveLeft;
        yield return KeyBindings.MoveRight;
        yield return KeyBindings.OpenMenu;
    }
}
