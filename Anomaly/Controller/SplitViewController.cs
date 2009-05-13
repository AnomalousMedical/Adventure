﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.ObjectManagement;
using System.Windows.Forms;
using Engine.Platform;
using Engine.Renderer;
using WeifenLuo.WinFormsUI.Docking;

namespace Anomaly
{
    class SplitViewController
    {
        private AnomalyController controller;
        private List<SplitViewHost> cameras = new List<SplitViewHost>();
        private bool camerasActive = false;
        private bool showStatsActive = false;
        private UpdateTimer mainTimer;
        private SimScene scene;

        public SplitViewController()
        {

        }

        public void initialize(AnomalyController controller)
        {
            this.controller = controller;
        }

        private SplitViewHost addCamera(String name, Vector3 translation, Vector3 lookAt)
        {
            SplitViewHost cameraHost = new SplitViewHost(name, this);
            cameraHost.DrawingWindow.initialize(name, controller.EventManager, controller.PluginManager.RendererPlugin, translation, lookAt, this);
            cameras.Add(cameraHost);
            if (camerasActive)
            {
                cameraHost.DrawingWindow.createCamera(mainTimer, scene);
            }
            cameraHost.DrawingWindow.showStats(showStatsActive);
            return cameraHost;
        }

        public void createFourWaySplit()
        {
            closeAllWindows();
            CameraSection cameraSection = AnomalyConfig.CameraSection;
            SplitViewHost camera1 = addCamera("Camera 1", cameraSection.FrontCameraPosition, cameraSection.FrontCameraLookAt);
            controller.showDockContent(camera1);
            SplitViewHost camera2 = addCamera("Camera 2", cameraSection.BackCameraPosition, cameraSection.BackCameraLookAt);
            camera2.Show(camera1.Pane, DockAlignment.Right, 0.5);
            SplitViewHost camera3 = addCamera("Camera 3", cameraSection.RightCameraPosition, cameraSection.RightCameraLookAt);
            camera3.Show(camera1.Pane, DockAlignment.Bottom, 0.5);
            SplitViewHost camera4 = addCamera("Camera 4", cameraSection.LeftCameraPosition, cameraSection.LeftCameraLookAt);
            camera4.Show(camera2.Pane, DockAlignment.Bottom, 0.5);
        }

        public void createThreeWayUpperSplit()
        {
            closeAllWindows();
            CameraSection cameraSection = AnomalyConfig.CameraSection;
            SplitViewHost camera1 = addCamera("Camera 1", cameraSection.FrontCameraPosition, cameraSection.FrontCameraLookAt);
            controller.showDockContent(camera1);
            SplitViewHost camera2 = addCamera("Camera 2", cameraSection.BackCameraPosition, cameraSection.BackCameraLookAt);
            camera2.Show(camera1.Pane, DockAlignment.Bottom, 0.5);
            SplitViewHost camera3 = addCamera("Camera 3", cameraSection.RightCameraPosition, cameraSection.RightCameraLookAt);
            camera3.Show(camera2.Pane, DockAlignment.Right, 0.5);
        }

        public void createTwoWaySplit()
        {
            closeAllWindows();
            CameraSection cameraSection = AnomalyConfig.CameraSection;
            SplitViewHost camera1 = addCamera("Camera 1", cameraSection.FrontCameraPosition, cameraSection.FrontCameraLookAt);
            controller.showDockContent(camera1);
            SplitViewHost camera2 = addCamera("Camera 2", cameraSection.BackCameraPosition, cameraSection.BackCameraLookAt);
            camera2.Show(camera1.Pane, DockAlignment.Right, 0.5);
        }

        public void createOneWaySplit()
        {
            closeAllWindows();
            CameraSection cameraSection = AnomalyConfig.CameraSection;
            SplitViewHost camera1 = addCamera("Camera 1", cameraSection.FrontCameraPosition, cameraSection.FrontCameraLookAt);
            controller.showDockContent(camera1);
        }

        public void destroyCameras()
        {
            foreach (SplitViewHost host in cameras)
            {
                host.DrawingWindow.destroyCamera();
            }
            camerasActive = false;
            mainTimer = null;
            scene = null;
        }

        public void createCameras(UpdateTimer mainTimer, SimScene scene)
        {
            foreach (SplitViewHost host in cameras)
            {
                host.DrawingWindow.createCamera(mainTimer, scene);
            }
            camerasActive = true;
            this.mainTimer = mainTimer;
            this.scene = scene;
        }

        public void showStats(bool show)
        {
            foreach (SplitViewHost host in cameras)
            {
                host.DrawingWindow.showStats(show);
            }
            showStatsActive = show;
        }

        internal void _alertCameraDestroyed(SplitViewHost host)
        {
            cameras.Remove(host);
        }

        private void closeAllWindows()
        {
            List<SplitViewHost> listCopy = new List<SplitViewHost>(cameras);
            foreach (SplitViewHost host in listCopy)
            {
                host.Close();
            }
        }
    }
}
