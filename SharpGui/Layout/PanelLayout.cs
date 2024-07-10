﻿using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpGui
{
    public class PanelLayout : ILayoutItem
    {
        public SharpPanel Panel;
        public ILayoutItem Child;

        public PanelLayout(SharpPanel panel, ILayoutItem child)
        {
            this.Panel = panel;
            this.Child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            var size = Child.GetDesiredSize(sharpGui);
            this.Panel.CalcDesiredSize = size;
            size = sharpGui.MeasurePanel(Panel);
            return size;
        }

        public void SetRect(in IntRect rect)
        {
            Panel.Rect = rect;

            var panelPadding = Panel.CalcIntPad;

            Child.SetRect(new IntRect(
                rect.Left + panelPadding.Left,
                rect.Top + panelPadding.Top,
                rect.Width - panelPadding.Left - panelPadding.Right,
                rect.Height - panelPadding.Top - panelPadding.Bottom
                ));
        }
    }

    public class PanelLayoutNoPad : ILayoutItem
    {
        public SharpPanel Panel;
        public ILayoutItem Child;

        public PanelLayoutNoPad(SharpPanel panel, ILayoutItem child)
        {
            this.Panel = panel;
            this.Child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            return Child.GetDesiredSize(sharpGui);
        }

        public void SetRect(in IntRect rect)
        {
            Panel.Rect = rect;
            Child.SetRect(rect);
        }
    }
}
