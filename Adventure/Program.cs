using Anomalous.OSPlatform;
using Anomalous.OSPlatform.Win32;
using System;
using System.IO;

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
                var outDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anomalous Adventure");
                var outFile = Path.Combine(outDir, $"crash-{DateTime.UtcNow.ToFileTime()}.txt");

                Directory.CreateDirectory(outDir);

                using var textWriter = new StreamWriter(File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None));

                while (e != null)
                {
                    textWriter.WriteLine("Begin " + e.GetType().Name);
                    textWriter.WriteLine(e.Message);
                    textWriter.WriteLine(e.StackTrace);
                    if (e is AggregateException)
                    {
                        foreach(var age in ((AggregateException)e).InnerExceptions)
                        {
                            textWriter.WriteLine($"Begin Inner AggregateException {age.GetType().Name}");
                            textWriter.WriteLine(age.Message);
                            textWriter.WriteLine(age.StackTrace);
                        }
                        e = null; //On AggregateExceptions we don't walk the InnerException
                    }
                    else
                    {
                        e = e.InnerException;
                    }
                }

                errorMessage += $"\nDetails written to '{outFile}'";

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
