using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Engine.Threads;

namespace Anomalous.OSPlatform
{
    public class FileOpenDialog
    {
        public delegate void ResultCallback(NativeDialogResult result, IEnumerable<String> files);

        public FileOpenDialog(NativeOSWindow parent = null, String message = "", String defaultDir = "", String defaultFile = "", String wildcard = "", bool selectMultiple = false)
        {
            Parent = parent;
            Message = message;
            DefaultDir = defaultDir;
            DefaultFile = defaultFile;
            Wildcard = wildcard;
            SelectMultiple = selectMultiple;
        }

        /// <summary>
        /// May or may not block the main thread depending on os. Assume it does
        /// not block and handle all results in the callback.
        /// </summary>
        /// <param name="callback">Called when the dialog is done showing with the results.</param>
        /// <returns></returns>
        public void showModal(ResultCallback callback)
        {
            FileOpenDialogResults results = new FileOpenDialogResults(callback);
            results.showNativeDialogModal(Parent, Message, DefaultDir, DefaultFile, Wildcard, SelectMultiple);
        }

        public NativeOSWindow Parent { get; set; }

        public String Message { get; set; }

        public String DefaultDir { get; set; }

        public String DefaultFile { get; set; }

        public String Wildcard { get; set; }

        public bool SelectMultiple { get; set; }

        class FileOpenDialogResults : IDisposable
        {
            List<String> paths = new List<string>();
            ResultCallback showModalCallback;
            GCHandle handle;
            CallbackHandler callbackHandler;

            public FileOpenDialogResults(ResultCallback callback)
            {
                this.showModalCallback = callback;
                callbackHandler = new CallbackHandler();
            }

            public void Dispose()
            {
                handle.Free();
            }

            public void showNativeDialogModal(NativeOSWindow parent, String message, String defaultDir, String defaultFile, String wildcard, bool selectMultiple)
            {
                handle = GCHandle.Alloc(this, GCHandleType.Normal);
                IntPtr parentPtr = parent != null ? parent._NativePtr : IntPtr.Zero;
                callbackHandler.showModal(this, parentPtr, message, defaultDir, defaultFile, wildcard, selectMultiple);
            }

            private void setPathString(IntPtr pathPtr)
            {
                paths.Add(Marshal.PtrToStringUni(pathPtr));
            }

            private void getResults(NativeDialogResult result)
            {
                ThreadManager.invoke(() =>
                {
                    try
                    {
                        this.showModalCallback(result, paths);
                    }
                    finally
                    {
                        this.Dispose();
                    }
                });
            }

            #region PInvoke

            [DllImport(NativePlatformPlugin.LibraryName, CallingConvention = CallingConvention.Cdecl)]
            private static extern void FileOpenDialog_showModal(IntPtr parent, [MarshalAs(UnmanagedType.LPWStr)] String message, [MarshalAs(UnmanagedType.LPWStr)] String defaultDir, [MarshalAs(UnmanagedType.LPWStr)] String defaultFile, [MarshalAs(UnmanagedType.LPWStr)] String wildcard, bool selectMultiple, FileOpenDialogSetPathString setPathString, FileOpenDialogResultCallback resultCallback
#if FULL_AOT_COMPILE
, IntPtr instanceHandle
#endif
);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void FileOpenDialogSetPathString(IntPtr path
#if FULL_AOT_COMPILE
, IntPtr instanceHandle
#endif
);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void FileOpenDialogResultCallback(NativeDialogResult result
#if FULL_AOT_COMPILE
, IntPtr instanceHandle
#endif
);

#if FULL_AOT_COMPILE
            class CallbackHandler
            {
                static FileOpenDialogSetPathString setPathStringCb;
                static FileOpenDialogResultCallback resultCb;

                static CallbackHandler()
                {
                    resultCb = getResults;
                    setPathStringCb = setPathString;
                }

                [Anomalous.Interop.MonoPInvokeCallback(typeof(FileOpenDialogResultCallback))]
                private static void getResults(NativeDialogResult result, IntPtr instanceHandle)
                {
                    GCHandle handle = GCHandle.FromIntPtr(instanceHandle);
                    (handle.Target as FileOpenDialogResults).getResults(result);
                }

                [Anomalous.Interop.MonoPInvokeCallback(typeof(FileOpenDialogSetPathString))]
                private static void setPathString(IntPtr path, IntPtr instanceHandle)
                {
                    GCHandle handle = GCHandle.FromIntPtr(instanceHandle);
                    (handle.Target as FileOpenDialogResults).setPathString(path);
                }

                public void showModal(FileOpenDialogResults obj, IntPtr parentPtr, String message, String defaultDir, String defaultFile, String wildcard, bool selectMultiple)
                {
                    FileOpenDialog_showModal(parentPtr, message, defaultDir, defaultFile, wildcard, selectMultiple, setPathStringCb, resultCb, GCHandle.ToIntPtr(obj.handle));
                }
            }
#else
            class CallbackHandler
            {
                FileOpenDialogSetPathString setPathStringCb;
                FileOpenDialogResultCallback resultCb;

                public void showModal(FileOpenDialogResults obj, IntPtr parentPtr, String message, String defaultDir, String defaultFile, String wildcard, bool selectMultiple)
                {
                    resultCb = obj.getResults;
                    setPathStringCb = obj.setPathString;
                    FileOpenDialog_showModal(parentPtr, message, defaultDir, defaultFile, wildcard, selectMultiple, setPathStringCb, resultCb);
                }
            }
#endif

            #endregion
        }
    }
}
