﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    static class SharpPanelExtensions
    {
        private static readonly Guid AllPanels = Guid.NewGuid();

        public static void Process(this SharpPanel panel, SharpGuiState state, SharpGuiBuffer buffer, SharpStyle style)
        {
            var rect = panel.Rect;
            int left = rect.Left;
            int top = rect.Top;
            int right = rect.Right;
            int bottom = rect.Bottom;

            var look = style.Normal;

            //Draw shadow
            if (look.ShadowOffset.x > 0 && look.ShadowOffset.y > 0)
            {
                var shadowOffset = look.ShadowOffset;
                var shadowLeft = left + shadowOffset.x;
                var shadowTop = top + shadowOffset.y;
                var shadowRight = right + shadowOffset.x;
                var shadowBottom = bottom + shadowOffset.y;

                buffer.DrawQuad(shadowLeft, shadowTop, shadowRight, shadowBottom, look.ShadowColor, panel.Layer);
            }

            //Draw border
            buffer.DrawQuad(left, top, right, bottom, look.BorderColor, panel.Layer);

            //Draw main area
            var mainLeft = left + look.Border.Left;
            var mainTop = top + look.Border.Top;
            var mainRight = right - look.Border.Right;
            var mainBottom = bottom - look.Border.Bottom;

            buffer.DrawQuad(mainLeft, mainTop, mainRight, mainBottom, look.Background, panel.Layer);
        }

        public static IntSize2 GetDesiredSize(this SharpPanel panel, SharpGuiState state, SharpStyle style)
        {
            var look = style.Normal;

            panel.CalcIntPad = look.Border + look.Padding;

            IntSize2 result = look.Border.ToSize() + look.Padding.ToSize() + panel.CalcDesiredSize;
            return result;
        }
    }
}
