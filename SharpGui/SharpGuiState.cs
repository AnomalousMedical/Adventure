using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    class SharpGuiState
    {
        private static readonly Guid EmptySpace = Guid.NewGuid(); //A guid for when the user clicks on empty space. This gets considered to be active

        private bool sawFocusedItem = false;

        public Guid HoverItem { get; private set; }
        public Guid ActiveItem { get; private set; }
        public int MouseX { get; private set; }
        public int MouseY { get; private set; }
        public bool MouseDown { get; private set; }
        public Guid FocusedItem { get; private set; }
        public KeyboardButtonCode KeyEntered { get; private set; }
        public GamepadButtonCode[] GamepadButtonEntered { get; private set; }
        public bool IsShift { get; private set; }
        public bool IsAlt { get; private set; }
        public bool IsCtrl { get; private set; }
        public Guid LastWidget { get; private set; }
        public uint LastKeyChar { get; private set; }
        public bool ShowHover { get; set; } = true;
        public RequestOnscreenKeyboard KeyboardPopupRequested { get; set; }

        KeyboardButtonCode standardAcceptKey = KeyboardButtonCode.KC_RETURN;
        GamepadButtonCode[] standardAcceptButton = [
            GamepadButtonCode.XInput_A,
            GamepadButtonCode.XInput_A,
            GamepadButtonCode.XInput_A,
            GamepadButtonCode.XInput_A,
        ];

        KeyboardButtonCode standardNextFocusKey = KeyboardButtonCode.KC_TAB;

        KeyboardButtonCode standardNavUpKey = KeyboardButtonCode.KC_UP;
        GamepadButtonCode[] standardNavUpButton = [
            GamepadButtonCode.XInput_DPadUp,
            GamepadButtonCode.XInput_DPadUp,
            GamepadButtonCode.XInput_DPadUp,
            GamepadButtonCode.XInput_DPadUp,
        ];

        KeyboardButtonCode standardNavDownKey = KeyboardButtonCode.KC_DOWN;
        GamepadButtonCode[] standardNavDownButton = [
            GamepadButtonCode.XInput_DPadDown,
            GamepadButtonCode.XInput_DPadDown,
            GamepadButtonCode.XInput_DPadDown,
            GamepadButtonCode.XInput_DPadDown,
        ];

        KeyboardButtonCode standardNavLeftKey = KeyboardButtonCode.KC_LEFT;
        GamepadButtonCode[] standardNavLeftButton = [
            GamepadButtonCode.XInput_DPadLeft,
            GamepadButtonCode.XInput_DPadLeft,
            GamepadButtonCode.XInput_DPadLeft,
            GamepadButtonCode.XInput_DPadLeft,
        ];

        KeyboardButtonCode standardNavRightKey = KeyboardButtonCode.KC_RIGHT;
        GamepadButtonCode[] standardNavRightButton = [
            GamepadButtonCode.XInput_DPadRight,
            GamepadButtonCode.XInput_DPadRight,
            GamepadButtonCode.XInput_DPadRight,
            GamepadButtonCode.XInput_DPadRight,
        ];

        public void Begin(int mouseX, int mouseY, bool mouseDown, KeyboardButtonCode lastKeyPressed, uint lastKeyChar, bool isShift, bool isAlt, bool isCtrl, GamepadButtonCode[] lastGamepadKey)
        {
            sawFocusedItem = false;
            this.LastKeyChar = lastKeyChar;
            this.KeyEntered = lastKeyPressed;
            this.GamepadButtonEntered = lastGamepadKey;
            this.MouseX = mouseX;
            this.MouseY = mouseY;
            this.MouseDown = mouseDown;
            this.HoverItem = Guid.Empty;
            this.IsShift = isShift;
            this.IsAlt = isAlt;
            this.IsCtrl = isCtrl;
        }

        public void End()
        {
            if (MouseDown)
            {
                //This needs to say nested, above check is just mouse up / down
                //If ActiveItem is empty at the end of the frame, consider empty space to be clicked.
                if (ActiveItem == Guid.Empty)
                {
                    ActiveItem = EmptySpace;
                }
            }
            else
            {
                ActiveItem = Guid.Empty;
            }

            if (!sawFocusedItem)
            {
                FocusedItem = Guid.Empty;
            }

            KeyEntered = KeyboardButtonCode.KC_UNASSIGNED;
        }

        public bool RegionHitByMouse(int left, int top, int right, int bottom)
        {
            return !(MouseX < left ||
                   MouseY < top ||
                   MouseX >= right ||
                   MouseY >= bottom);
        }

        /// <summary>
        /// Try to set this item active. Will also make it the hover item.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shouldActivate"></param>
        public void TrySetActiveItem(Guid id, bool shouldActivate)
        {
            HoverItem = id;
            if (ShowHover)
            {
                FocusedItem = id;
            }
            if (ActiveItem == Guid.Empty && shouldActivate)
            {
                ActiveItem = id;
            }
        }

        /// <summary>
        /// Call this in every widget to try to get keyboard focus. Only changes if nothing has focus.
        /// </summary>
        /// <param name="id"></param>
        public void GrabFocus(Guid id)
        {
            if (FocusedItem == Guid.Empty)
            {
                FocusedItem = id;
            }
        }

        /// <summary>
        /// Take keyboard focus, but any previous items with focus may have already run.
        /// Treat it as not having focus until the next frame. This is good for taking focus
        /// when things happen like clicking on the item with the mouse. It would be ideal
        /// to call this at the end of your widget where input is being handled. The FocusedItem
        /// property is changed immediately after this is called. This will also clear any input
        /// for this frame from the keyboard and gamepad.
        /// </summary>
        /// <param name="id"></param>
        public void StealFocus(Guid id)
        {
            FocusedItem = id;
            KeyEntered = KeyboardButtonCode.KC_UNASSIGNED;
            GamepadButtonEntered[0] = GamepadButtonCode.NUM_BUTTONS;
            GamepadButtonEntered[1] = GamepadButtonCode.NUM_BUTTONS;
            GamepadButtonEntered[2] = GamepadButtonCode.NUM_BUTTONS;
            GamepadButtonEntered[3] = GamepadButtonCode.NUM_BUTTONS;
        }

        /// <summary>
        /// Handle common focus code like changing between items. This will return true if the calling code should further process
        /// input and false if it should do nothing more.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ProcessFocus(Guid id, int gamepad, Guid? navUp = null, Guid? navDown = null, Guid? navLeft = null, Guid? navRight = null)
        {
            bool callerHandlesInput = false;
            if (FocusedItem == id)
            {
                sawFocusedItem = true;
                callerHandlesInput = true;
                //If tab is pressed, drop to allow next widget to pick it up.
                if (IsStandardNextFocusPressed())
                {
                    if (IsShift)
                    {
                        FocusedItem = LastWidget;
                    }
                    else
                    {
                        FocusedItem = Guid.Empty;
                    }
                    callerHandlesInput = false;
                }
                else if (navUp != null && IsStandardNavUpPressed(gamepad))
                {
                    FocusedItem = navUp.Value;
                    callerHandlesInput = false;
                }
                else if (navDown != null && IsStandardNavDownPressed(gamepad))
                {
                    FocusedItem = navDown.Value;
                    callerHandlesInput = false;
                }
                else if (navLeft != null && IsStandardNavLeftPressed(gamepad))
                {
                    FocusedItem = navLeft.Value;
                    callerHandlesInput = false;
                }
                else if (navRight != null && IsStandardNavRightPressed(gamepad))
                {
                    FocusedItem = navRight.Value;
                    callerHandlesInput = false;
                }

                //If input was handled by anything here, clear the current key so nothing else processes it.
                if (!callerHandlesInput)
                {
                    ShowHover = false;
                    KeyEntered = KeyboardButtonCode.KC_UNASSIGNED;
                    GamepadButtonEntered[0] = GamepadButtonCode.NUM_BUTTONS;
                    GamepadButtonEntered[1] = GamepadButtonCode.NUM_BUTTONS;
                    GamepadButtonEntered[2] = GamepadButtonCode.NUM_BUTTONS;
                    GamepadButtonEntered[3] = GamepadButtonCode.NUM_BUTTONS;
                }
            }
            LastWidget = id;
            return callerHandlesInput;
        }

        public SharpLook GetLookForId(Guid id, SharpStyle style)
        {
            if (ShowHover && HoverItem == id)
            {
                if (ActiveItem == id)
                {
                    if (FocusedItem == id)
                    {
                        return style.HoverAndActiveAndFocus;
                    }

                    return style.HoverAndActive;
                }

                if (FocusedItem == id)
                {
                    return style.HoverAndFocus;
                }

                return style.Hover;
            }

            if (ActiveItem == id)
            {
                return style.Active;
            }

            if (FocusedItem == id)
            {
                return style.Focus;
            }

            return style.Normal;
        }

        public bool IsStandardAcceptPressed(int gamepad)
        {
            return GamepadButtonEntered[gamepad] == standardAcceptButton[gamepad] || KeyEntered == standardAcceptKey;
        }

        public void OverrideStandardAccept(KeyboardButtonCode key, GamepadButtonCode[] buttons)
        {
            this.standardAcceptKey = key;
            this.standardAcceptButton = buttons;
        }

        public bool IsStandardNextFocusPressed()
        {
            return KeyEntered == standardNextFocusKey;
        }

        public void OverrideStandardNextFocus(KeyboardButtonCode key)
        {
            this.standardNextFocusKey = key;
        }

        public bool IsStandardNavUpPressed(int gamepad)
        {
            return GamepadButtonEntered[gamepad] == standardNavUpButton[gamepad] || KeyEntered == standardNavUpKey;
        }

        public void OverrideStandardNavUp(KeyboardButtonCode key, GamepadButtonCode[] buttons)
        {
            this.standardNavUpKey = key;
            this.standardNavUpButton = buttons;
        }

        public bool IsStandardNavDownPressed(int gamepad)
        {
            return GamepadButtonEntered[gamepad] == standardNavDownButton[gamepad] || KeyEntered == standardNavDownKey;
        }

        public void OverrideStandardNavDown(KeyboardButtonCode key, GamepadButtonCode[] buttons)
        {
            this.standardNavDownKey = key;
            this.standardNavDownButton = buttons;
        }

        public bool IsStandardNavLeftPressed(int gamepad)
        {
            return GamepadButtonEntered[gamepad] == standardNavLeftButton[gamepad] || KeyEntered == standardNavLeftKey;
        }

        public void OverrideStandardNavLeft(KeyboardButtonCode key, GamepadButtonCode[] buttons)
        {
            this.standardNavLeftKey = key;
            this.standardNavLeftButton = buttons;
        }

        public bool IsStandardNavRightPressed(int gamepad)
        {
            return GamepadButtonEntered[gamepad] == standardNavRightButton[gamepad] || KeyEntered == standardNavRightKey;
        }

        public void OverrideStandardNavRight(KeyboardButtonCode key, GamepadButtonCode[] buttons)
        {
            this.standardNavRightKey = key;
            this.standardNavRightButton = buttons;
        }

        public void RequestKeyboardPopup(RequestedOnscreenKeyboardMode mode, int x, int y, int width, int height)
        {
            this.KeyboardPopupRequested?.Invoke(mode, x, y, width, height);
        }
    }
}
