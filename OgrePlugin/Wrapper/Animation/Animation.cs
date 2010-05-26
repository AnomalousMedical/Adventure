﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OgreWrapper
{
    public class Animation : IDisposable
    {
        protected IntPtr animation;

        internal static Animation createWrapper(IntPtr nativePtr, object[] args)
        {
            return new Animation(nativePtr);
        }

        protected Animation(IntPtr animation)
        {
            this.animation = animation;
        }

        public void Dispose()
        {
            animation = IntPtr.Zero;
        }
    }
}
