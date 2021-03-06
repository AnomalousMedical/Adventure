using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Engine.Platform.Input;
using Engine.Platform;
using Logging;
using Engine;

namespace Anomalous.OSPlatform.Android
{
    class AndroidGamepad : GamepadHardware
    {
        public static AndroidGamepad Pad1 { get; private set; }
        public static AndroidGamepad Pad2 { get; private set; }
        public static AndroidGamepad Pad3 { get; private set; }
        public static AndroidGamepad Pad4 { get; private set; }

        public static AndroidGamepad GetPad(GamepadId pad)
        {
            switch (pad)
            {
                case GamepadId.Pad1:
                    return Pad1;
                case GamepadId.Pad2:
                    return Pad2;
                case GamepadId.Pad3:
                    return Pad3;
                case GamepadId.Pad4:
                    return Pad4;
            }
            throw new NotSupportedException($"Cannot find gamepad {pad}");
        }

        public static bool IsJoystickEvent(KeyEvent e)
        {
            //Getting weird values for source, so not checking it for now, buttons are specifically joystick anyway
            //if ((e.Source & InputSourceType.ClassJoystick) == InputSourceType.ClassJoystick)
            switch (e.KeyCode)
            {
                case Keycode.ButtonA:
                case Keycode.ButtonB:
                case Keycode.ButtonC:
                case Keycode.ButtonL1:
                case Keycode.ButtonL2:
                case Keycode.ButtonMode:
                case Keycode.ButtonR1:
                case Keycode.ButtonR2:
                case Keycode.ButtonSelect:
                case Keycode.ButtonStart:
                case Keycode.ButtonThumbl:
                case Keycode.ButtonThumbr:
                case Keycode.ButtonX:
                case Keycode.ButtonY:
                case Keycode.ButtonZ:
                case Keycode.Button1:
                case Keycode.Button2:
                case Keycode.Button3:
                case Keycode.Button4:
                case Keycode.Button5:
                case Keycode.Button6:
                case Keycode.Button7:
                case Keycode.Button8:
                case Keycode.Button9:
                case Keycode.Button10:
                case Keycode.Button11:
                case Keycode.Button12:
                case Keycode.Button13:
                case Keycode.Button14:
                case Keycode.Button15:
                case Keycode.Button16:
                    return true;
            }
            return false;
        }

        public static bool IsJoystickEvent(MotionEvent ev)
        {
            return ev.Source == InputSourceType.Joystick;
        }

        private bool[] keysDown = new bool[(int)GamepadButtonCode.NUM_BUTTONS];

        public AndroidGamepad(Gamepad pad) : base(pad)
        {
            switch (pad.Id)
            {
                case GamepadId.Pad1:
                    Pad1 = this;
                    break;
                case GamepadId.Pad2:
                    Pad2 = this;
                    break;
                case GamepadId.Pad3:
                    Pad3 = this;
                    break;
                case GamepadId.Pad4:
                    Pad4 = this;
                    break;
            }
        }

        public override void Dispose()
        {

        }

        public override void Update()
        {

        }

        /// <summary>
        /// Fire a key event, it should be known to be a joystick event before calling this.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public void HandleJoystickEvent(KeyEvent e)
        {
            //You can handle multiple controllers by watching them connect and disconnect, will do later
            //https://developer.android.com/training/game-controllers/multiple-controllers.html

            var button = KeycodeToButton(e.KeyCode);
            bool last, current;
            switch (e.Action)
            {
                case KeyEventActions.Down:
                    if (e.RepeatCount == 0)
                    {
                        last = keysDown[(int)button];
                        keysDown[(int)button] = current = true;
                        process(last, current, button);
                        //Log.Debug($"AndroidGamepad down {e.KeyCode} {e.Action}");
                    }
                    break;
                case KeyEventActions.Up:
                    last = keysDown[(int)button];
                    keysDown[(int)button] = current = false;
                    process(last, current, button);
                    //Log.Debug($"AndroidGamepad button up {e.KeyCode} {e.Action}");
                    break;
            }

            //Log.Debug($"AndroidGamepad button {e.KeyCode} {e.Action}");
        }

        //Good info
        //https://developer.android.com/training/game-controllers/controller-input.html#joystick

        /// <summary>
        /// Fire a generic motion event, it should be known to be a joystick event before calling this.
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        public void HandleJoystickEvent(MotionEvent ev)
        {
            //You should add history support, but right now joysticks are polling only anyway
            //when you add an event for them make sure to process the history

            var hatx = ev.GetAxisValue(Axis.HatX);
            var haty = ev.GetAxisValue(Axis.HatY);
            bool last, current;

            var key = GamepadButtonCode.XInput_DPadUp;
            last = keysDown[(int)key];
            keysDown[(int)key] = current = haty < -0.1f;
            process(last, current, key);

            key = GamepadButtonCode.XInput_DPadDown;
            last = keysDown[(int)key];
            keysDown[(int)key] = current = haty > 0.1f; 
            process(last, current, key);

            key = GamepadButtonCode.XInput_DPadLeft;
            last = keysDown[(int)key];
            keysDown[(int)key] = current = hatx < -0.1f;
            process(last, current, key);

            key = GamepadButtonCode.XInput_DPadRight;
            last = keysDown[(int)key];
            keysDown[(int)key] = current = hatx > 0.1f;
            process(last, current, key);

            const int historyPose = -1;

            Vector2 lStick = new Vector2(getCenteredAxis(ev, ev.Device, Axis.X, historyPose),
                                         getCenteredAxis(ev, ev.Device, Axis.Y, historyPose));

            Vector2 rStick = new Vector2(getCenteredAxis(ev, ev.Device, Axis.Z, historyPose),
                                         getCenteredAxis(ev, ev.Device, Axis.Rz, historyPose));

            this.fireMovement(lStick, rStick, ev.GetAxisValue(Axis.Ltrigger), ev.GetAxisValue(Axis.Rtrigger));
        }

        private static float getCenteredAxis(MotionEvent evt, InputDevice device, Axis axis, int historyPos) {
            InputDevice.MotionRange range =
                    device.GetMotionRange(axis, evt.Source);

            // A joystick at rest does not always report an absolute position of
            // (0,0). Use the getFlat() method to determine the range of values
            // bounding the joystick axis center.
            if (range != null) {
                float flat = range.Flat;
                float value =
                        historyPos < 0 ? evt.GetAxisValue(axis):
                        evt.GetHistoricalAxisValue(axis, historyPos);

                // Ignore axis values that are within the 'flat' region of the
                // joystick axis center.
                if (Math.Abs(value) > flat) {
                    return value;
                }
        }
            return 0;
}

        private void process(bool last, bool current, GamepadButtonCode code)
        {
            if (!last && current)
            {
                this.fireButtonDown(code);
                //Log.Debug($"Button {code} down");
            }
            else if (last && !current)
            {
                this.fireButtonUp(code);
                //Log.Debug($"Button {code} up");
            }
        }

        private GamepadButtonCode KeycodeToButton(Keycode code)
        {
            switch (code)
            {
                case Keycode.ButtonA:
                    return GamepadButtonCode.XInput_A;
                case Keycode.ButtonB:
                    return GamepadButtonCode.XInput_B;
                case Keycode.ButtonC:
                    return GamepadButtonCode.XInput_C;
                case Keycode.ButtonL1:
                    return GamepadButtonCode.XInput_LeftShoulder;
                case Keycode.ButtonL2:
                    return GamepadButtonCode.XInput_LTrigger;
                case Keycode.ButtonMode:
                    return GamepadButtonCode.XInput_Guide;
                case Keycode.ButtonR1:
                    return GamepadButtonCode.XInput_RightShoulder;
                case Keycode.ButtonR2:
                    return GamepadButtonCode.XInput_RTrigger;
                case Keycode.ButtonSelect:
                    return GamepadButtonCode.XInput_Select;
                case Keycode.ButtonStart:
                    return GamepadButtonCode.XInput_Start;
                case Keycode.ButtonThumbl:
                    return GamepadButtonCode.XInput_LThumb;
                case Keycode.ButtonThumbr:
                    return GamepadButtonCode.XInput_RThumb;
                case Keycode.ButtonX:
                    return GamepadButtonCode.XInput_X;
                case Keycode.ButtonY:
                    return GamepadButtonCode.XInput_Y;
                case Keycode.ButtonZ:
                    return GamepadButtonCode.XInput_Z;
                case Keycode.DpadUp:
                    return GamepadButtonCode.XInput_DPadUp;
                case Keycode.DpadDown:
                    return GamepadButtonCode.XInput_DPadDown;
                case Keycode.DpadLeft:
                    return GamepadButtonCode.XInput_DPadLeft;
                case Keycode.DpadRight:
                    return GamepadButtonCode.XInput_DPadRight;
                case Keycode.Button1:
                    return GamepadButtonCode.Button1;
                case Keycode.Button2:
                    return GamepadButtonCode.Button2;
                case Keycode.Button3:
                    return GamepadButtonCode.Button3;
                case Keycode.Button4:
                    return GamepadButtonCode.Button4;
                case Keycode.Button5:
                    return GamepadButtonCode.Button5;
                case Keycode.Button6:
                    return GamepadButtonCode.Button6;
                case Keycode.Button7:
                    return GamepadButtonCode.Button7;
                case Keycode.Button8:
                    return GamepadButtonCode.Button8;
                case Keycode.Button9:
                    return GamepadButtonCode.Button9;
                case Keycode.Button10:
                    return GamepadButtonCode.Button10;
                case Keycode.Button11:
                    return GamepadButtonCode.Button11;
                case Keycode.Button12:
                    return GamepadButtonCode.Button12;
                case Keycode.Button13:
                    return GamepadButtonCode.Button13;
                case Keycode.Button14:
                    return GamepadButtonCode.Button14;
                case Keycode.Button15:
                    return GamepadButtonCode.Button15;
                case Keycode.Button16:
                    return GamepadButtonCode.Button16;
                default:
                    throw new NotSupportedException($"Cannot convert Keycode {code} to GamepadButtonCode");
            }
        }
    }
}