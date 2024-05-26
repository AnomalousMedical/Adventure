using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpGui
{
    public class KeepSizeLeftLayout : ILayoutItem
    {
        public ILayoutItem Child;
        private IntSize2 lastCalculatedSize;

        public KeepSizeLeftLayout(ILayoutItem child)
        {
            this.Child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            lastCalculatedSize = Child.GetDesiredSize(sharpGui);
            return lastCalculatedSize;
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

    public class KeepSizeRightLayout : ILayoutItem
    {
        public ILayoutItem Child;
        public IntPad Margin;
        private IntSize2 lastCalculatedSize;

        public KeepSizeRightLayout(ILayoutItem child)
        {
            this.Child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            lastCalculatedSize = Child.GetDesiredSize(sharpGui);
            return lastCalculatedSize;
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
}
