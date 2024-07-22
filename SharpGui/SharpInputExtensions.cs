using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    static class SharpInputExtensions
    {
        public static bool Process(this SharpInput input, SharpGuiState state, SharpGuiBuffer buffer, IFontManager fontManager, SharpStyle style, Guid? navUp, Guid? navDown, Guid? navLeft, Guid? navRight, int gamepad)
        {
            Guid id = input.Id;

            var rect = input.Rect;
            int left = rect.Left;
            int top = rect.Top;
            int right = rect.Right;
            int bottom = rect.Bottom;

            state.GrabFocus(id);

            // Check whether the button should be active
            bool regionHit = state.RegionHitByMouse(left, top, right, bottom);
            if (regionHit)
            {
                state.TrySetActiveItem(id, state.MouseDown);
            }

            //Draw
            var look = state.GetLookForId(id, style);

            //Draw shadow
            if (look.ShadowOffset.x > 0 && look.ShadowOffset.y > 0)
            {
                var shadowOffset = look.ShadowOffset;
                var shadowLeft = left + shadowOffset.x;
                var shadowTop = top + shadowOffset.y;
                var shadowRight = right + shadowOffset.x;
                var shadowBottom = bottom + shadowOffset.y;

                buffer.DrawQuad(shadowLeft, shadowTop, shadowRight, shadowBottom, look.ShadowColor, input.Layer);
            }

            //Draw border
            buffer.DrawQuad(left, top, right, bottom, look.BorderColor, input.Layer);

            //Draw main button
            var mainLeft = left + look.Border.Left;
            var mainTop = top + look.Border.Top;
            var mainRight = right - look.Border.Right;
            var mainBottom = bottom - look.Border.Bottom;

            buffer.DrawQuad(mainLeft, mainTop, mainRight, mainBottom, look.Background, input.Layer);

            //Draw text
            var font = fontManager.FontOrDefault(input.Font);
            var textLeft = mainLeft + look.Padding.Left;
            var textTop = mainTop + look.Padding.Top;
            var textRight = mainRight - look.Padding.Right;
            bool fontIsWider = false;
            if (input.Text != null)
            {
                fontIsWider = font.IsTextWider(input.Text, textRight - textLeft);
                if (fontIsWider)
                {
                    buffer.DrawTextReverse(textLeft, textTop, textRight, look.Color, input.Text, font, input.Layer);
                }
                else
                {
                    buffer.DrawText(textLeft, textTop, textRight, look.Color, input.Text, font, input.Layer);
                }
            }

            //Determine clicked
            if (regionHit && !state.MouseDown && state.ActiveItem == id)
            {
                state.StealFocus(id);
            }

            bool result = false;

            if (state.ProcessFocus(id, 0, navUp: navUp, navDown: navDown, navLeft: navLeft, navRight: navRight))
            {
                if (DateTime.Now.Second % 2 == 0)
                {
                    var cursorLoc = font.FindCursorPos(input.Text, out var tallestChar);
                    var cursorPos = new IntVector2(textRight, textTop + cursorLoc.y);
                    if (!fontIsWider)
                    {
                        cursorPos.x = textLeft + cursorLoc.x;
                    }

                    //Draw Cursor
                    var cursorLeft = cursorPos.x;
                    var cursorTop = cursorPos.y;
                    var cursorRight = cursorLeft + Math.Max(look.Border.Left / 2, 1);
                    var cursorBottom = cursorTop + tallestChar;

                    buffer.DrawQuad(cursorLeft, cursorTop, cursorRight, cursorBottom, look.Color, input.Layer);
                }

                //Handle input
                if (state.LastKeyChar != uint.MaxValue)
                {
                    input.Text.Append((char)state.LastKeyChar);
                    result = true;
                }

                switch (state.KeyEntered)
                {
                    case Engine.Platform.KeyboardButtonCode.KC_BACK:
                        if (input.Text.Length > 0)
                        {
                            input.Text.Remove(input.Text.Length - 1, 1);
                            result = true;
                        }
                        break;
                }

                if (state.IsStandardAcceptPressed(gamepad))
                {
                    state.RequestKeyboardPopup(input.OnscreenKeyboardMode, input.Rect.Left, input.Rect.Top, input.Rect.Width, input.Rect.Height);
                    result = true;
                }
            }
            return result;
        }

        public static IntSize2 GetDesiredSize(this SharpInput input, Font font, SharpGuiState state, SharpStyle style)
        {
            var look = state.GetLookForId(input.Id, style);

            IntSize2 result = look.Border.ToSize() + look.Padding.ToSize();
            if (input.Text?.Length > 0)
            {
                result += font.MeasureText(input.Text);
            }
            else
            {
                result += new IntSize2(0, font.SubstituteGlyphInfoSize);
            }
            return result;
        }
    }
}
