﻿using DiligentEngine;
using Engine;
using Engine.Platform;
using System;
using System.Text;

namespace SharpGui
{
    public interface ISharpGui
    {
        /// <summary>
        /// Start a gui frame.
        /// </summary>
        /// <param name="clock"></param>
        void Begin(Clock clock);
        
        /// <summary>
        /// End a gui frame. Call after begin and before render.
        /// </summary>
        void End();
        
        /// <summary>
        /// Render the gui frame to the context.
        /// </summary>
        /// <param name="immediateContext"></param>
        void Render(IDeviceContext immediateContext);

        Guid ActiveItem { get; }

        Guid HoverItem { get; }

        Guid FocusedItem { get; }

        KeyboardButtonCode KeyEntered { get; }

        GamepadButtonCode[] GamepadButtonEntered { get; }

        /// <summary>
        /// Draw a slider. Returns true if the passed in value changed.
        /// </summary>
        /// <param name="slider">The slider to draw.</param>
        /// <param name="value">The current value of the slider.</param>
        /// <returns>True if value changed.</returns>
        public bool Slider(SharpSliderHorizontal slider, ref int value, Guid? navUp = null, Guid? navDown = null, GamepadId gamepad = GamepadId.Pad1);

        /// <summary>
        /// Draw a slider. Returns true if the passed in value changed.
        /// </summary>
        /// <param name="slider">The slider to draw.</param>
        /// <param name="value">The current value of the slider.</param>
        /// <returns>True if value changed.</returns>
        public bool Slider(SharpSliderVertical slider, ref int value, Guid? navLeft = null, Guid? navRight = null, GamepadId gamepad = GamepadId.Pad1);

        /// <summary>
        /// Draw a progress bar. Does not take input or foucs.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="percent"></param>
        void Progress(SharpProgressHorizontal progress, float percent);

        /// <summary>
        /// Draw some text of unlimited width.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <param name="text"></param>
        void Text(int x, int y, Color color, string text, float layer = 0f);

        /// <summary>
        /// Draw text up to a max width.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <param name="text"></param>
        void Text(int x, int y, Color color, string text, int maxWidth, float layer = 0f);

        /// <summary>
        /// Draw the given sharp text.
        /// </summary>
        void Text(SharpText text);

        /// <summary>
        /// Draw a button.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        bool Button(SharpButton button, Guid? navUp = null, Guid? navDown = null, Guid? navLeft = null, Guid? navRight = null, GamepadId gamepad = GamepadId.Pad1);

        /// <summary>
        /// Draw a text input.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        bool Input(SharpInput input, Guid? navUp = null, Guid? navDown = null, Guid? navLeft = null, Guid? navRight = null);

        /// <summary>
        /// Draw a panel.
        /// </summary>
        /// <param name="input"></param>
        void Panel(SharpPanel input);
        bool IsStandardNextPressed(GamepadId gamepad = GamepadId.Pad1);
        bool IsStandardPreviousPressed(GamepadId gamepad = GamepadId.Pad1);

        /// <summary>
        /// Measure the size of a button.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        IntSize2 MeasureButton(SharpButton button);

        /// <summary>
        /// Measure the size of an input text.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IntSize2 MeasureInput(SharpInput input);

        /// <summary>
        /// Measure the size of a panel.
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        IntSize2 MeasurePanel(SharpPanel panel);

        /// <summary>
        /// This should be used carefully ideally before begin or after end is called.
        /// This will change the current focused item to the new id. Useful
        /// for navigation.
        /// </summary>
        /// <param name="id"></param>
        void StealFocus(Guid id);

        /// <summary>
        /// Measure the size of the text in the given string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IntSize2 MeasureText(String text);

        /// <summary>
        /// Measure the size of the text in the string builder.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IntSize2 MeasureText(StringBuilder text);

        /// <summary>
        /// Determine if this input type's standardized back button is pressed.
        /// </summary>
        /// <returns></returns>
        bool IsStandardBackPressed(GamepadId gamepad = GamepadId.Pad1);
    }
}