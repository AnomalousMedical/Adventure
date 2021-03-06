using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Engine.Threads;

namespace Anomalous.OSPlatform
{
    public class DirDialog
    {
        public delegate void ResultCallback(NativeDialogResult result, String path);

        public DirDialog(NativeOSWindow parent = null, String message = "", String startPath = "")
        {
            Parent = parent;
            Message = message;
            StartPath = startPath;
        }

        /// <summary>
        /// May or may not block the main thread depending on os. Assume it does
        /// not block and handle all results in the callback.
        /// </summary>
        /// <param name="callback">Called when the dialog is done showing with the results.</param>
        /// <returns></returns>
        public void showModal(ResultCallback callback)
        {
            DirDialogResults results = new DirDialogResults(callback);
            results.showNativeDialogModal(Parent, Message, StartPath);
        }

        public NativeOSWindow Parent { get; set; }

        public String Message { get; set; }

        public String StartPath { get; set; }

        class DirDialogResults : IDisposable
        {
            CallbackHandler callbackHandler;
            ResultCallback showModalCallback;
            GCHandle handle;

            public DirDialogResults(ResultCallback callback)
            {
                this.showModalCallback = callback;
                callbackHandler = new CallbackHandler();
            }

            public void Dispose()
            {
                handle.Free();
            }

            public void showNativeDialogModal(NativeOSWindow parent, String message, String startPath)
            {
                handle = GCHandle.Alloc(this, GCHandleType.Normal);
                IntPtr parentPtr = parent != null ? parent._NativePtr : IntPtr.Zero;
                callbackHandler.showModal(this, parentPtr, message, startPath);
            }

            private void getResults(NativeDialogResult result, IntPtr filePtr)
            {
                String managedFileString = Marshal.PtrToStringUni(filePtr);
                ThreadManager.invoke(() =>
                {
                    try
                    {
                        this.showModalCallback(result, managedFileString);
                    }
                    finally
                    {
                        this.Dispose();
                    }
                });
            }

            #region PInvoke

            [DllImport(NativePlatformPlugin.LibraryName, CallingConvention = CallingConvention.Cdecl)]
            private static extern void DirDialog_showModal(IntPtr parent, [MarshalAs(UnmanagedType.LPWStr)] String message, [MarshalAs(UnmanagedType.LPWStr)] String startPath, DirDialogResultCallback resultCallback
#if FULL_AOT_COMPILE
            , IntPtr instanceHandle
#endif
);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void DirDialogResultCallback(NativeDialogResult result, IntPtr file
#if FULL_AOT_COMPILE
            , IntPtr instanceHandle
#endif
);

#if FULL_AOT_COMPILE
            class CallbackHandler
            {
                static DirDialogResultCallback resultCb;

                static CallbackHandler()
                {
                    resultCb = getResults;
                }

                [Anomalous.Interop.MonoPInvokeCallback(typeof(DirDialogResultCallback))]
                private static void getResults(NativeDialogResult result, IntPtr file, IntPtr instanceHandle)
                {
                    GCHandle handle = GCHandle.FromIntPtr(instanceHandle);
                    (handle.Target as DirDialogResults).getResults(result, file);
                }

                public void showModal(DirDialogResults obj, IntPtr parentPtr, String message, String startPath)
                {
                    DirDialog_showModal(parentPtr, message, startPath, resultCb, GCHandle.ToIntPtr(obj.handle));
                }
            }
#else
            class CallbackHandler
            {
                DirDialogResultCallback resultCb;

                public void showModal(DirDialogResults obj, IntPtr parentPtr, String message, String startPath)
                {
                    resultCb = obj.getResults;
                    DirDialog_showModal(parentPtr, message, startPath, resultCb);
                }
            }
#endif

            #endregion
        }
    }
}
