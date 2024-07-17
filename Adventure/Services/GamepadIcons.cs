using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class GamepadIcons
{
    private ImageTexture icons;
    public ImageTexture Icons
    {
        get
        {
            if (icons == null)
            {
                icons = imageManager.Load("Graphics/Icons/kf_prompts_2048.png", true);
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

    public Rect A { get; } = CreateRect(0, 0);
    public Rect B { get; } = CreateRect(1, 0);
    public Rect Start { get; } = CreateRect(2, 0);
    public Rect DpadRight { get; } = CreateRect(3, 0);
    public Rect DpadUp { get; } = CreateRect(4, 0);
    public Rect LeftTrigger { get; } = CreateRect(5, 0);
    public Rect RightTrigger { get; } = CreateRect(6, 0);
    public Rect LeftStick { get; } = CreateRect(7, 0);
    public Rect RightStick { get; } = CreateRect(8, 0);
    public Rect LeftStickYAxis { get; } = CreateRect(9, 0);
    public Rect RightStickYAxis { get; } = CreateRect(10, 0);

    public Rect X { get; } = CreateRect(0, 1);
    public Rect Y { get; } = CreateRect(1, 1);
    public Rect Select { get; } = CreateRect(2, 1);
    public Rect DpadLeft { get; } = CreateRect(3, 1);
    public Rect DpadDown { get; } = CreateRect(4, 1);
    public Rect LeftShoulder { get; } = CreateRect(5, 1);
    public Rect RightShoulder { get; } = CreateRect(6, 1);
    public Rect LeftThumb { get; } = CreateRect(7, 1);
    public Rect RightThumb { get; } = CreateRect(8, 1);
    public Rect LeftStickXAxis { get; } = CreateRect(9, 1);
    public Rect RightStickXAxis { get; } = CreateRect(10, 1);

    public Rect Missing { get; } = CreateRect(14, 0);

    //public Rect  { get; } = CreateRect(0, 0);
    //public Rect  { get; } = CreateRect(1, 0);
    //public Rect  { get; } = CreateRect(2, 0);
    //public Rect  { get; } = CreateRect(3, 0);
    //public Rect  { get; } = CreateRect(4, 0);
    //public Rect  { get; } = CreateRect(5, 0);
    //public Rect  { get; } = CreateRect(6, 0);
    //public Rect  { get; } = CreateRect(7, 0);
    //public Rect  { get; } = CreateRect(8, 0);
    //public Rect  { get; } = CreateRect(9, 0);
    //public Rect  { get; } = CreateRect(10, 0);
    //public Rect  { get; } = CreateRect(11, 0);
    //public Rect  { get; } = CreateRect(12, 0);
    //public Rect  { get; } = CreateRect(13, 0);
    //public Rect  { get; } = CreateRect(14, 0);
    //public Rect  { get; } = CreateRect(15, 0);

    private IImageManager imageManager;
    private Dictionary<GamepadButtonCode, Rect> rects;

    public Rect GetButtonRect(GamepadButtonCode code)
    {
        if(rects.TryGetValue(code, out var val))
        {
            return val;
        }
        return Missing;
    }

    public GamepadIcons(IImageManager imageManager)
    {
        this.imageManager = imageManager;

        rects = new Dictionary<GamepadButtonCode, Rect>()
        {
            { GamepadButtonCode.XInput_A, A },
            { GamepadButtonCode.XInput_B, B },
            { GamepadButtonCode.XInput_X, X },
            { GamepadButtonCode.XInput_Y, Y },
            { GamepadButtonCode.XInput_LeftShoulder, LeftShoulder },
            { GamepadButtonCode.XInput_RightShoulder, RightShoulder },
            { GamepadButtonCode.XInput_Select, Select },
            { GamepadButtonCode.XInput_Start, Start },
            { GamepadButtonCode.XInput_LThumb, LeftThumb },
            { GamepadButtonCode.XInput_RThumb, RightThumb },
            { GamepadButtonCode.XInput_DPadUp, DpadUp },
            { GamepadButtonCode.XInput_DPadDown, DpadDown },
            { GamepadButtonCode.XInput_DPadLeft, DpadLeft },
            { GamepadButtonCode.XInput_DPadRight, DpadRight },
            //{ GamepadButtonCode.XInput_Guide,  },
            //{ GamepadButtonCode.XInput_C,  },
            //{ GamepadButtonCode.XInput_Z,  },
            { GamepadButtonCode.XInput_LTrigger, LeftTrigger },
            { GamepadButtonCode.XInput_RTrigger, RightTrigger },
        };
    }
}
