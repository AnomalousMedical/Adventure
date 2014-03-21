﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Platform
{
    public delegate void OSWindowEvent(OSWindow window);

    public interface OSWindow
    {
        /// <summary>
        /// An int pointer to the handle of the window.
        /// </summary>
        String WindowHandle { get; }

        /// <summary>
        /// The current width of the window.
        /// </summary>
        int WindowWidth { get; }

        /// <summary>
        /// The current height of the window.
        /// </summary>
        int WindowHeight { get; }

        /// <summary>
        /// True if the window has focus.
        /// </summary>
        bool Focused { get; }

        /// <summary>
        /// Called when the window is moved.
        /// </summary>
        /// <param name="window">The window.</param>
        event OSWindowEvent Moved;

        /// <summary>
        /// Called when the window is resized.
        /// </summary>
        /// <param name="window">The window.</param>
        event OSWindowEvent Resized;

        /// <summary>
        /// Called when the window is closing. This happens before the window is
        /// gone and its handle is no longer valid.
        /// </summary>
        /// <param name="window">The window.</param>
        event OSWindowEvent Closing;

        /// <summary>
        /// Called whenthe window has closed.
        /// </summary>
        /// <param name="window">The window that closed.</param>
        event OSWindowEvent Closed;

        /// <summary>
        /// Called when the focus has changed on the window.
        /// </summary>
        /// <param name="window"></param>
        event OSWindowEvent FocusChanged;
    }
}
