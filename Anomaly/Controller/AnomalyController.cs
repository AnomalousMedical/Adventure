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
using PCPlatform;

namespace Anomaly
{
    /// <summary>
    /// This is the primary controller for the Anomaly editor.
    /// </summary>
    public class AnomalyController : IDisposable, IDockProvider
    {
        #region Fields
        //Engine
        private PluginManager pluginManager;
        private LogFileListener logListener;

        //GUI
        private AnomalyMain mainForm;
        private DrawingWindow hiddenEmbedWindow;
        private IObjectEditorGUI mainObjectEditor = new ObjectEditorForm();
        private DrawingWindowController drawingWindowController = new DrawingWindowController();
        private MovePanel movePanel = new MovePanel();
        private EulerRotatePanel rotatePanel = new EulerRotatePanel();
        private Dictionary<String, DebugVisualizer> debugVisualizers = new Dictionary<string, DebugVisualizer>();
        private ConsoleWindow consoleWindow = new ConsoleWindow();
        private VerticalObjectEditor verticalObjectEditor = new VerticalObjectEditor();
        private SceneViewLightManager lightManager;

        //Platform
        private UpdateTimer mainTimer;
        private PCSystemTimer systemTimer;
        private EventManager eventManager;
        private PCInputHandler inputHandler;
        private EventUpdateListener eventUpdate;
        private FullSpeedUpdateListener fixedUpdate;

        //Scene
        private SceneController sceneController = new SceneController();
        private ResourceController resourceController;
        private SimObjectController simObjectController;
        private InstanceBuilder instanceBuilder;
        private EditInterfaceRendererController interfaceRenderer;

        //Tools
        private ToolInteropController toolInterop = new ToolInteropController();
        private MoveController moveController = new MoveController();
        private SelectionController selectionController = new SelectionController();
        private RotateController rotateController = new RotateController();
        private MovementTool movementTool;
        private RotateTool rotateTool;
        private ToolManager toolManager;

        private Stopwatch stopwatch = new Stopwatch();
        private Solution solution;

        //Solution
        private SolutionController solutionController;
        private SolutionPanel solutionPanel = new SolutionPanel();

        //Serialization
        private XmlSaver xmlSaver = new XmlSaver();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnomalyController()
        {
            
        }

        #endregion Constructors

        #region Functions

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
            Log.Default.addLogListener(consoleWindow);

            //Initialize the plugins
            hiddenEmbedWindow = new DrawingWindow();
            pluginManager = new PluginManager(AnomalyConfig.ConfigFile);
            pluginManager.OnConfigureDefaultWindow = createWindow;
            DynamicDLLPluginLoader pluginLoader = new DynamicDLLPluginLoader();
            ConfigIterator pluginIterator = solution.PluginSection.PluginIterator;
            pluginIterator.reset();
            while (pluginIterator.hasNext())
            {
                pluginLoader.addPath(pluginIterator.next());
            }
            pluginLoader.loadPlugins(pluginManager);
            pluginManager.initializePlugins();
            pluginManager.RendererPlugin.PrimaryWindow.setEnabled(false);

            lightManager = pluginManager.RendererPlugin.createSceneViewLightManager();

            //Load the config file and set the resource root up.
            VirtualFileSystem.Instance.addArchive(solution.ResourceRoot);

            resourceController = new ResourceController(this);

            solution.loadExternalFiles(this);

            //Create the main form
            AnomalyTreeIcons.createIcons();
            mainForm = new AnomalyMain();

            //Intialize the platform
            systemTimer = new PCSystemTimer();

            PCUpdateTimer win32Timer = new PCUpdateTimer(systemTimer);
            WindowsMessagePump windowsPump = new WindowsMessagePump();
            windowsPump.MessageReceived += new PumpMessageEvent(win32Timer_MessageReceived);
            win32Timer.MessagePump = windowsPump;
            mainTimer = win32Timer;
            mainTimer.FramerateCap = AnomalyConfig.EngineConfig.MaxFPS;
            inputHandler = new PCInputHandler(mainForm, false, false, false, mainTimer);
            eventManager = new EventManager(inputHandler, Enum.GetValues(typeof(EventLayers)));
            eventUpdate = new EventUpdateListener(eventManager);
            mainTimer.addUpdateListener(eventUpdate);
            pluginManager.setPlatformInfo(mainTimer, eventManager);

            //Initialize controllers
            instanceBuilder = new InstanceBuilder();
            sceneController.initialize(this);
            sceneController.OnSceneLoaded += sceneController_OnSceneLoaded;
            sceneController.OnSceneUnloading += sceneController_OnSceneUnloading;
            toolInterop.setMoveController(moveController);
            toolInterop.setSelectionController(selectionController);
            toolInterop.setRotateController(rotateController);
            simObjectController = new SimObjectController(this);

            toolManager = new ToolManager(eventManager);
            mainTimer.addUpdateListener(toolManager);
            fixedUpdate = new FullSpeedUpdateListener(sceneController);
            mainTimer.addUpdateListener(fixedUpdate);
            toolInterop.setToolManager(toolManager);
            movementTool = new MovementTool("MovementTool", moveController);
            toolManager.addTool(movementTool);
            rotateTool = new RotateTool("RotateTool", rotateController);
            toolManager.addTool(rotateTool);

            interfaceRenderer = new EditInterfaceRendererController(pluginManager.RendererPlugin, mainTimer, sceneController, verticalObjectEditor);

            solutionController = new SolutionController(solution, solutionPanel, this, verticalObjectEditor);

            //Initialize the windows
            mainForm.initialize(this);
            drawingWindowController.initialize(this, eventManager, pluginManager.RendererPlugin, AnomalyConfig.ConfigFile);
            movePanel.initialize(moveController);
            rotatePanel.initialize(rotateController);
            verticalObjectEditor.AutoExpand = true;

            //Initialize debug visualizers
            foreach (DebugInterface debugInterface in pluginManager.DebugInterfaces)
            {
                DebugVisualizer visualizer = new DebugVisualizer();
                visualizer.initialize(debugInterface);
                debugVisualizers.Add(visualizer.Text, visualizer);
            }

            mainForm.SuspendLayout();

            //Attempt to restore windows, or create default layout.
            if (!mainForm.restoreWindows(AnomalyConfig.DocRoot + "/windows.ini", getDockContent))
            {
                drawingWindowController.createOneWaySplit();
                mainForm.showDockContent(movePanel);
                rotatePanel.Show(movePanel.Pane, DockAlignment.Right, 0.5);
                mainForm.showDockContent(verticalObjectEditor);
                mainForm.showDockContent(solutionPanel);
                foreach (DebugVisualizer visualizer in debugVisualizers.Values)
                {
                    mainForm.showDockContent(visualizer);
                }
                mainForm.showDockContent(consoleWindow);
            }
            else
            {
                foreach (DebugVisualizer visualizer in debugVisualizers.Values)
                {
                    if (visualizer.DockPanel == null)
                    {
                        mainForm.showDockContent(visualizer);
                    }
                }
            }

            mainForm.ResumeLayout();
        }

        void win32Timer_MessageReceived(ref WinMsg message)
        {
            Message msg = Message.Create(message.hwnd, message.message, message.wParam, message.lParam);
            ManualMessagePump.pumpMessage(ref msg);
        }

        /// <summary>
        /// Dispose of this controller and cleanup.
        /// </summary>
        public void Dispose()
        {
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
            if (pluginManager != null)
            {
                pluginManager.Dispose();
            }
            if (hiddenEmbedWindow != null)
            {
                hiddenEmbedWindow.Dispose();
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
            mainTimer.startLoop();
        }

        /// <summary>
        /// Stop the loop and begin the process of shutting down the program.
        /// </summary>
        public void shutdown()
        {
            mainForm.saveWindows(AnomalyConfig.DocRoot + "/windows.ini");
            sceneController.destroyScene();
            mainTimer.stopLoop();
        }

        /// <summary>
        /// Show the primary ObjectEditorForm for the given EditInterface.
        /// </summary>
        /// <param name="editInterface">The EditInterface to display on the form.</param>
        public void showObjectEditor(EditInterface editInterface)
        {
            mainObjectEditor.setEditInterface(editInterface, null, null, null);
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
            movePanel.Enabled = true;
            rotatePanel.Enabled = true;
            sceneController.setDynamicMode(false);
            sceneController.destroyScene();
            sceneController.createScene();
            toolManager.setEnabled(true);
            verticalObjectEditor.Enabled = true;
            solutionPanel.Enabled = true;
        }

        /// <summary>
        /// Put the editor into dynamic mode. This allows the objects to move
        /// and behave as they would in the scene, however, it limits editing
        /// capability.
        /// </summary>
        public void setDynamicMode()
        {
            toolManager.setEnabled(false);
            movePanel.Enabled = false;
            rotatePanel.Enabled = false;
            sceneController.setDynamicMode(true);
            sceneController.destroyScene();
            sceneController.createScene();
            verticalObjectEditor.Enabled = false;
            solutionPanel.Enabled = false;
        }

        public void enableMoveTool()
        {
            toolManager.enableTool(movementTool);
            toolManager.setEnabled(!sceneController.isDynamicMode());
        }

        public void enableRotateTool()
        {
            toolManager.enableTool(rotateTool);
            toolManager.setEnabled(!sceneController.isDynamicMode());
        }

        public void enableSelectTool()
        {
            toolManager.setEnabled(false);
        }

        public void copy()
        {
            EngineClipboard.clear();
            if (solutionPanel.IsActivated)
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
            else if (verticalObjectEditor.IsActivated)
            {
                EditInterface selectedInterface = verticalObjectEditor.SelectedEditInterface;
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
            if (solutionPanel.IsActivated)
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
            else if (verticalObjectEditor.IsActivated)
            {
                EditInterface selectedInterface = verticalObjectEditor.SelectedEditInterface;
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
            if (solutionPanel.IsActivated)
            {
                EditInterface selectedInterface = solutionController.CurrentEditInterface;
                ClipboardEntry clipEntry = selectedInterface.ClipboardEntry;
                if (selectedInterface.SupportsClipboard && clipEntry.SupportsPaste)
                {
                    EngineClipboard.paste(clipEntry);
                }
            }
            else if (verticalObjectEditor.IsActivated)
            {
                EditInterface selectedInterface = verticalObjectEditor.SelectedEditInterface;
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
            if (persistString == movePanel.GetType().ToString())
            {
                return movePanel;
            }
            if (persistString == rotatePanel.GetType().ToString())
            {
                return rotatePanel;
            }
            if (persistString == consoleWindow.GetType().ToString())
            {
                return consoleWindow;
            }
            if (persistString == verticalObjectEditor.GetType().ToString())
            {
                return verticalObjectEditor;
            }
            if (persistString == solutionPanel.GetType().ToString())
            {
                return solutionPanel;
            }
            String name;
            if (DebugVisualizer.RestoreFromPersistance(persistString, out name))
            {
                if (debugVisualizers.ContainsKey(name))
                {
                    return debugVisualizers[name];
                }
                return null;
            }
            Vector3 translation;
            Vector3 lookAt;
            if (DrawingWindowHost.RestoreFromString(persistString, out name, out translation, out lookAt))
            {
                return drawingWindowController.createDrawingWindowHost(name, translation, lookAt);
            }
            return null;
        }

        /// <summary>
        /// Helper function to create the default window. This is the callback
        /// to the PluginManager.
        /// </summary>
        /// <param name="defaultWindow"></param>
        private void createWindow(out WindowInfo defaultWindow)
        {
            defaultWindow = new WindowInfo(hiddenEmbedWindow, "Primary");
        }

        /// <summary>
        /// Callback for when the scene is loaded.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="scene"></param>
        private void sceneController_OnSceneLoaded(SceneController controller, SimScene scene)
        {
            drawingWindowController.createCameras(mainTimer, scene);
            lightManager.sceneLoaded(scene);
            toolManager.createSceneElements(scene.getDefaultSubScene(), pluginManager);
        }

        /// <summary>
        /// Callback for when the scene is unloading.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="scene"></param>
        private void sceneController_OnSceneUnloading(SceneController controller, SimScene scene)
        {
            drawingWindowController.destroyCameras();
            lightManager.sceneUnloading(scene);
            toolManager.destroySceneElements(scene.getDefaultSubScene(), pluginManager);
        }

        #endregion Functions

        #region Properties

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
        /// The MoveController to move objects with.
        /// </summary>
        public MoveController MoveController
        {
            get
            {
                return moveController;
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

        /// <summary>
        /// The RotateController that handles rotating objects.
        /// </summary>
        public RotateController RotateController
        {
            get
            {
                return rotateController;
            }
        }

        /// <summary>
        /// Get the SplitViewController.
        /// </summary>
        public DrawingWindowController ViewController
        {
            get
            {
                return drawingWindowController;
            }
        }

        public Solution Solution
        {
            get
            {
                return solution;
            }
        }

        #endregion Properties
    }
}
