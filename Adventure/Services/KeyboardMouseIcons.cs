using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class KeyboardMouseIcons
{
    private ImageTexture icons;
    public ImageTexture Icons
    {
        get
        {
            if (icons == null)
            {
                icons = imageManager.Load("Graphics/Icons/kf_prompts_kb_2048.png");
            }
            return icons;
        }
    }

    private const int IconWidth = 128;
    private const int IconHeight = 128;
    private const float ImageWidth = 2048f;
    private const float ImageHeight = 1024f;
    private const float IconSizeU = IconWidth / ImageWidth;
    private const float IconSizeV = IconHeight / ImageHeight;

    static Rect CreateRect(int x, int y)
    {
        return new Rect(x * IconWidth / ImageWidth, y * IconHeight / ImageHeight, IconSizeU, IconSizeV);
    }

    public Rect Esc { get; } = CreateRect(0, 0);
    public Rect F1 { get; } = CreateRect(1, 0);
    public Rect F2 { get; } = CreateRect(2, 0);
    public Rect F3 { get; } = CreateRect(3, 0);
    public Rect F4 { get; } = CreateRect(4, 0);
    public Rect F5 { get; } = CreateRect(5, 0);
    public Rect F6 { get; } = CreateRect(6, 0);
    public Rect F7 { get; } = CreateRect(7, 0);
    public Rect F8 { get; } = CreateRect(8, 0);
    public Rect F9 { get; } = CreateRect(9, 0);
    public Rect F10 { get; } = CreateRect(10, 0);
    public Rect F11 { get; } = CreateRect(11, 0);
    public Rect F12 { get; } = CreateRect(12, 0);
    public Rect Prtsc { get; } = CreateRect(13, 0);
    public Rect Scrlk { get; } = CreateRect(14, 0);
    public Rect Pause { get; } = CreateRect(15, 0);

    public Rect Tilde { get; } = CreateRect(0, 1);
    public Rect NumRow1 { get; } = CreateRect(1, 1);
    public Rect NumRow2 { get; } = CreateRect(2, 1);
    public Rect NumRow3 { get; } = CreateRect(3, 1);
    public Rect NumRow4 { get; } = CreateRect(4, 1);
    public Rect NumRow5 { get; } = CreateRect(5, 1);
    public Rect NumRow6 { get; } = CreateRect(6, 1);
    public Rect NumRow7 { get; } = CreateRect(7, 1);
    public Rect NumRow8 { get; } = CreateRect(8, 1);
    public Rect NumRow9 { get; } = CreateRect(9, 1);
    public Rect NumRow0 { get; } = CreateRect(10, 1);
    public Rect Minus { get; } = CreateRect(11, 1);
    public Rect Equal { get; } = CreateRect(12, 1);
    public Rect Backspace { get; } = CreateRect(13, 1);
    public Rect Insert { get; } = CreateRect(14, 1);
    public Rect Home { get; } = CreateRect(15, 1);

    public Rect Tab { get; } = CreateRect(0, 2);
    public Rect Q { get; } = CreateRect(1, 2);
    public Rect W { get; } = CreateRect(2, 2);
    public Rect E { get; } = CreateRect(3, 2);
    public Rect R { get; } = CreateRect(4, 2);
    public Rect T { get; } = CreateRect(5, 2);
    public Rect Y { get; } = CreateRect(6, 2);
    public Rect U { get; } = CreateRect(7, 2);
    public Rect I { get; } = CreateRect(8, 2);
    public Rect O { get; } = CreateRect(9, 2);
    public Rect P { get; } = CreateRect(10, 2);
    public Rect LBracket { get; } = CreateRect(11, 2);
    public Rect RBracket { get; } = CreateRect(12, 2);
    public Rect Backslash { get; } = CreateRect(13, 2);
    public Rect Del { get; } = CreateRect(14, 2);
    public Rect End { get; } = CreateRect(15, 2);

    public Rect CapsLock { get; } = CreateRect(0, 3);
    public Rect A { get; } = CreateRect(1, 3);
    public Rect S { get; } = CreateRect(2, 3);
    public Rect D { get; } = CreateRect(3, 3);
    public Rect F { get; } = CreateRect(4, 3);
    public Rect G { get; } = CreateRect(5, 3);
    public Rect H { get; } = CreateRect(6, 3);
    public Rect J { get; } = CreateRect(7, 3);
    public Rect K { get; } = CreateRect(8, 3);
    public Rect L { get; } = CreateRect(9, 3);
    public Rect Semicolin { get; } = CreateRect(10, 3);
    public Rect Quotes { get; } = CreateRect(11, 3);
    public Rect Enter { get; } = CreateRect(12, 3);
    public Rect Up { get; } = CreateRect(13, 3);
    public Rect PgUp { get; } = CreateRect(14, 3);
    public Rect PgDown { get; } = CreateRect(15, 3);

    public Rect Shift { get; } = CreateRect(0, 4);
    public Rect Z { get; } = CreateRect(1, 4);
    public Rect X { get; } = CreateRect(2, 4);
    public Rect C { get; } = CreateRect(3, 4);
    public Rect V { get; } = CreateRect(4, 4);
    public Rect B { get; } = CreateRect(5, 4);
    public Rect N { get; } = CreateRect(6, 4);
    public Rect M { get; } = CreateRect(7, 4);
    public Rect LessThan { get; } = CreateRect(8, 4);
    public Rect GreaterThan { get; } = CreateRect(9, 4);
    public Rect Question { get; } = CreateRect(10, 4);
    public Rect LShift { get; } = CreateRect(11, 4);
    public Rect RShift { get; } = CreateRect(12, 4);
    public Rect Left { get; } = CreateRect(13, 4);
    public Rect Down { get; } = CreateRect(14, 4);
    public Rect Right { get; } = CreateRect(15, 4);

    public Rect Ctrl { get; } = CreateRect(0, 5);
    public Rect Alt { get; } = CreateRect(1, 5);
    public Rect Space { get; } = CreateRect(2, 5);
    public Rect LAlt { get; } = CreateRect(3, 5);
    public Rect LCtrl { get; } = CreateRect(4, 5);
    public Rect RAlt { get; } = CreateRect(5, 5);
    public Rect RCtrl { get; } = CreateRect(6, 5);
    public Rect NumLock { get; } = CreateRect(7, 5);
    public Rect NumDivide { get; } = CreateRect(8, 5);
    public Rect NumMultiply { get; } = CreateRect(9, 5);
    public Rect NumMinus { get; } = CreateRect(10, 5);
    public Rect Num7 { get; } = CreateRect(11, 5);
    public Rect Num8 { get; } = CreateRect(12, 5);
    public Rect Num9 { get; } = CreateRect(13, 5);
    public Rect NumPlus { get; } = CreateRect(14, 5);

    public Rect Num4 { get; } = CreateRect(11, 6);
    public Rect Num5 { get; } = CreateRect(12, 6);
    public Rect Num6 { get; } = CreateRect(13, 6);
    public Rect NumEnter { get; } = CreateRect(14, 6);

    public Rect MouseLeft { get; } = CreateRect(0, 7);
    public Rect MouseRight { get; } = CreateRect(1, 7);
    public Rect Mouse3 { get; } = CreateRect(2, 7);
    public Rect MouseWheelUp { get; } = CreateRect(3, 7);
    public Rect MouseWheelDown { get; } = CreateRect(4, 7);
    public Rect Mouse4 { get; } = CreateRect(5, 7);
    public Rect Mouse5 { get; } = CreateRect(6, 7);
    public Rect Mouse6 { get; } = CreateRect(7, 7);
    public Rect Missing { get; } = CreateRect(8, 7);
    //Space
    public Rect Num1 { get; } = CreateRect(11, 7);
    public Rect Num2 { get; } = CreateRect(12, 7);
    public Rect Num3 { get; } = CreateRect(13, 7);
    public Rect Num0 { get; } = CreateRect(14, 7);
    public Rect NumDot { get; } = CreateRect(15, 7);

    private IImageManager imageManager;
    private Dictionary<KeyboardButtonCode, Rect> keyRects;
    private Dictionary<MouseButtonCode, Rect> mouseRects;

    public Rect GetButtonRect(KeyboardButtonCode code)
    {
        if(keyRects.TryGetValue(code, out var val))
        {
            return val;
        }
        return Missing;
    }

    public Rect GetButtonRect(MouseButtonCode code)
    {
        if (mouseRects.TryGetValue(code, out var val))
        {
            return val;
        }
        return Missing;
    }

    public KeyboardMouseIcons(IImageManager imageManager)
    {
        this.imageManager = imageManager;

        keyRects = new Dictionary<KeyboardButtonCode, Rect>()
        {
            //{ KeyboardButtonCode.KC_UNASSIGNED, },
            { KeyboardButtonCode.KC_ESCAPE, Esc },
            { KeyboardButtonCode.KC_1, NumRow1 },
            { KeyboardButtonCode.KC_2, NumRow2 },
            { KeyboardButtonCode.KC_3, NumRow3 },
            { KeyboardButtonCode.KC_4, NumRow4 },
            { KeyboardButtonCode.KC_5, NumRow5 },
            { KeyboardButtonCode.KC_6, NumRow6 },
            { KeyboardButtonCode.KC_7, NumRow7 },
            { KeyboardButtonCode.KC_8, NumRow8 },
            { KeyboardButtonCode.KC_9, NumRow9 },
            { KeyboardButtonCode.KC_0, NumRow0 },
            { KeyboardButtonCode.KC_MINUS, Minus },    // - on main keyboard
            { KeyboardButtonCode.KC_EQUALS, Equal },
            { KeyboardButtonCode.KC_BACK, Backspace },    // backspace
            { KeyboardButtonCode.KC_TAB, Tab },
            { KeyboardButtonCode.KC_Q, Q },
            { KeyboardButtonCode.KC_W, W },
            { KeyboardButtonCode.KC_E, E },
            { KeyboardButtonCode.KC_R, R },
            { KeyboardButtonCode.KC_T, T },
            { KeyboardButtonCode.KC_Y, Y },
            { KeyboardButtonCode.KC_U, U },
            { KeyboardButtonCode.KC_I, I },
            { KeyboardButtonCode.KC_O, O },
            { KeyboardButtonCode.KC_P, P },
            { KeyboardButtonCode.KC_LBRACKET, LBracket },
            { KeyboardButtonCode.KC_RBRACKET, RBracket },
            { KeyboardButtonCode.KC_RETURN, Enter },    // Enter on main keyboard
            { KeyboardButtonCode.KC_LCONTROL, LCtrl },
            { KeyboardButtonCode.KC_A, A },
            { KeyboardButtonCode.KC_S, S },
            { KeyboardButtonCode.KC_D, D },
            { KeyboardButtonCode.KC_F, F },
            { KeyboardButtonCode.KC_G, G },
            { KeyboardButtonCode.KC_H, H },
            { KeyboardButtonCode.KC_J, J },
            { KeyboardButtonCode.KC_K, K },
            { KeyboardButtonCode.KC_L, L },
            { KeyboardButtonCode.KC_SEMICOLON, Semicolin },
            { KeyboardButtonCode.KC_APOSTROPHE, Quotes },
            { KeyboardButtonCode.KC_GRAVE, Tilde },    // accent
            { KeyboardButtonCode.KC_LSHIFT, LShift },
            { KeyboardButtonCode.KC_BACKSLASH, Backslash },
            { KeyboardButtonCode.KC_Z, Z },
            { KeyboardButtonCode.KC_X, X },
            { KeyboardButtonCode.KC_C, C },
            { KeyboardButtonCode.KC_V, V },
            { KeyboardButtonCode.KC_B, B },
            { KeyboardButtonCode.KC_N, N },
            { KeyboardButtonCode.KC_M, M },
            { KeyboardButtonCode.KC_COMMA, LessThan },
            { KeyboardButtonCode.KC_PERIOD, GreaterThan },    // . on main keyboard
            { KeyboardButtonCode.KC_SLASH, Question },    // / on main keyboard
            { KeyboardButtonCode.KC_RSHIFT, RShift },
            { KeyboardButtonCode.KC_MULTIPLY, NumMultiply },    // * on numeric keypad
            { KeyboardButtonCode.KC_LMENU, LAlt },    // left Alt
            { KeyboardButtonCode.KC_SPACE, Space },
            { KeyboardButtonCode.KC_CAPITAL, CapsLock },
            { KeyboardButtonCode.KC_F1, F1 },
            { KeyboardButtonCode.KC_F2, F2 },
            { KeyboardButtonCode.KC_F3, F3 },
            { KeyboardButtonCode.KC_F4, F4 },
            { KeyboardButtonCode.KC_F5, F5 },
            { KeyboardButtonCode.KC_F6, F6 },
            { KeyboardButtonCode.KC_F7, F7 },
            { KeyboardButtonCode.KC_F8, F8 },
            { KeyboardButtonCode.KC_F9, F9 },
            { KeyboardButtonCode.KC_F10, F10 },
            { KeyboardButtonCode.KC_NUMLOCK, NumLock },
            { KeyboardButtonCode.KC_SCROLL, Scrlk },    // Scroll Lock
            { KeyboardButtonCode.KC_NUMPAD7, Num7 },
            { KeyboardButtonCode.KC_NUMPAD8, Num8 },
            { KeyboardButtonCode.KC_NUMPAD9, Num9 },
            { KeyboardButtonCode.KC_SUBTRACT, NumMinus },    // - on numeric keypad
            { KeyboardButtonCode.KC_NUMPAD4, Num4 },
            { KeyboardButtonCode.KC_NUMPAD5, Num5 },
            { KeyboardButtonCode.KC_NUMPAD6, Num6 },
            { KeyboardButtonCode.KC_ADD, NumPlus },    // + on numeric keypad
            { KeyboardButtonCode.KC_NUMPAD1, Num1 },
            { KeyboardButtonCode.KC_NUMPAD2, Num2 },
            { KeyboardButtonCode.KC_NUMPAD3, Num3 },
            { KeyboardButtonCode.KC_NUMPAD0, Num0 },
            { KeyboardButtonCode.KC_DECIMAL, NumDot },    // . on numeric keypad
            //{ KeyboardButtonCode.KC_OEM_102, },    // < > | on UK/Germany keyboards
            { KeyboardButtonCode.KC_F11, F11 },
            { KeyboardButtonCode.KC_F12, F12 },
            //{ KeyboardButtonCode.KC_F13, },    //                     (NEC PC98)
            //{ KeyboardButtonCode.KC_F14, },    //                     (NEC PC98)
            //{ KeyboardButtonCode.KC_F15, },    //                     (NEC PC98)
            //{ KeyboardButtonCode.KC_KANA, },    // (Japanese keyboard)
            //{ KeyboardButtonCode.KC_ABNT_C1, },    // / ? on Portugese (Brazilian) keyboards
            //{ KeyboardButtonCode.KC_CONVERT, },    // (Japanese keyboard)
            //{ KeyboardButtonCode.KC_NOCONVERT, },    // (Japanese keyboard)
            //{ KeyboardButtonCode.KC_YEN, },    // (Japanese keyboard)
            //{ KeyboardButtonCode.KC_ABNT_C2, },    // Numpad . on Portugese (Brazilian) keyboards
            //{ KeyboardButtonCode.KC_NUMPADEQUALS, },    // = on numeric keypad (NEC PC98)
            //{ KeyboardButtonCode.KC_PREVTRACK, },    // Previous Track (KC_CIRCUMFLEX on Japanese keyboard)
            //{ KeyboardButtonCode.KC_AT, },    //                     (NEC PC98)
            //{ KeyboardButtonCode.KC_COLON, },    //                     (NEC PC98)
            //{ KeyboardButtonCode.KC_UNDERLINE, },    //                     (NEC PC98)
            //{ KeyboardButtonCode.KC_KANJI, },    // (Japanese keyboard)
            //{ KeyboardButtonCode.KC_STOP, },    //                     (NEC PC98)
            //{ KeyboardButtonCode.KC_AX, },    //                     (Japan AX)
            //{ KeyboardButtonCode.KC_UNLABELED, },    //                        (J3100)
            //{ KeyboardButtonCode.KC_NEXTTRACK, },    // Next Track
            { KeyboardButtonCode.KC_NUMPADENTER, NumEnter },    // Enter on numeric keypad
            { KeyboardButtonCode.KC_RCONTROL, RCtrl },
            //{ KeyboardButtonCode.KC_MUTE, },    // Mute
            //{ KeyboardButtonCode.KC_CALCULATOR, },    // Calculator
            //{ KeyboardButtonCode.KC_PLAYPAUSE, },    // Play / Pause
            //{ KeyboardButtonCode.KC_MEDIASTOP, },    // Media Stop
            //{ KeyboardButtonCode.KC_VOLUMEDOWN, },    // Volume -
            //{ KeyboardButtonCode.KC_VOLUMEUP, },    // Volume +
            //{ KeyboardButtonCode.KC_WEBHOME, },    // Web home
            //{ KeyboardButtonCode.KC_NUMPADCOMMA, },    // , on numeric keypad (NEC PC98)
            { KeyboardButtonCode.KC_DIVIDE, NumDivide },    // / on numeric keypad
            //{ KeyboardButtonCode.KC_SYSRQ, },
            { KeyboardButtonCode.KC_RMENU, RAlt },    // right Alt
            { KeyboardButtonCode.KC_PAUSE, Pause },    // Pause
            { KeyboardButtonCode.KC_HOME, Home },    // Home on arrow keypad
            { KeyboardButtonCode.KC_UP, Up },    // UpArrow on arrow keypad
            { KeyboardButtonCode.KC_PGUP, PgUp },    // PgUp on arrow keypad
            { KeyboardButtonCode.KC_LEFT, Left },    // LeftArrow on arrow keypad
            { KeyboardButtonCode.KC_RIGHT, Right },    // RightArrow on arrow keypad
            { KeyboardButtonCode.KC_END, End },    // End on arrow keypad
            { KeyboardButtonCode.KC_DOWN, Down },    // DownArrow on arrow keypad
            { KeyboardButtonCode.KC_PGDOWN, PgDown },    // PgDn on arrow keypad
            { KeyboardButtonCode.KC_INSERT, Insert },    // Insert on arrow keypad
            { KeyboardButtonCode.KC_DELETE, Del },    // Delete on arrow keypad
            //{ KeyboardButtonCode.KC_LWIN, },    // Left Windows key
            //{ KeyboardButtonCode.KC_RWIN, },    // Right Windows key
            //{ KeyboardButtonCode.KC_APPS, },    // AppMenu key
            //{ KeyboardButtonCode.KC_POWER, },    // System Power
            //{ KeyboardButtonCode.KC_SLEEP, },    // System Sleep
            //{ KeyboardButtonCode.KC_WAKE, },    // System Wake
            //{ KeyboardButtonCode.KC_WEBSEARCH, },    // Web Search
            //{ KeyboardButtonCode.KC_WEBFAVORITES, },    // Web Favorites
            //{ KeyboardButtonCode.KC_WEBREFRESH, },    // Web Refresh
            //{ KeyboardButtonCode.KC_WEBSTOP, },    // Web Stop
            //{ KeyboardButtonCode.KC_WEBFORWARD, },    // Web Forward
            //{ KeyboardButtonCode.KC_WEBBACK, },    // Web Back
            //{ KeyboardButtonCode.KC_MYCOMPUTER, },    // My Computer
            //{ KeyboardButtonCode.KC_MAIL, },    // Mail
            //{ KeyboardButtonCode.KC_MEDIASELECT, },     // Media Select
        };

        mouseRects = new Dictionary<MouseButtonCode, Rect>()
        {
            { MouseButtonCode.MB_BUTTON0, MouseLeft },
            { MouseButtonCode.MB_BUTTON1, MouseRight },
            { MouseButtonCode.MB_BUTTON2, Mouse3 },
            { MouseButtonCode.MB_BUTTON3, Mouse4 },
            { MouseButtonCode.MB_BUTTON4, Mouse5 },
            { MouseButtonCode.MB_BUTTON5, Mouse6 },
            //{ MouseButtonCode.MB_BUTTON6,  },
            //{ MouseButtonCode.MB_BUTTON7,  }
        };
    }
}
