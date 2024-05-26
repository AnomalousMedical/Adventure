using Anomalous.OSPlatform;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    internal class EventManagerTracker : IDisposable
    {
        private readonly EventManager eventManager;
        private readonly NativeOSWindow osWindow;
        private readonly ISharpGui sharpGui;

        public EventManagerTracker(EventManager eventManager, NativeOSWindow osWindow, ISharpGui sharpGui)
        {
            this.eventManager = eventManager;
            this.osWindow = osWindow;
            this.sharpGui = sharpGui;
            eventManager.InputModeSwitched += EventManager_InputModeSwitched;
        }

        public void Dispose()
        {
            eventManager.InputModeSwitched -= EventManager_InputModeSwitched;
        }

        private void EventManager_InputModeSwitched(EventManager ems)
        {
            switch (ems.CurrentInputMode)
            {
                case InputMode.Gamepad:
                    sharpGui.ShowHover = false;
                    osWindow.setCursor(CursorType.Hidden);
                    break;
                default:
                    sharpGui.ShowHover = true;
                    osWindow.setCursor(CursorType.Arrow);
                    break;
            }
        }
    }
}
