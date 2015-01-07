﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Logging;
using Engine.Renderer;
using Engine.Platform;
using Engine.Resources;
using System.IO;
using Engine.ObjectManagement;
using Editor;
using System.Xml;
using Engine.Saving.XMLSaver;
using Engine.Saving;
using Engine.Editing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Diagnostics;
using OgrePlugin;
using MyGUIPlugin;
using Anomalous.GuiFramework.Cameras;
using Anomalous.GuiFramework.Editor;
using Anomalous.GuiFramework;
using Anomalous.OSPlatform;
using Anomaly.GUI;

namespace Anomaly
{
    /// <summary>
    /// This is the primary controller for the Anomaly editor.
    /// </summary>
    public class AnomalyController : IDisposable, IDockProvider
    {
        //Engine
        private PluginManager pluginManager;
        private LogFileListener logListener;
        private App app;

        //GUI
        private NativeOSWindow mainWindow;
        private AnomalyMain mainForm;
        private IObjectEditorGUI mainObjectEditor = new ObjectEditorForm();
        private PropertiesEditor propertiesEditor;
        private SceneViewLightManager lightManager;

        private MDILayoutManager mdiLayout;
        private GUIManager guiManager;
        private SceneViewController sceneViewController;
        private SceneStatsDisplayManager sceneStatsDisplayManager;
        private FrameClearManager frameClearManager;

        private LogWindow consoleWindow;
        private DebugVisualizer debugVisualizer;

        //Platform
        private NativeUpdateTimer mainTimer;
        private NativeSystemTimer systemTimer;
        private EventManager eventManager;
        private NativeInputHandler inputHandler;
        private EventUpdateListener eventUpdate;
        private FullSpeedUpdateListener fixedUpdate;

        //Scene
        private SceneController sceneController = new SceneController();
        private ResourceController resourceController;
        private SimObjectController simObjectController;
        private InstanceBuilder instanceBuilder;
        private EditInterfaceRendererController interfaceRenderer;

        //Tools
        private SelectionController selectionController = new SelectionController();
        private SimObjectMover selectionMovementTools;

        private Stopwatch stopwatch = new Stopwatch();
        private Solution solution;

        //Solution
        private SolutionController solutionController;
        private SolutionWindow solutionWindow;

        //Serialization
        private XmlSaver xmlSaver = new XmlSaver();

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnomalyController(App app)
        {
            this.app = app;
        }

        /// <summary>
        /// Intialize all plugins and create everything.
        /// </summary>
        public void initialize(Solution solution)
        {
            this.solution = solution;

            //Create the log.
            logListener = new LogFileListener();
            logListener.openLogFile(AnomalyConfig.DocRoot + "/log.log");
            Log.Default.addLogListener(logListener);

            mainWindow = new NativeOSWindow("Anomaly Editor", new IntVector2(-1, -1), new IntSize2(AnomalyConfig.EngineConfig.HorizontalRes, AnomalyConfig.EngineConfig.VerticalRes));
            mainWindow.Closed += mainWindow_Closed;

            //Initialize the plugins
            pluginManager = new PluginManager(AnomalyConfig.ConfigFile);
            //Hardcoded assemblies
            MyGUIInterface.EventLayerKey = EventLayers.Main;

            GuiFrameworkCamerasInterface.MoveCameraEventLayer = EventLayers.Cameras;
            GuiFrameworkCamerasInterface.SelectWindowEventLayer = EventLayers.AfterMain;

            GuiFrameworkEditorInterface.ToolsEventLayers = EventLayers.Tools;

            pluginManager.addPluginAssembly(typeof(NativePlatformPlugin).Assembly);
            pluginManager.addPluginAssembly(typeof(OgreInterface).Assembly);
            pluginManager.addPluginAssembly(typeof(MyGUIInterface).Assembly);
            pluginManager.addPluginAssembly(typeof(GuiFrameworkInterface).Assembly);
            pluginManager.addPluginAssembly(typeof(GuiFrameworkCamerasInterface).Assembly);
            pluginManager.addPluginAssembly(typeof(GuiFrameworkEditorInterface).Assembly);
            pluginManager.OnConfigureDefaultWindow = createWindow;

            //Dynamic assemblies
            DynamicDLLPluginLoader pluginLoader = new DynamicDLLPluginLoader();
            ConfigIterator pluginIterator = solution.PluginSection.PluginIterator;
            pluginIterator.reset();
            while (pluginIterator.hasNext())
            {
                pluginLoader.addPath(pluginIterator.next());
            }
            pluginLoader.loadPlugins(pluginManager);
            pluginManager.initializePlugins();
            frameClearManager = new FrameClearManager(OgreInterface.Instance.OgrePrimaryWindow.OgreRenderTarget, new Color(0.2f, 0.2f, 0.2f));

            lightManager = pluginManager.RendererPlugin.createSceneViewLightManager();

            //Core resources
            MyGUIInterface.Instance.CommonResourceGroup.addResource(this.GetType().AssemblyQualifiedName, "EmbeddedResource", true);
            OgreResourceGroupManager.getInstance().initializeAllResourceGroups();

            //Intialize the platform
            systemTimer = new NativeSystemTimer();

            mainTimer = new NativeUpdateTimer(systemTimer);
            mainTimer.FramerateCap = AnomalyConfig.EngineConfig.MaxFPS;
            inputHandler = new NativeInputHandler(mainWindow, false);
            eventManager = new EventManager(inputHandler, Enum.GetValues(typeof(EventLayers)));
            eventUpdate = new EventUpdateListener(eventManager);
            mainTimer.addUpdateListener(eventUpdate);
            pluginManager.setPlatformInfo(mainTimer, eventManager);

            //Layout Chain
            mdiLayout = new MDILayoutManager();

            //Scene views
            sceneViewController = new SceneViewController(mdiLayout, eventManager, mainTimer, pluginManager.RendererPlugin.PrimaryWindow, MyGUIInterface.Instance.OgrePlatform.getRenderManager(), null);
            sceneStatsDisplayManager = new SceneStatsDisplayManager(sceneViewController, OgreInterface.Instance.OgrePrimaryWindow.OgreRenderTarget);
            sceneStatsDisplayManager.StatsVisible = true;
            sceneViewController.createWindow("Camera 1", Vector3.Backward * 100, Vector3.Zero, Vector3.Min, Vector3.Max, 0.0f, float.MaxValue, 100);

            //Tools
            selectionMovementTools = new SimObjectMover("SelectionMover", PluginManager.Instance.RendererPlugin, eventManager, sceneViewController);
            selectionMovementTools.Visible = true;
            selectionMovementTools.addMovableObject("Selection", new SelectionMovableObject(selectionController));

            LayoutChain layoutChain = new LayoutChain();
            //layoutChain.addLink(new SingleChildChainLink(GUILocationNames.Taskbar, mainForm.LayoutContainer), true);
            layoutChain.addLink(new PopupAreaChainLink(GUILocationNames.FullscreenPopup), true);
            layoutChain.SuppressLayout = true;
            layoutChain.addLink(new MDIChainLink(GUILocationNames.MDI, mdiLayout), true);
            layoutChain.addLink(new PopupAreaChainLink(GUILocationNames.ContentAreaPopup), true);
            layoutChain.addLink(new FinalChainLink("SceneViews", mdiLayout.DocumentArea), true);
            layoutChain.SuppressLayout = false;

            guiManager = new GUIManager();
            guiManager.createGUI(mdiLayout, layoutChain, mainWindow);

            layoutChain.layout();

            //Load the config file and set the resource root up.
            VirtualFileSystem.Instance.addArchive(solution.ResourceRoot);

            resourceController = new ResourceController(this);

            solution.loadExternalFiles(this);

            //Create the main form
            AnomalyTreeIcons.createIcons();
            mainForm = new AnomalyMain();

            //Initialize controllers
            instanceBuilder = new InstanceBuilder();
            sceneController.initialize(this);
            sceneController.OnSceneLoaded += sceneController_OnSceneLoaded;
            sceneController.OnSceneUnloading += sceneController_OnSceneUnloading;
            simObjectController = new SimObjectController(this);

            fixedUpdate = new FullSpeedUpdateListener(sceneController);
            mainTimer.addUpdateListener(fixedUpdate);

            propertiesEditor = new PropertiesEditor("Properties", "Anomaly.GUI.Properties");
            guiManager.addManagedDialog(propertiesEditor);

            interfaceRenderer = new EditInterfaceRendererController(pluginManager.RendererPlugin, mainTimer, sceneController, propertiesEditor);

            solutionWindow = new SolutionWindow();
            guiManager.addManagedDialog(solutionWindow);
            solutionWindow.Visible = true;

            propertiesEditor.showRelativeTo(solutionWindow, WindowAlignment.Right);

            solutionController = new SolutionController(solution, solutionWindow, this, propertiesEditor);

            //Initialize the windows
            mainForm.initialize(this);
            propertiesEditor.AutoExpand = true;

            mainForm.SuspendLayout();

            //Create GUI
            consoleWindow = new LogWindow();
            guiManager.addManagedDialog(consoleWindow);
            consoleWindow.Visible = true;
            Log.Default.addLogListener(consoleWindow);

            debugVisualizer = new DebugVisualizer(pluginManager, sceneController);
            guiManager.addManagedDialog(debugVisualizer);
            debugVisualizer.Visible = true;

            mainForm.ResumeLayout();
        }

        /// <summary>
        /// Dispose of this controller and cleanup.
        /// </summary>
        public void Dispose()
        {
            if(consoleWindow != null)
            {
                Log.Default.removeLogListener(consoleWindow);
                consoleWindow.Dispose();
            }
            if(debugVisualizer != null)
            {
                debugVisualizer.Dispose();
            }
            if(mdiLayout != null)
            {
                mdiLayout.Dispose();
            }
            if (selectionMovementTools != null)
            {
                selectionMovementTools.Dispose();
            }
            if(guiManager != null)
            {
                guiManager.Dispose();
            }
            if(sceneViewController != null)
            {
                sceneViewController.Dispose();
            }
            if (eventManager != null)
            {
                eventManager.Dispose();
            }
            if (inputHandler != null)
            {
                inputHandler.Dispose();
            }
            if(systemTimer != null)
            {
                systemTimer.Dispose();
            }
            if(lightManager != null)
            {
                pluginManager.RendererPlugin.destroySceneViewLightManager(lightManager);
            }
            if (frameClearManager != null)
            {
                frameClearManager.Dispose();
            }
            if (pluginManager != null)
            {
                pluginManager.Dispose();
            }
            if(mainWindow != null)
            {
                mainWindow.Dispose();
            }

            AnomalyConfig.save();
            logListener.closeLogFile();
        }

        /// <summary>
        /// Show the form to the user and start the loop.
        /// </summary>
        public void start()
        {
            mainForm.Show();
            mainForm.Activate();
            //mainTimer.startLoop();
        }

        /// <summary>
        /// Stop the loop and begin the process of shutting down the program.
        /// </summary>
        public void shutdown()
        {
            mainForm.saveWindows(AnomalyConfig.DocRoot + "/windows.ini");
            sceneController.destroyScene();
            app.exit();
        }

        public void idle()
        {
            mainTimer.OnIdle();
        }

        /// <summary>
        /// Show the primary ObjectEditorForm for the given EditInterface.
        /// </summary>
        /// <param name="editInterface">The EditInterface to display on the form.</param>
        public void showObjectEditor(EditInterface editInterface)
        {
            mainObjectEditor.setEditInterface(editInterface, null, null);
        }

        public void showDockContent(DockContent content)
        {
            mainForm.showDockContent(content);
        }

        public void hideDockContent(DockContent content)
        {
            mainForm.hideDockContent(content);
        }

        /// <summary>
        /// Save the scene to the given filename.
        /// </summary>
        /// <param name="filename">The filename to save to.</param>
        public void saveScene(String filename)
        {
            ScenePackage scenePackage = new ScenePackage();
            scenePackage.SceneDefinition = sceneController.getSceneDefinition();
            scenePackage.ResourceManager = resourceController.getResourceManager();
            scenePackage.SimObjectManagerDefinition = simObjectController.getSimObjectManagerDefinition();
            XmlTextWriter fileWriter = new XmlTextWriter(filename, Encoding.Unicode);
            fileWriter.Formatting = Formatting.Indented;
            xmlSaver.saveObject(scenePackage, fileWriter);
            fileWriter.Close();
            Log.ImportantInfo("Scene saved to {0}.", filename);
        }

        /// <summary>
        /// Helper funciton to change to a new scene from a ScenePackage.
        /// </summary>
        public void buildScene()
        {
            stopwatch.Start();
            sceneController.destroyScene();
            solution.createCurrentProject();
            sceneController.createScene();
            stopwatch.Stop();
            Log.Info("Scene loaded in {0} seconds.", stopwatch.Elapsed.TotalSeconds);
            stopwatch.Reset();
        }

        public void refreshGlobalResources()
        {
            sceneController.destroyScene();
            solution.refreshGlobalResources();
            sceneController.createScene();
        }

        public void saveSolution(bool forceSave)
        {
            solution.save(forceSave);
        }

        public void build()
        {
            solution.build();
        }
        
        /// <summary>
        /// Put the editor into static mode. This allows full editing privlidges
        /// of all objects.
        /// </summary>
        public void setStaticMode()
        {
            //movePanel.Enabled = true;
            //rotatePanel.Enabled = true;
            sceneController.setDynamicMode(false);
            sceneController.destroyScene();
            sceneController.createScene();
            selectionMovementTools.Visible = true;
            propertiesEditor.Enabled = true;
            solutionWindow.Enabled = true;
        }

        /// <summary>
        /// Put the editor into dynamic mode. This allows the objects to move
        /// and behave as they would in the scene, however, it limits editing
        /// capability.
        /// </summary>
        public void setDynamicMode()
        {
            selectionMovementTools.Visible = false;
            //movePanel.Enabled = false;
            //rotatePanel.Enabled = false;
            sceneController.setDynamicMode(true);
            sceneController.destroyScene();
            sceneController.createScene();
            propertiesEditor.Enabled = false;
            solutionWindow.Enabled = false;
        }

        public void enableMoveTool()
        {
            selectionMovementTools.ShowMoveTools = true;
            selectionMovementTools.ShowRotateTools = false;
        }

        public void enableRotateTool()
        {
            selectionMovementTools.ShowMoveTools = false;
            selectionMovementTools.ShowRotateTools = true;
        }

        public void enableSelectTool()
        {
            selectionMovementTools.ShowMoveTools = false;
            selectionMovementTools.ShowRotateTools = false;
        }

        public void copy()
        {
            EngineClipboard.clear();
            if (solutionWindow.IsActivated)
            {
                foreach (EditInterface selectedInterface in solutionController.SelectedEditInterfaces)
                {
                    ClipboardEntry clipEntry = selectedInterface.ClipboardEntry;
                    if (selectedInterface.SupportsClipboard && clipEntry.SupportsCopy)
                    {
                        EngineClipboard.add(clipEntry);
                    }
                }
            }
            else if (propertiesEditor.IsActivated)
            {
                EditInterface selectedInterface = propertiesEditor.SelectedEditInterface;
                if (selectedInterface.SupportsClipboard)
                {
                    ClipboardEntry clipEntry = selectedInterface.ClipboardEntry;
                    if (clipEntry.SupportsCopy)
                    {
                        EngineClipboard.add(clipEntry);
                    }
                }

            }
            EngineClipboard.Mode = EngineClipboardMode.Copy;
        }

        public void cut()
        {
            EngineClipboard.clear();
            if (solutionWindow.IsActivated)
            {
                foreach (EditInterface selectedInterface in solutionController.SelectedEditInterfaces)
                {
                    ClipboardEntry clipEntry = selectedInterface.ClipboardEntry;
                    if (selectedInterface.SupportsClipboard && clipEntry.SupportsCut)
                    {
                        EngineClipboard.add(clipEntry);
                    }
                }
            }
            else if (propertiesEditor.IsActivated)
            {
                EditInterface selectedInterface = propertiesEditor.SelectedEditInterface;
                if (selectedInterface.SupportsClipboard)
                {
                    ClipboardEntry clipEntry = selectedInterface.ClipboardEntry;
                    if (clipEntry.SupportsCut)
                    {
                        EngineClipboard.add(clipEntry);
                    }
                }

            }
            EngineClipboard.Mode = EngineClipboardMode.Cut;
        }

        public void paste()
        {
            if (solutionWindow.IsActivated)
            {
                EditInterface selectedInterface = solutionController.CurrentEditInterface;
                ClipboardEntry clipEntry = selectedInterface.ClipboardEntry;
                if (selectedInterface.SupportsClipboard && clipEntry.SupportsPaste)
                {
                    EngineClipboard.paste(clipEntry);
                }
            }
            else if (propertiesEditor.IsActivated)
            {
                EditInterface selectedInterface = propertiesEditor.SelectedEditInterface;
                if (selectedInterface.SupportsClipboard)
                {
                    ClipboardEntry clipEntry = selectedInterface.ClipboardEntry;
                    if (clipEntry.SupportsPaste)
                    {
                        EngineClipboard.paste(clipEntry);
                    }
                }

            }
        }

        /// <summary>
        /// Restore function for restoring the window layout.
        /// </summary>
        /// <param name="persistString">The string describing the window.</param>
        /// <returns>The IDockContent associated with the given string.</returns>
        private IDockContent getDockContent(String persistString)
        {
            return null;
        }

        /// <summary>
        /// Helper function to create the default window. This is the callback
        /// to the PluginManager.
        /// </summary>
        /// <param name="defaultWindow"></param>
        private void createWindow(out WindowInfo defaultWindow)
        {
            //Setup main window
            defaultWindow = new WindowInfo(mainWindow, "Primary");
            defaultWindow.Fullscreen = AnomalyConfig.EngineConfig.Fullscreen;
            defaultWindow.MonitorIndex = 0;

            if (AnomalyConfig.EngineConfig.Fullscreen)
            {
                mainWindow.setSize(AnomalyConfig.EngineConfig.HorizontalRes, AnomalyConfig.EngineConfig.VerticalRes);
                mainWindow.ExclusiveFullscreen = true;
            }
            else
            {
                mainWindow.Maximized = true;
            }
            mainWindow.show();
        }

        /// <summary>
        /// Callback for when the scene is loaded.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="scene"></param>
        private void sceneController_OnSceneLoaded(SceneController controller, SimScene scene)
        {
            sceneViewController.createCameras(scene);
            lightManager.sceneLoaded(scene);
            selectionMovementTools.sceneLoaded(scene);
        }

        /// <summary>
        /// Callback for when the scene is unloading.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="scene"></param>
        private void sceneController_OnSceneUnloading(SceneController controller, SimScene scene)
        {
            sceneViewController.destroyCameras();
            lightManager.sceneUnloading(scene);
            selectionMovementTools.sceneUnloading(scene);
        }

        /// <summary>
        /// The PluginManager with all plugins currently loaded.
        /// </summary>
        public PluginManager PluginManager
        {
            get
            {
                return pluginManager;
            }
        }

        /// <summary>
        /// The EventManager for the editor.
        /// </summary>
        public EventManager EventManager
        {
            get
            {
                return eventManager;
            }
        }

        /// <summary>
        /// The main UpdateTimer driving the main thread.
        /// </summary>
        public UpdateTimer MainTimer
        {
            get
            {
                return mainTimer;
            }
        }

        /// <summary>
        /// The SceneController that handles aspects of the scene.
        /// </summary>
        public SceneController SceneController
        {
            get
            {
                return sceneController;
            }
        }

        public SimObjectController SimObjectController
        {
            get
            {
                return simObjectController;
            }
        }

        /// <summary>
        /// The ResourceController that manages the resources.
        /// </summary>
        public ResourceController ResourceController
        {
            get
            {
                return resourceController;
            }
        }

        /// <summary>
        /// The SelectionController that manages the current selection.
        /// </summary>
        public SelectionController SelectionController
        {
            get
            {
                return selectionController;
            }
        }

        public Solution Solution
        {
            get
            {
                return solution;
            }
        }

        void mainWindow_Closed(OSWindow window)
        {
            shutdown();
        }
    }
}
