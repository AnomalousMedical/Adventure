using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpGui
{
    class ScreenPositioner : IScreenPositioner
    {
        private OSWindow window;

        public ScreenPositioner(OSWindow window)
        {
            this.window = window;
        }

        public IntRect GetTopLeftRect(in IntSize2 size)
        {
            return new IntRect(0, 0, size.Width, size.Height);
        }

        public IntRect GetTopRightRect(in IntSize2 size)
        {
            return new IntRect(window.WindowWidth - size.Width, 0, size.Width, size.Height);
        }

        public IntRect GetBottomRightRect(in IntSize2 size)
        {
            return new IntRect(window.WindowWidth - size.Width, window.WindowHeight - size.Height, size.Width, size.Height);
        }

        public IntRect GetBottomLeftRect(in IntSize2 size)
        {
            return new IntRect(0, window.WindowHeight - size.Height, size.Width, size.Height);
        }

        public IntRect GetCenterRect(in IntSize2 size)
        {
            return new IntRect(window.WindowWidth / 2 - size.Width / 2, window.WindowHeight / 2 - size.Height / 2, size.Width, size.Height);
        }

        public IntRect GetCenterTopRect(in IntSize2 size)
        {
            return new IntRect(window.WindowWidth / 2 - size.Width / 2, 0, size.Width, size.Height);
        }

        public IntSize2 ScreenSize => new IntSize2(window.WindowWidth, window.WindowHeight);
    }
}
