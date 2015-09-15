﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgrePlugin.VirtualTexture
{
    /// <summary>
    /// This class helps manage a handle to a resource in the TextureCache. This uses reference counting,
    /// which is incremented internally for the checkout and is decremented on dispose. The idea is to
    /// allow for a using block when using these handles.
    /// </summary>
    abstract class TextureCacheHandle : IDisposable
    {
        private int numCheckouts = 0;
        private bool destroyOnNoRef = false;
        private Object lockObject = new object();

        public TextureCacheHandle(bool destroyOnNoRef)
        {
            this.destroyOnNoRef = destroyOnNoRef;
        }

        /// <summary>
        /// Dispose, call when finished with a checked out handle. Destroys
        /// the image if there are no more outstanding resources and the cahce requested it.
        /// </summary>
        public void Dispose()
        {
            lock (lockObject)
            {
                --numCheckouts;
                if (destroyOnNoRef && numCheckouts == 0)
                {
                    disposing();
                }
            }
        }

        protected abstract void disposing();

        public abstract PixelBox getPixelBox(VTexPage page, IndirectionTexture indirectionTexture, int padding, int padding2, int textelsPerPage);

        /// <summary>
        /// The size of the image in bytes.
        /// </summary>
        public abstract ulong Size { get; }

        /// <summary>
        /// Internal function to increment the counter.
        /// </summary>
        internal void checkout()
        {
            lock (lockObject)
            {
                ++numCheckouts;
            }
        }

        /// <summary>
        /// Internal management function to destroy the image if possible.
        /// </summary>
        internal void destroyIfPossible()
        {
            lock (lockObject)
            {
                destroyOnNoRef = true;
                if (numCheckouts == 0)
                {
                    disposing();
                }
            }
        }
    }

}
