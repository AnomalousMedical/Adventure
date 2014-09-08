﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Platform;
using Engine;
using Logging;

namespace Editor
{
    enum CameraEvents
    {
        RotateCamera,
        PanCamera,
        ZoomCamera,
    }

    public class OrbitCameraController : UpdateListener
    {
        #region Static

        private static readonly ButtonEvent RotateCamera;
        private static readonly ButtonEvent PanCamera;
        private static readonly ButtonEvent ZoomCamera;

        static OrbitCameraController()
        {
            RotateCamera = new ButtonEvent(CameraEvents.RotateCamera, EventLayers.Main);
            RotateCamera.addButton(MouseButtonCode.MB_BUTTON1);
            DefaultEvents.registerDefaultEvent(RotateCamera);

            PanCamera = new ButtonEvent(CameraEvents.PanCamera, EventLayers.Main);
            PanCamera.addButton(MouseButtonCode.MB_BUTTON1);
            PanCamera.addButton(KeyboardButtonCode.KC_LCONTROL);
            DefaultEvents.registerDefaultEvent(PanCamera);

            ZoomCamera = new ButtonEvent(CameraEvents.ZoomCamera, EventLayers.Main);
            ZoomCamera.addButton(MouseButtonCode.MB_BUTTON1);
            ZoomCamera.addButton(KeyboardButtonCode.KC_LMENU);
            DefaultEvents.registerDefaultEvent(ZoomCamera);
        }

        private const float HALF_PI = (float)Math.PI / 2.0f - 0.001f;
        private const int SCROLL_SCALE = 5;

        #endregion Static

        private SceneView camera;
        private EventLayer events;

        //These three vectors form the axis relative to the current rotation.
        private Vector3 normalDirection; //z
        private Vector3 rotatedLeft; //x
        private Vector3 rotatedUp; //y
        
        private float orbitDistance;
        private float yaw;
        private float pitch;
        private bool currentlyInMotion;
        private Vector3 lookAt;
        private Vector3 translation;
        private CameraMotionValidator motionValidator = null;

        public OrbitCameraController(SceneView camera, EventLayer events)
        {
            this.camera = camera;
            this.events = events;
            translation = camera.Translation;
            lookAt = camera.LookAt;
            computeStartingValues(translation - lookAt);
        }

        public OrbitCameraController(Vector3 translation, Vector3 lookAt, EventManager eventManager)
        {
            this.camera = null;
            this.events = eventManager[EventLayers.Main];
            this.translation = translation;
            this.lookAt = lookAt;
            computeStartingValues(translation - lookAt);
        }

        public void sendUpdate(Clock clock)
        {
            if (camera != null)
            {
                IntVector3 mouseCoords = events.Mouse.AbsolutePosition;
                bool activeWindow = motionValidator == null || (motionValidator.allowMotion((int)mouseCoords.x, (int)mouseCoords.y) && motionValidator.isActiveWindow());
                if (RotateCamera.FirstFrameDown)
                {
                    if (activeWindow)
                    {
                        currentlyInMotion = true;
                    }
                }
                else if (RotateCamera.FirstFrameUp)
                {
                    currentlyInMotion = false;
                }
                mouseCoords = events.Mouse.RelativePosition;
                if (currentlyInMotion)
                {
                    if (PanCamera.HeldDown)
                    {
                        lookAt += rotatedLeft * (mouseCoords.x / (events.Mouse.AreaWidth * SCROLL_SCALE) * orbitDistance);
                        lookAt += rotatedUp * (mouseCoords.y / (events.Mouse.AreaHeight * SCROLL_SCALE) * orbitDistance);
                        updateTranslation(lookAt + normalDirection * orbitDistance);
                    }
                    else if (ZoomCamera.HeldDown)
                    {
                        orbitDistance += mouseCoords.y;
                        if (orbitDistance < 0.0f)
                        {
                            orbitDistance = 0.0f;
                        }
                        //camera.setOrthoWindowHeight(orbitDistance);
                        updateTranslation(normalDirection * orbitDistance + lookAt);
                    }
                    else if (RotateCamera.HeldDown)
                    {
                        yaw += mouseCoords.x / -100.0f;
                        pitch += mouseCoords.y / 100.0f;
                        if (pitch > HALF_PI)
                        {
                            pitch = HALF_PI;
                        }
                        if (pitch < -HALF_PI)
                        {
                            pitch = -HALF_PI;
                        }

                        Quaternion yawRot = new Quaternion(Vector3.Up, yaw);
                        Quaternion pitchRot = new Quaternion(Vector3.Left, pitch);

                        Quaternion rotation = yawRot * pitchRot;
                        normalDirection = Quaternion.quatRotate(ref rotation, ref Vector3.Backward);
                        rotatedUp = Quaternion.quatRotate(ref rotation, ref Vector3.Up);
                        rotatedLeft = normalDirection.cross(ref rotatedUp);

                        updateTranslation(normalDirection * orbitDistance + lookAt);
                        camera.LookAt = lookAt;
                    }
                }
                if (activeWindow)
                {
                    if (mouseCoords.z != 0)
                    {
                        if (mouseCoords.z < 0)
                        {
                            orbitDistance += 3.6f;
                        }
                        else if (mouseCoords.z > 0)
                        {
                            orbitDistance -= 3.6f;
                            if (orbitDistance < 0.0f)
                            {
                                orbitDistance = 0.0f;
                            }
                        }
                        //camera.setOrthoWindowHeight(orbitDistance);
                        updateTranslation(normalDirection * orbitDistance + lookAt);
                    }
                }
            }
        }

        public void loopStarting()
        {
            
        }

        public void exceededMaxDelta()
        {
            
        }

        /// <summary>
        /// set the current camera for this controller. This can be set to null to disable the controller.
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        public void setCamera(SceneView camera)
        {
            this.camera = camera;
        }

        /// <summary>
        /// Set the camera to the given position looking at the given point.
        /// </summary>
        /// <param name="position">The position to set the camera at.</param>
        /// <param name="lookAt">The look at point of the camera.</param>
        public void setNewPosition(Vector3 position, Vector3 lookAt)
        {
            this.lookAt = lookAt;
            computeStartingValues(position - lookAt);
            //camera.setOrthoWindowHeight(orbitDistance);
            updateTranslation(normalDirection * orbitDistance + lookAt);
            camera.LookAt = lookAt;
        }

        /// <summary>
        /// Helper function to compute the starting orbitDistance, yaw, pitch,
        /// normalDirection and left values.
        /// </summary>
        /// <param name="localTrans">The translation of the camera relative to the look at point.</param>
        private void computeStartingValues(Vector3 localTrans)
        {
            //Compute the orbit distance, this is the distance from the location to the look at point.
            orbitDistance = localTrans.length();

            //Compute the yaw.
            float localY = localTrans.y;
            localTrans.y = 0;
            yaw = Vector3.Backward.angle(ref localTrans);
            if (localTrans.x < 0)
            {
                yaw = -yaw;
            }

            //Compute the pitch by rotating the local translation to -yaw.
            localTrans.y = localY;
            localTrans = Quaternion.quatRotate(new Quaternion(Vector3.Up, -yaw), localTrans);
            localTrans.x = 0;
            pitch = Vector3.Backward.angle(ref localTrans);
            if (localTrans.y < 0)
            {
                pitch = -pitch;
            }

            //Compute the normal direction and the left vector.
            Quaternion yawRot = new Quaternion(Vector3.Up, yaw);
            Quaternion pitchRot = new Quaternion(Vector3.Left, pitch);
            Quaternion rotation = yawRot * pitchRot;
            normalDirection = Quaternion.quatRotate(ref rotation, ref Vector3.Backward);
            rotatedUp = Quaternion.quatRotate(ref rotation, ref Vector3.Up);
            rotatedLeft = normalDirection.cross(ref rotatedUp);
        }

        private void updateTranslation(Vector3 translation)
        {
            camera.Translation = translation;
            this.translation = translation;
        }

        public Vector3 Translation
        {
            get
            {
                return translation;
            }
        }

        public Vector3 LookAt
        {
            get
            {
                return lookAt;
            }
        }

        public CameraMotionValidator MotionValidator
        {
            get
            {
                return motionValidator;
            }
            set
            {
                motionValidator = value;
            }
        }
    }
}
