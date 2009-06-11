﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Logging;
using Engine;
using Anomaly.GUI;

namespace Anomaly
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SplashScreen splash = new SplashScreen();
            splash.Show();
            Application.DoEvents();
            using (AnomalyController anomalyController = new AnomalyController())
            {
                try
                {
                    anomalyController.initialize();
                    anomalyController.createNewScene();
                    splash.Close();
                    splash.Dispose();
                    anomalyController.start();
                }
                catch (Exception e)
                {
                    Log.Default.printException(e);
                    MessageBox.Show(e.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
