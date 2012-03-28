﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.Platform;
using OgreWrapper;
using OgrePlugin;
using Logging;
using Engine.Renderer;

namespace MyGUIPlugin
{
    public class MyGUIInterface : PluginInterface
    {
        public static MyGUIInterface Instance { get; private set; }

        private OgrePlatform ogrePlatform;
        private Gui gui;
        private SceneManager sceneManager;
        private OgreWindow ogreWindow;
        Camera camera;
        Viewport vp;

        private UpdateTimer mainTimer;
        private MyGUIUpdate myGUIUpdate;
        private MyGUIRenderListener renderListener;
        private ManagedMyGUILogListener managedLogListener;

        public MyGUIInterface()
        {
            if (Instance == null)
            {
                Instance = this;
                SmoothShowDuration = 0.25f;
            }
            else
            {
                throw new Exception("Can only create MyGUIInterface one time.");
            }
        }

        public void Dispose()
        {
            if(mainTimer != null)
            {
                mainTimer.removeFixedUpdateListener(myGUIUpdate);
            }
            if (vp != null)
            {
                ogreWindow.OgreRenderWindow.destroyViewport(vp);
            }
            if (camera != null)
            {
                sceneManager.destroyCamera(camera);
            }
            if(gui != null)
            {
                gui.shutdown();
                gui.Dispose();
            }
            if(ogrePlatform != null)
            {
                ogrePlatform.shutdown();
                ogrePlatform.Dispose();
            }
            if(sceneManager != null)
            {
                Root.getSingleton().destroySceneManager(sceneManager);
            }
            if (managedLogListener != null)
            {
                managedLogListener.Dispose();
            }
        }

        public void initialize(PluginManager pluginManager)
        {
            Log.Info("Initializing MyGUI");

            OgreResourceGroupManager.getInstance().addResourceLocation(GetType().AssemblyQualifiedName, "EmbeddedResource", "MyGUI", true);
            OgreResourceGroupManager.getInstance().initializeAllResourceGroups();

            sceneManager = Root.getSingleton().createSceneManager(SceneType.ST_GENERIC, "MyGUIScene");
            ogreWindow = pluginManager.RendererPlugin.PrimaryWindow as OgreWindow;

            //Create camera and viewport
            camera = sceneManager.createCamera("MyGUICamera");
            vp = ogreWindow.OgreRenderWindow.addViewport(camera, int.MaxValue, 0.0f, 0.0f, 1.0f, 1.0f);
            vp.setBackgroundColor(new Color(1.0f, 0.0f, 0.0f, 0.0f));
            vp.setClearEveryFrame(false);
            vp.clear();

            //Create Ogre Platform
            ogrePlatform = new OgrePlatform();
            ogrePlatform.initialize(ogreWindow.OgreRenderWindow, sceneManager, "MyGUI", "");

            //Create log
            managedLogListener = new ManagedMyGUILogListener();

            renderListener = new MyGUIRenderListener(vp, sceneManager);

            gui = new Gui();
            gui.initialize("");

            //Load config files
            ResourceManager resourceManager = ResourceManager.Instance;
            if (!String.IsNullOrEmpty(OSTheme))
            {
                resourceManager.load(OSTheme);
            }
            resourceManager.load(MainTheme);

            Log.Info("Finished initializing MyGUI");
        }

        public void setPlatformInfo(UpdateTimer mainTimer, EventManager eventManager)
        {
            this.mainTimer = mainTimer;
            myGUIUpdate = new MyGUIUpdate(gui, eventManager);
            mainTimer.addFixedUpdateListener(myGUIUpdate);
        }

        public string getName()
        {
            return "MyGUIPlugin";
        }

        public DebugInterface getDebugInterface()
        {
            return null;
        }

        public void createDebugCommands(List<CommandManager> commands)
        {
            
        }

        public void destroyViewport()
        {
            ogreWindow.OgreRenderWindow.destroyViewport(vp);
            vp = null;
        }

        public void recreateViewport(RendererWindow window)
        {
            ogreWindow = window as OgreWindow;
            ogrePlatform.getRenderManager().setRenderWindow(ogreWindow.OgreRenderWindow);
            vp = ogreWindow.OgreRenderWindow.addViewport(camera, int.MaxValue, 0.0f, 0.0f, 1.0f, 1.0f);
            vp.setBackgroundColor(new Color(1.0f, 0.0f, 0.0f, 0.0f));
            vp.setClearEveryFrame(false);
        }

        public OgrePlatform OgrePlatform
        {
            get
            {
                return ogrePlatform;
            }
        }

        static MyGUIInterface()
        {
            LogFile = "MyGUI.log";
            OSTheme = "MyGUIPlugin.Resources.MyGUIPlugin_Windows.xml";
            MainTheme = "MyGUIPlugin.Resources.MyGUIPlugin_Main.xml";
        }

        /// <summary>
        /// The log file location for MyGUI. Set before initializing.
        /// </summary>
        public static String LogFile { get; set; }

        /// <summary>
        /// The OS Theme MyGUI. Set before initializing.
        /// Changes stuff depending on the os like window close button alignment
        /// and other things in the main theme.
        /// </summary>
        public static String OSTheme { get; set; }

        /// <summary>
        /// The main theme file to load. This is loaded after the OSTheme and
        /// will contain common items to all themes.
        /// </summary>
        public static String MainTheme { get; set; }

        /// <summary>
        /// The amount of time Smooth Show transitions should take.
        /// </summary>
        public static float SmoothShowDuration { get; set; }

        /// <summary>
        /// This event is fired before MyGUI renders.
        /// </summary>
        public event EventHandler RenderStarted
        {
            add
            {
                renderListener.RenderStarted += value;
            }
            remove
            {
                renderListener.RenderStarted -= value;
            }
        }

        /// <summary>
        /// This event is fired after MyGUI renders.
        /// </summary>
        public event EventHandler RenderEnded
        {
            add
            {
                renderListener.RenderEnded += value;
            }
            remove
            {
                renderListener.RenderEnded -= value;
            }
        }
    }
}
