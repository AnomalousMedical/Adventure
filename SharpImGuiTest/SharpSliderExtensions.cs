﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpImGuiTest
{ 
    static class SharpSliderExtensions
    {
        private static int ButtonMargin = 8;
        private static Color ButtonBackgroundColor = Color.FromARGB(0xff777777);
        private static Color FocusAndActive = Color.FromARGB(0xffff0000);
        private static Color Focus = Color.FromARGB(0xffffffff);
        private static Color Normal = Color.FromARGB(0xffaaaaaa);

        public static bool Process(this SharpSlider slider, ref int value, SharpGuiState state, SharpGuiBuffer buffer)
        {
            Guid id = slider.Id;
            int x = slider.X;
            int y = slider.Y;
            int width = slider.Width;
            int height = slider.Height;
            int max = slider.Max;

            var doubleMargin = ButtonMargin * 2;
            var withinMarginHeight = height - doubleMargin;
            var buttonX = x + ButtonMargin;
            var buttonY = y + ButtonMargin;
            int buttonWidth = width - doubleMargin;
            int buttonHeight = withinMarginHeight / (max + 1);

            // Calculate mouse cursor's relative y offset
            int ypos = value * buttonHeight;
            buttonY += ypos;

            // Check for hotness
            if (state.RegionHit(x, y, width, height))
            {
                state.FocusItem = id;
                if (state.ActiveItem == Guid.Empty && state.MouseDown)
                {
                    state.ActiveItem = id;
                }
            }

            // Render the scrollbar
            buffer.DrawQuad(x, y, width, height, ButtonBackgroundColor);

            // Render scroll button
            var color = Normal;
            if (state.FocusItem == id)
            {
                if (state.ActiveItem == id)
                {
                    color = FocusAndActive;
                }
                else
                {
                    color = Focus;
                }
            }
            buffer.DrawQuad(buttonX, buttonY, buttonWidth, buttonHeight, color);

            // Update widget value
            if (state.ActiveItem == id)
            {
                int mousepos = state.MouseY - (y + ButtonMargin);
                if (mousepos < 0) { mousepos = 0; }
                if (mousepos > withinMarginHeight) { mousepos = withinMarginHeight; }

                int v = mousepos / buttonHeight;
                if (v > max)
                {
                    v = max;
                }
                if (v != value)
                {
                    value = v;
                    return true;
                }
            }

            return false;
        }
    }
}