﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Platform
{
    /// <summary>
    /// The id 1-4 of the gamepads
    /// </summary>
    public enum GamepadId
    {
        Pad1 = 0,
        Pad2 = 1,
        Pad3 = 2,
        Pad4 = 3
    }

    /// <summary>
    /// Provides button number codes for buttons.
    /// </summary>
    public enum GamepadButtonCode : int
    {
        Button0 = 0,   XInput_A = 0,
        Button1 = 1,   XInput_B = 1,
        Button2 = 2,   XInput_X = 2,
        Button3 = 3,   XInput_Y = 3,
        Button4 = 4,   XInput_LeftShoulder = 4,
        Button5 = 5,   XInput_RightShoulder = 5,
        Button6 = 6,   XInput_Select = 6,
        Button7 = 7,   XInput_Start = 7,
        Button8 = 8,   XInput_LThumb= 8,
        Button9 = 9,   XInput_RThumb = 9,
        Button10 = 10, XInput_DPadUp = 10,
        Button11 = 11, XInput_DPadDown = 11,
        Button12 = 12, XInput_DPadLeft = 12,
        Button13 = 13, XInput_DPadRight = 13,
        Button14 = 14, XInput_Guide = 14,
        Button15 = 15, XInput_C = 15,
        Button16 = 16, XInput_Z = 16,
        Button17 = 17, XInput_LTrigger = 17,
        Button18 = 18, XInput_RTrigger = 18,
        NUM_BUTTONS = 19,
    };

    /// <summary>
    /// Provides mask codes for mouse buttons.
    /// </summary>
    public enum MouseButtonCode : int
    {
	    MB_BUTTON0 = 0, 
	    MB_BUTTON1 = 1, 
	    MB_BUTTON2 = 2,
	    MB_BUTTON3 = 3, 
	    MB_BUTTON4 = 4,	
	    MB_BUTTON5 = 5, 
	    MB_BUTTON6 = 6,	
	    MB_BUTTON7 = 7,
        NUM_BUTTONS = 8,
    };

    /// <summary>
    /// Provides the direction for the mouse wheel. None is 0 so you can check for
    /// presence with > 0.
    /// </summary>
    public enum MouseWheelDirection : int
    {
        None = 0,
        Up = 1,
        Down = 2
    };

    /// <summary>
    /// Masks for the modifier keys on the keyboard.
    /// </summary>
    public enum Modifier : int
    {
	    Shift = 0x0000001,
	    Ctrl  = 0x0000010,
	    Alt   = 0x0000100
    };

    /// <summary>
    /// Keyboard button codes.
    /// </summary>
    public enum KeyboardButtonCode : int
    {
	    KC_UNASSIGNED  = 0x00,
	    KC_ESCAPE      = 0x01,
	    KC_1           = 0x02,
	    KC_2           = 0x03,
	    KC_3           = 0x04,
	    KC_4           = 0x05,
	    KC_5           = 0x06,
	    KC_6           = 0x07,
	    KC_7           = 0x08,
	    KC_8           = 0x09,
	    KC_9           = 0x0A,
	    KC_0           = 0x0B,
	    KC_MINUS       = 0x0C,    // - on main keyboard
	    KC_EQUALS      = 0x0D,
	    KC_BACK        = 0x0E,    // backspace
	    KC_TAB         = 0x0F,
	    KC_Q           = 0x10,
	    KC_W           = 0x11,
	    KC_E           = 0x12,
	    KC_R           = 0x13,
	    KC_T           = 0x14,
	    KC_Y           = 0x15,
	    KC_U           = 0x16,
	    KC_I           = 0x17,
	    KC_O           = 0x18,
	    KC_P           = 0x19,
	    KC_LBRACKET    = 0x1A,
	    KC_RBRACKET    = 0x1B,
	    KC_RETURN      = 0x1C,    // Enter on main keyboard
	    KC_LCONTROL    = 0x1D,
	    KC_A           = 0x1E,
	    KC_S           = 0x1F,
	    KC_D           = 0x20,
	    KC_F           = 0x21,
	    KC_G           = 0x22,
	    KC_H           = 0x23,
	    KC_J           = 0x24,
	    KC_K           = 0x25,
	    KC_L           = 0x26,
	    KC_SEMICOLON   = 0x27,
	    KC_APOSTROPHE  = 0x28,
	    KC_GRAVE       = 0x29,    // accent
	    KC_LSHIFT      = 0x2A,
	    KC_BACKSLASH   = 0x2B,
	    KC_Z           = 0x2C,
	    KC_X           = 0x2D,
	    KC_C           = 0x2E,
	    KC_V           = 0x2F,
	    KC_B           = 0x30,
	    KC_N           = 0x31,
	    KC_M           = 0x32,
	    KC_COMMA       = 0x33,
	    KC_PERIOD      = 0x34,    // . on main keyboard
	    KC_SLASH       = 0x35,    // / on main keyboard
	    KC_RSHIFT      = 0x36,
	    KC_MULTIPLY    = 0x37,    // * on numeric keypad
	    KC_LMENU       = 0x38,    // left Alt
	    KC_SPACE       = 0x39,
	    KC_CAPITAL     = 0x3A,
	    KC_F1          = 0x3B,
	    KC_F2          = 0x3C,
	    KC_F3          = 0x3D,
	    KC_F4          = 0x3E,
	    KC_F5          = 0x3F,
	    KC_F6          = 0x40,
	    KC_F7          = 0x41,
	    KC_F8          = 0x42,
	    KC_F9          = 0x43,
	    KC_F10         = 0x44,
	    KC_NUMLOCK     = 0x45,
	    KC_SCROLL      = 0x46,    // Scroll Lock
	    KC_NUMPAD7     = 0x47,
	    KC_NUMPAD8     = 0x48,
	    KC_NUMPAD9     = 0x49,
	    KC_SUBTRACT    = 0x4A,    // - on numeric keypad
	    KC_NUMPAD4     = 0x4B,
	    KC_NUMPAD5     = 0x4C,
	    KC_NUMPAD6     = 0x4D,
	    KC_ADD         = 0x4E,    // + on numeric keypad
	    KC_NUMPAD1     = 0x4F,
	    KC_NUMPAD2     = 0x50,
	    KC_NUMPAD3     = 0x51,
	    KC_NUMPAD0     = 0x52,
	    KC_DECIMAL     = 0x53,    // . on numeric keypad
	    KC_OEM_102     = 0x56,    // < > | on UK/Germany keyboards
	    KC_F11         = 0x57,
	    KC_F12         = 0x58,
	    KC_F13         = 0x64,    //                     (NEC PC98)
	    KC_F14         = 0x65,    //                     (NEC PC98)
	    KC_F15         = 0x66,    //                     (NEC PC98)
	    KC_KANA        = 0x70,    // (Japanese keyboard)
	    KC_ABNT_C1     = 0x73,    // / ? on Portugese (Brazilian) keyboards
	    KC_CONVERT     = 0x79,    // (Japanese keyboard)
	    KC_NOCONVERT   = 0x7B,    // (Japanese keyboard)
	    KC_YEN         = 0x7D,    // (Japanese keyboard)
	    KC_ABNT_C2     = 0x7E,    // Numpad . on Portugese (Brazilian) keyboards
	    KC_NUMPADEQUALS= 0x8D,    // = on numeric keypad (NEC PC98)
	    KC_PREVTRACK   = 0x90,    // Previous Track (KC_CIRCUMFLEX on Japanese keyboard)
	    KC_AT          = 0x91,    //                     (NEC PC98)
	    KC_COLON       = 0x92,    //                     (NEC PC98)
	    KC_UNDERLINE   = 0x93,    //                     (NEC PC98)
	    KC_KANJI       = 0x94,    // (Japanese keyboard)
	    KC_STOP        = 0x95,    //                     (NEC PC98)
	    KC_AX          = 0x96,    //                     (Japan AX)
	    KC_UNLABELED   = 0x97,    //                        (J3100)
	    KC_NEXTTRACK   = 0x99,    // Next Track
	    KC_NUMPADENTER = 0x9C,    // Enter on numeric keypad
	    KC_RCONTROL    = 0x9D,
	    KC_MUTE        = 0xA0,    // Mute
	    KC_CALCULATOR  = 0xA1,    // Calculator
	    KC_PLAYPAUSE   = 0xA2,    // Play / Pause
	    KC_MEDIASTOP   = 0xA4,    // Media Stop
	    KC_VOLUMEDOWN  = 0xAE,    // Volume -
	    KC_VOLUMEUP    = 0xB0,    // Volume +
	    KC_WEBHOME     = 0xB2,    // Web home
	    KC_NUMPADCOMMA = 0xB3,    // , on numeric keypad (NEC PC98)
	    KC_DIVIDE      = 0xB5,    // / on numeric keypad
	    KC_SYSRQ       = 0xB7,
	    KC_RMENU       = 0xB8,    // right Alt
	    KC_PAUSE       = 0xC5,    // Pause
	    KC_HOME        = 0xC7,    // Home on arrow keypad
	    KC_UP          = 0xC8,    // UpArrow on arrow keypad
	    KC_PGUP        = 0xC9,    // PgUp on arrow keypad
	    KC_LEFT        = 0xCB,    // LeftArrow on arrow keypad
	    KC_RIGHT       = 0xCD,    // RightArrow on arrow keypad
	    KC_END         = 0xCF,    // End on arrow keypad
	    KC_DOWN        = 0xD0,    // DownArrow on arrow keypad
	    KC_PGDOWN      = 0xD1,    // PgDn on arrow keypad
	    KC_INSERT      = 0xD2,    // Insert on arrow keypad
	    KC_DELETE      = 0xD3,    // Delete on arrow keypad
	    KC_LWIN        = 0xDB,    // Left Windows key
	    KC_RWIN        = 0xDC,    // Right Windows key
	    KC_APPS        = 0xDD,    // AppMenu key
	    KC_POWER       = 0xDE,    // System Power
	    KC_SLEEP       = 0xDF,    // System Sleep
	    KC_WAKE        = 0xE3,    // System Wake
	    KC_WEBSEARCH   = 0xE5,    // Web Search
	    KC_WEBFAVORITES= 0xE6,    // Web Favorites
	    KC_WEBREFRESH  = 0xE7,    // Web Refresh
	    KC_WEBSTOP     = 0xE8,    // Web Stop
	    KC_WEBFORWARD  = 0xE9,    // Web Forward
	    KC_WEBBACK     = 0xEA,    // Web Back
	    KC_MYCOMPUTER  = 0xEB,    // My Computer
	    KC_MAIL        = 0xEC,    // Mail
	    KC_MEDIASELECT = 0xED,     // Media Select

        None = KC_UNASSIGNED,
    };
}
