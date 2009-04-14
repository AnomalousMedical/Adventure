﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Platform
{
    public abstract class InputHandler
    {
        private OSWindow windowHandle;


        /// <summary>
	    /// Creates a Keyboard object linked to the system keyboard.  This keyboard is valid
	    /// until the InputHandler is destroyed.
	    /// </summary>
	    /// <param name="buffered">True if the keyboard should be buffered, which allows the keyboard events to fire.</param>
	    /// <returns>The new keyboard.</returns>
	    public abstract Keyboard createKeyboard(bool buffered);

	    /// <summary>
	    /// Destroys the given keyboard.  The keyboard will be disposed after this function
	    /// call and you will no longer be able to use it.
	    /// </summary>
	    /// <param name="keyboard">The keyboard to destroy.</param>
        public abstract void destroyKeyboard(Keyboard keyboard);

	    /// <summary>
	    /// Creates a Mouse object linked to the system mouse.  This mouse is valid
	    /// until the InputHandler is destroyed.
	    /// </summary>
	    /// <param name="buffered">True if the mouse should be buffered, which allows the mouse events to fire.</param>
	    /// <returns>The new mouse.</returns>
        public abstract Mouse createMouse(bool buffered);

	    /// <summary>
	    /// Destroys the given mouse.  The mouse will be disposed after this function
	    /// call and you will no longer be able to use it.
	    /// </summary>
	    /// <param name="mouse">The mouse to destroy.</param>
        public abstract void destroyMouse(Mouse mouse);
    }
}
