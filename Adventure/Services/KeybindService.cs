using Anomalous.Interop;
using Engine.Platform;
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

class KeybindService(GameOptions options)
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

    public void SetBinding(KeyBindings binding, KeyboardMouseBinding key)
    {
        options.KeyboardBindings[binding] = key;
        KeybindChanged?.Invoke(this, binding);
    }

    public KeyboardMouseBinding GetKeyboardMouseBinding(KeyBindings binding)
    {
        if(options.KeyboardBindings.TryGetValue(binding, out var keyBind))
        {
            return keyBind;
        }
        return defaultKeys[binding];
    }

    public KeyboardButtonCode[] GetKeyboardBinding(KeyBindings binding)
    {
        var keyBind = GetKeyboardMouseBinding(binding);
        if(keyBind.KeyboardButton != null)
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
