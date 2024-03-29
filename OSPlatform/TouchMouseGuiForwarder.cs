﻿using Anomalous.OSPlatform;
using Engine;
using Engine.Platform;
using Engine.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anomalous.OSPlatform
{
    public class TouchMouseGuiForwarder : OnscreenKeyboardManager
    {
        enum MouseStatus
        {
            Released,
            Left,
            Right
        }

        private const long RightClickDeltaTime = 800000; //microseconds

        private int currentFingerId = int.MinValue;
        private long fingerDownTime = long.MinValue;
        private TravelTracker captureClickZone;
        private MouseStatus mouseInjectionMode = MouseStatus.Released;

        private Touches touches;
        private NativeOSWindow window;
        private InputHandler inputHandler;
		private bool enabled = true;
		private OnscreenKeyboardMode keyboardMode = OnscreenKeyboardMode.Hidden;
        private SystemTimer systemTimer;
        private bool forwardTouchesAsMouse = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventManager">The EventManager to use.</param>
        /// <param name="inputHandler">The InputHandler to use.</param>
        /// <param name="window">The window to show the onscreen keyboard on.</param>
        /// <param name="lastEventLayer">The last event layer in the eventManager.</param>
        public TouchMouseGuiForwarder(EventManager eventManager, InputHandler inputHandler, SystemTimer systemTimer, NativeOSWindow window, Object lastEventLayer, IScaleHelper scaleHelper)
        {
            captureClickZone = new TravelTracker(scaleHelper.Scaled(5));
            this.touches = eventManager.Touches;
            this.touches.FingerStarted += HandleFingerStarted;
            this.inputHandler = inputHandler;
            this.window = window;
            this.systemTimer = systemTimer;
        }

        /// <summary>
        /// In rare instances you might have to toggle the onscreen keyboard manually right away, this function will
        /// do that, but most times this is handled automatically with no problems.
        /// </summary>
        public void toggleKeyboard()
        {
            if (keyboardMode != window.KeyboardMode)
            {
                window.KeyboardMode = keyboardMode;
            }
        }

        /// <summary>
        /// Enable / Disable finger tracking.
        /// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
				if(!enabled && currentFingerId == int.MinValue)
				{
					stopTrackingFinger();
				}
			}
        }

        /// <summary>
        /// Set this to true to fire mouse events while touches are happening.
        /// </summary>
        public bool ForwardTouchesAsMouse
        {
            get
            {
                return forwardTouchesAsMouse;
            }
            set
            {
                forwardTouchesAsMouse = value;
            }
        }

        /// <summary>
        /// The keyboard mode that will be set the next time togglekeyboard is called. This does not reflect
        /// the actual keyboard status.
        /// </summary>
        public OnscreenKeyboardMode KeyboardMode
        {
            get
            {
                return keyboardMode;
            }
            set
            {
                if (keyboardMode != value)
                {
                    keyboardMode = value;
                    ThreadManager.invoke(toggleKeyboard);
                }
            }
        }

        void HandleFingerStarted(Finger obj)
        {
            if (currentFingerId == int.MinValue && enabled)
            {
                fingerDownTime = systemTimer.getCurrentTime();
                var finger = touches.Fingers[0];
                currentFingerId = finger.Id;
                touches.FingerEnded += fingerEnded;
                touches.FingerMoved += HandleFingerMoved;
				touches.FingersCanceled += HandleFingersCanceled;
                captureClickZone.reset();
                if (ForwardTouchesAsMouse)
                {
                    inputHandler.injectMoved(finger.PixelX, finger.PixelY);
                    mouseInjectionMode = MouseStatus.Released;
                }
            }
        }

        void HandleFingerMoved(Finger obj)
        {
            if (ForwardTouchesAsMouse && obj.Id == currentFingerId)
            {
                inputHandler.injectMoved(obj.PixelX, obj.PixelY);
                captureClickZone.traveled(new IntVector2(obj.PixelDeltaX, obj.PixelDeltaY));
                if (mouseInjectionMode == MouseStatus.Released && captureClickZone.TraveledOverLimit)
                {
                    if (systemTimer.getCurrentTime() - fingerDownTime < RightClickDeltaTime)
                    {
                        mouseInjectionMode = MouseStatus.Left;
                        inputHandler.injectButtonDown(MouseButtonCode.MB_BUTTON0); 
                    }
                    else
                    {
                        mouseInjectionMode = MouseStatus.Right;
                        inputHandler.injectButtonDown(MouseButtonCode.MB_BUTTON1);
                    }
                }
            }
        }

        void fingerEnded(Finger obj)
        {
            if (obj.Id == currentFingerId)
            {
                stopTrackingFinger();
                if (ForwardTouchesAsMouse)
                {
                    inputHandler.injectMoved(obj.PixelX, obj.PixelY);
                    switch (mouseInjectionMode)
                    {
                        case MouseStatus.Released:
                            if (systemTimer.getCurrentTime() - fingerDownTime < RightClickDeltaTime)
                            {
                                inputHandler.injectButtonDown(MouseButtonCode.MB_BUTTON0);
                                inputHandler.injectButtonUp(MouseButtonCode.MB_BUTTON0);
                            }
                            else
                            {
                                inputHandler.injectButtonDown(MouseButtonCode.MB_BUTTON1);
                                inputHandler.injectButtonUp(MouseButtonCode.MB_BUTTON1);
                            }
                            break;
                        case MouseStatus.Left:
                            inputHandler.injectButtonUp(MouseButtonCode.MB_BUTTON0);
                            break;
                        case MouseStatus.Right:
                            inputHandler.injectButtonUp(MouseButtonCode.MB_BUTTON1);
                            break;
                    }
                }
                mouseInjectionMode = MouseStatus.Released;
            }
		}

		void HandleFingersCanceled()
		{
			if(currentFingerId != int.MinValue)
			{
				stopTrackingFinger();
				inputHandler.injectButtonUp(MouseButtonCode.MB_BUTTON0);
				toggleKeyboard();
			}
		}

		void stopTrackingFinger()
		{
			touches.FingerEnded -= fingerEnded;
			touches.FingerMoved -= HandleFingerMoved;
            touches.FingersCanceled -= HandleFingersCanceled;
            currentFingerId = int.MinValue;
		}
    }
}
