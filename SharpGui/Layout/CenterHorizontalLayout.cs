using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    public class CenterHorizontalLayout : ILayoutItem
    {
        private readonly ILayoutItem child;
        private IntSize2 desiredSize;

        public CenterHorizontalLayout(ILayoutItem child)
        {
            this.child = child;
        }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            desiredSize = child.GetDesiredSize(sharpGui);
            return desiredSize;
        }

        public void SetRect(in IntRect rect)
        {
            if(rect.Width < desiredSize.Width)
            {
                child.SetRect(rect);
            }
            else
            {
                var widthDiff = rect.Width - desiredSize.Width;
                child.SetRect(new IntRect(rect.Left + widthDiff / 2, rect.Top, desiredSize.Width, rect.Height));
            }
        }
    }
}
