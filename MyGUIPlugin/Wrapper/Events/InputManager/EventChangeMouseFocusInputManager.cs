﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Logging;

namespace MyGUIPlugin
{
    class EventChangeMouseFocusInputManager : MyGUIEventTranslator
    {
        private CallbackHandler callbackHandler;
        private FocusChangedEvent boundEvent;

        public EventChangeMouseFocusInputManager(InputManager inputManager)
        {
            callbackHandler = new CallbackHandler();
            nativeEventTranslator = callbackHandler.create(this, inputManager);
        }

        public override void Dispose()
        {
            base.Dispose();
            callbackHandler.Dispose();
        }

        /// <summary>
        /// The event that will be called back.
        /// </summary>
        public event FocusChangedEvent BoundEvent
        {
            add
            {
                if (boundEvent == null)
                {
                    MyGUIEventTranslator_bindEvent(nativeEventTranslator);
                }
                boundEvent += value;
            }
            remove
            {
                boundEvent -= value;
                if (boundEvent == null)
                {
                    MyGUIEventTranslator_unbindEvent(nativeEventTranslator);
                }
            }
        }

        private void nativeEvent(IntPtr widget)
        {
            if (boundEvent != null)
            {
                boundEvent.Invoke(WidgetManager.getWidget(widget));
            }
        }

        #region PInvoke

        [DllImport(MyGUIInterface.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr EventChangeMouseFocusInputManager_Create(IntPtr widget, NativeEventDelegate nativeEventCallback
#if FULL_AOT_COMPILE
        , IntPtr instanceHandle
#endif
);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void NativeEventDelegate(IntPtr widget
#if FULL_AOT_COMPILE
        , IntPtr instanceHandle
#endif
);

#if FULL_AOT_COMPILE
        class CallbackHandler : IDisposable
        {
            private static NativeEventDelegate nativeEventCallback;

            static CallbackHandler()
            {
                nativeEventCallback = nativeEvent;
            }

            [MonoTouch.MonoPInvokeCallback(typeof(NativeEventDelegate))]
            private static void nativeEvent(IntPtr widget, IntPtr instanceHandle)
            {
                GCHandle handle = GCHandle.FromIntPtr(instanceHandle);
                (handle.Target as EventChangeMouseFocusInputManager).nativeEvent(widget);
            }

            GCHandle handle;

            public IntPtr create(EventChangeMouseFocusInputManager obj, InputManager inputManager)
            {
                handle = GCHandle.Alloc(obj);
                return EventChangeMouseFocusInputManager_Create(inputManager.Ptr, nativeEventCallback, GCHandle.ToIntPtr(handle));
            }

            public void Dispose()
            {
                handle.Free();
            }
        }
#else
        class CallbackHandler : IDisposable
        {
            private NativeEventDelegate nativeEventCallback;

            public IntPtr create(EventChangeMouseFocusInputManager obj, InputManager inputManager)
            {
                nativeEventCallback = obj.nativeEvent;
                return EventChangeMouseFocusInputManager_Create(inputManager.Ptr, nativeEventCallback);
            }

            public void Dispose()
            {
                nativeEventCallback = null;
            }
        }
#endif

        #endregion
    }
}