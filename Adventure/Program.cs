using Anomalous.OSPlatform;
using Anomalous.OSPlatform.Win32;
using System;

namespace Adventure
{
    class Program
    {
        static void Main(string[] args)
        {
            WindowsRuntimePlatformInfo.Initialize();

            CoreApp app = null;
            try
            {
                app = new CoreApp();
                app.Run();
            }
            catch (Exception e)
            {
                if (app != null)
                {
                    //app.saveCrashLog();
                }
#if DETAILED_MESSAGES
                String errorMessage = e.Message + "\n" + e.StackTrace;
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    errorMessage += "\n" + e.Message + "\n" + e.StackTrace;
                }
#else
                String errorMessage = e.Message;
#endif
                MessageDialog.showErrorDialog(errorMessage, "Exception");
            }
            finally
            {
                if (app != null)
                {
                    app.Dispose();
                }
            }
        }
    }
}
