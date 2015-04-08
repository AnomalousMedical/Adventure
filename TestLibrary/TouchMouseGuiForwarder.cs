﻿using Anomalous.libRocketWidget;
using Anomalous.OSPlatform;
using Engine;
using Engine.Platform;
using libRocketPlugin;
using MyGUIPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anomalous.Minimus
{
    public class TouchMouseGuiForwarder
    {
        private int currentFingerId = int.MinValue;
        private IntVector2 gestureStartPos;

        private Touches touches; //Ghetto
        private NativeOSWindow window;
        private RocketWidget currentRocketWidget;
        private NativeInputHandler inputHandler;
		private bool enabled = true;
		private OnscreenKeyboardMode keyboardMode = OnscreenKeyboardMode.Hidden;

        public TouchMouseGuiForwarder(EventManager eventManager, NativeInputHandler inputHandler, NativeOSWindow window)
        {
            this.touches = eventManager.Touches;
            this.touches.FingerStarted += HandleFingerStarted;
            this.inputHandler = inputHandler;
            this.window = window;
            InputManager.Instance.ChangeKeyFocus += HandleChangeKeyFocus;
            RocketWidget.ElementFocused += HandleElementFocused;
			RocketWidget.ElementBlurred += HandleElementBlurred;
			RocketWidget.RocketWidgetDisposing += HandleRocketWidgetDisposing;

			eventManager[EventLayers.Last].Keyboard.KeyPressed += HandleKeyPressed;
			eventManager[EventLayers.Last].Keyboard.KeyReleased += HandleKeyReleased;
        }

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

        void HandleElementFocused(RocketWidget rocketWidget, Element element)
        {
            if (element != null)
            {
                currentRocketWidget = rocketWidget;
                String tag = element.TagName;
				switch (tag) 
				{
					case "input":
						String type = element.GetAttributeString ("type");
                        if(type == "password")
                        {
                            switch(type)
                            {
                                case "password":
                                    keyboardMode = OnscreenKeyboardMode.Secure;
                                    break;
                                case "text":
                                    keyboardMode = OnscreenKeyboardMode.Normal;
                                    break;
                                default:
                                    keyboardMode = OnscreenKeyboardMode.Hidden;
                                    break;
                            }

                        }
						break;
					case "textarea":
                        keyboardMode = OnscreenKeyboardMode.Normal;
						break;
					default:
                        keyboardMode = OnscreenKeyboardMode.Hidden;
						break;
				}
			}
		}

		void HandleElementBlurred (RocketWidget widget, Element element)
		{
			if(widget == currentRocketWidget)
			{
                keyboardMode = OnscreenKeyboardMode.Hidden;
			}
		}

		void HandleRocketWidgetDisposing(RocketWidget widget)
		{
			if(widget == currentRocketWidget)
			{
				currentRocketWidget = null;
                keyboardMode = OnscreenKeyboardMode.Hidden;
				//Handle these for keyboard toggle right away or it won't work
				toggleKeyboard();
			}
		}

        void HandleChangeKeyFocus(Widget widget)
        {
            if (currentRocketWidget == null || !currentRocketWidget.isHostWidget(widget))
            {
                if (widget != null && widget is EditBox)
                {
                    keyboardMode = OnscreenKeyboardMode.Normal;
                }
                else
                {
                    keyboardMode = OnscreenKeyboardMode.Hidden;
                }
            }
        }

        void HandleFingerStarted(Finger obj)
        {
            if (currentFingerId == int.MinValue && enabled)
            {
                var finger = touches.Fingers[0];
                currentFingerId = finger.Id;
                touches.FingerEnded += fingerEnded;
                touches.FingerMoved += HandleFingerMoved;
				touches.FingersCanceled += HandleFingersCanceled;
                gestureStartPos = new IntVector2(finger.PixelX, finger.PixelY);
                inputHandler.injectMoved(finger.PixelX, finger.PixelY);
                inputHandler.injectButtonDown(MouseButtonCode.MB_BUTTON0);
            }
        }

        void HandleFingerMoved(Finger obj)
        {
            if (obj.Id == currentFingerId)
            {
                inputHandler.injectMoved(obj.PixelX, obj.PixelY);
            }
        }

        void fingerEnded(Finger obj)
        {
            if (obj.Id == currentFingerId)
            {
				stopTrackingFinger();
                inputHandler.injectMoved(obj.PixelX, obj.PixelY);
                inputHandler.injectButtonUp(MouseButtonCode.MB_BUTTON0);
				toggleKeyboard();
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
			currentFingerId = int.MinValue;
		}

		void toggleKeyboard()
		{
			if(keyboardMode != window.KeyboardMode)
			{
                window.KeyboardMode = keyboardMode;
			}
		}

		void HandleKeyReleased(KeyboardButtonCode keyCode)
		{
			toggleKeyboard();
		}

		void HandleKeyPressed(KeyboardButtonCode keyCode, uint keyChar)
		{
			toggleKeyboard();
		}
    }
}
