using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpGui
{
    public class KeepWidthLeftLayout : ILayoutItem
    {
        public ILayoutItem Child;
        private IntSize2 lastCalculatedSize;

        public KeepWidthLeftLayout(ILayoutItem child)
        {
            this.Child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            lastCalculatedSize = Child.GetDesiredSize(sharpGui);
            return new IntSize2(0, lastCalculatedSize.Height);
        }

        public void SetRect(in IntRect rect)
        {
            Child.SetRect(new IntRect(
                rect.Left,
                rect.Top,
                lastCalculatedSize.Width,
                lastCalculatedSize.Height
            ));
        }
    }

    public class KeepWidthRightLayout : ILayoutItem
    {
        public ILayoutItem Child;
        public IntPad Margin;
        private IntSize2 lastCalculatedSize;

        public KeepWidthRightLayout(ILayoutItem child)
        {
            this.Child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            lastCalculatedSize = Child.GetDesiredSize(sharpGui);
            return new IntSize2(0, lastCalculatedSize.Height);
        }

        public void SetRect(in IntRect rect)
        {
            Child.SetRect(new IntRect(
                rect.Left + rect.Width - lastCalculatedSize.Width,
                rect.Top,
                lastCalculatedSize.Width,
                lastCalculatedSize.Height
            ));
        }
    }

    public class KeepWidthCenterLayout : ILayoutItem
    {
        public ILayoutItem Child;
        public IntPad Margin;
        private IntSize2 lastCalculatedSize;

        public KeepWidthCenterLayout(ILayoutItem child)
        {
            this.Child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            lastCalculatedSize = Child.GetDesiredSize(sharpGui);
            return new IntSize2(0, lastCalculatedSize.Height);
        }

        public void SetRect(in IntRect rect)
        {
            Child.SetRect(new IntRect(
                rect.Left + rect.Width / 2 - lastCalculatedSize.Width / 2,
                rect.Top,
                lastCalculatedSize.Width,
                lastCalculatedSize.Height
            ));
        }
    }
}
