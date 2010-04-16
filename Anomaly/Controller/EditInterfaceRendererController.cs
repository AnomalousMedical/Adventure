﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Renderer;
using Engine.Platform;
using Engine.ObjectManagement;
using Engine.Editing;
using Editor;
using Engine;

namespace Anomaly
{
    class EditInterfaceRendererController : UpdateListener
    {
        private RendererPlugin renderer;
        private DebugDrawingSurface debugSurface;
        private UpdateTimer timer;
        private EditInterfaceRenderer currentRenderer;
        private IObjectEditorGUI mainEditor;

        public EditInterfaceRendererController(RendererPlugin renderer, UpdateTimer timer, SceneController sceneController, IObjectEditorGUI mainEditor)
        {
            this.renderer = renderer;
            this.timer = timer;
            this.mainEditor = mainEditor;
            sceneController.OnSceneLoaded += new SceneControllerEvent(sceneController_OnSceneLoaded);
            sceneController.OnSceneUnloading += new SceneControllerEvent(sceneController_OnSceneUnloading);
            timer.addFixedUpdateListener(this);

            mainEditor.ActiveInterfaceChanged += new ObjectEditorGUIEvent(mainEditor_ActiveInterfaceChanged);
            mainEditor.FieldChanged += new ObjectEditorGUIEvent(mainEditor_FieldChanged);
            mainEditor.MainInterfaceChanged += new ObjectEditorGUIEvent(mainEditor_MainInterfaceChanged);
        }

        void mainEditor_MainInterfaceChanged(EditInterface editInterface, object editingObject)
        {
            //Check for an instance object, if it is found set the debug surface position to that instance.
            Instance instance = editingObject as Instance;
            if (instance != null)
            {
                debugSurface.moveOrigin(instance.Translation);
                debugSurface.setOrientation(instance.Definition.Rotation);
            }
            else
            {
                debugSurface.moveOrigin(Vector3.Zero);
                debugSurface.setOrientation(Quaternion.Identity);
            }
        }

        void mainEditor_FieldChanged(EditInterface editInterface, object editingObject)
        {
            if (currentRenderer != null)
            {
                currentRenderer.propertiesChanged(debugSurface);
            }
        }

        void mainEditor_ActiveInterfaceChanged(EditInterface editInterface, object editingObject)
        {
            if (currentRenderer != null)
            {
                currentRenderer.interfaceDeselected(debugSurface);
            }
            debugSurface.clearAll();
            currentRenderer = editInterface.Renderer;
            if (currentRenderer != null)
            {
                currentRenderer.interfaceSelected(debugSurface);
            }
        }

        void sceneController_OnSceneLoaded(SceneController controller, SimScene scene)
        {
            debugSurface = renderer.createDebugDrawingSurface("EditInterfaceRenderer", scene.getDefaultSubScene());
        }

        void sceneController_OnSceneUnloading(SceneController controller, Engine.ObjectManagement.SimScene scene)
        {
            renderer.destroyDebugDrawingSurface(debugSurface);
        }

        #region UpdateListener Members

        public void sendUpdate(Clock clock)
        {
            if (currentRenderer != null)
            {
                currentRenderer.frameUpdate(debugSurface);
            }
        }

        public void loopStarting()
        {
            
        }

        public void exceededMaxDelta()
        {
            
        }

        #endregion
    }
}
