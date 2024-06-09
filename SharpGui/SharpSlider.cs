using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    public abstract class SharpSlider : ILayoutItem
    {
        public SharpSlider()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }

        public IntRect Rect;

        public IntSize2 DesiredSize;

        public int Max { get; set; }

        public float Layer { get; set; }

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            return DesiredSize;
        }

        public void SetRect(in IntRect rect)
        {
            this.Rect = rect;
        }
    }

    public class SharpSliderHorizontal : SharpSlider
    {
        
    }

    public class SharpSliderVertical : SharpSlider
    {
    }
}
