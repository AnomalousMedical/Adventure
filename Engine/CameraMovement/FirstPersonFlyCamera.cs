﻿using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.CameraMovement
{
    public class FirstPersonFlyCamera : IDisposable
    {
        public class Description
        {
            public Object EventLayer { get; set; } = EventLayers.Default;
        }

        private const float HALF_PI = MathFloat.PI / 2.0f - 0.001f;
        private readonly EventManager eventManager;
        private readonly IScaleHelper scaleHelper;
        Vector3 camPos = Vector3.Zero;
        Quaternion camRot = Quaternion.Identity;
        float yaw = 0;
        float pitch = 0;
        float moveSpeed = 10.0f;
        float viewSpeed = 1.0f;
        float rStickSensitivity = 3;

        float xSensitivity = 0.005f;
        float ySensitivity = 0.005f;

        Vector3 currentForward = Vector3.Forward;
        Vector3 currentLeft = Vector3.Left;

        ButtonEvent moveForward;
        ButtonEvent moveBackward;
        ButtonEvent moveLeft;
        ButtonEvent moveRight;
        ButtonEvent moveUp;
        ButtonEvent moveDown;
        ButtonEvent moveUpPad;
        ButtonEvent moveDownPad;
        ButtonEvent pitchUp;

        ButtonEvent pitchDown;
        ButtonEvent yawLeft;
        ButtonEvent yawRight;

        ButtonEvent mouseLook;

        public FirstPersonFlyCamera(EventManager eventManager, Description description, IScaleHelper scaleHelper)
        {
            moveForward = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_W });
            moveBackward = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_S });
            moveLeft = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_A });
            moveRight = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_D });
            moveUp = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_E });
            moveDown = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_Q });
            moveUpPad = new ButtonEvent(description.EventLayer, gamepadButtons: new[] { GamepadButtonCode.XInput_RTrigger });
            moveDownPad = new ButtonEvent(description.EventLayer, gamepadButtons: new[] { GamepadButtonCode.XInput_LTrigger });
            pitchUp = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_UP });

            pitchDown = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_DOWN });
            yawLeft = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_LEFT });
            yawRight = new ButtonEvent(description.EventLayer, keys: new [] { KeyboardButtonCode.KC_RIGHT });

            mouseLook = new ButtonEvent(description.EventLayer, mouseButtons: new MouseButtonCode[] { MouseButtonCode.MB_BUTTON1 });

            eventManager.addEvent(moveForward);
            eventManager.addEvent(moveBackward);
            eventManager.addEvent(moveLeft);
            eventManager.addEvent(moveRight);
            eventManager.addEvent(moveUp);
            eventManager.addEvent(moveDown);
            eventManager.addEvent(moveUpPad);
            eventManager.addEvent(moveDownPad);
            eventManager.addEvent(pitchUp);
            eventManager.addEvent(pitchDown);
            eventManager.addEvent(yawLeft);
            eventManager.addEvent(yawRight);
            eventManager.addEvent(mouseLook);
            this.eventManager = eventManager;
            this.scaleHelper = scaleHelper;
        }

        public void Dispose()
        {
            eventManager.removeEvent(moveForward);
            eventManager.removeEvent(moveBackward);
            eventManager.removeEvent(moveLeft);
            eventManager.removeEvent(moveRight);
            eventManager.removeEvent(moveUp);
            eventManager.removeEvent(moveDown);
            eventManager.removeEvent(moveUpPad);
            eventManager.removeEvent(moveDownPad);
            eventManager.removeEvent(pitchUp);
            eventManager.removeEvent(pitchDown);
            eventManager.removeEvent(yawLeft);
            eventManager.removeEvent(yawRight);
            eventManager.removeEvent(mouseLook);
        }

        public void UpdateInput(Clock clock)
        {
            bool updateRotation = false;

            if (pitchUp.Down)
            {
                pitch += clock.DeltaSeconds * viewSpeed;
                updateRotation = true;
            }

            if (pitchDown.Down)
            {
                pitch -= clock.DeltaSeconds * viewSpeed;
                updateRotation = true;
            }

            if (yawLeft.Down)
            {
                yaw -= clock.DeltaSeconds * viewSpeed;
                updateRotation = true;
            }

            if (yawRight.Down)
            {
                yaw += clock.DeltaSeconds * viewSpeed;
                updateRotation = true;
            }

            if (mouseLook.Down)
            {
                var mousePos = eventManager.Mouse.RelativePosition;
                if (mousePos != IntVector3.Zero)
                {
                    updateRotation = true;
                    
                    mousePos.x = Math.Min(mousePos.x, scaleHelper.Scaled(10));
                    mousePos.y = Math.Min(mousePos.y, scaleHelper.Scaled(10));
                    
                    yaw += mousePos.x * xSensitivity;
                    pitch -= mousePos.y * ySensitivity;
                }
            }

            var rStick = eventManager.Pad1.RStick;
            if (rStick.x != 0 || rStick.y != 0)
            {
                updateRotation = true;
                yaw += rStick.x * rStickSensitivity * clock.DeltaSeconds;
                pitch -= rStick.y * rStickSensitivity * clock.DeltaSeconds;
            }

            if (updateRotation)
            {
                if (pitch > HALF_PI)
                {
                    pitch = HALF_PI;
                }
                if (pitch < -HALF_PI)
                {
                    pitch = -HALF_PI;
                }

                var yawRot = new Quaternion(Vector3.Up, yaw);
                var pitchRot = new Quaternion(Vector3.Left, pitch);
                camRot = yawRot * pitchRot;

                currentForward = Quaternion.quatRotate(camRot, Vector3.Forward);
                currentLeft = Quaternion.quatRotate(camRot, Vector3.Left);
            }

            var lStick = eventManager.Pad1.LStick;
            if (lStick.x != 0 || lStick.y != 0)
            {
                camPos += currentForward * lStick.y * clock.DeltaSeconds * moveSpeed;
                camPos -= currentLeft * lStick.x * clock.DeltaSeconds * moveSpeed;
            }

            if (moveForward.Down)
            {
                camPos += currentForward * clock.DeltaSeconds * moveSpeed;
            }

            if (moveBackward.Down)
            {
                camPos -= currentForward * clock.DeltaSeconds * moveSpeed;
            }

            if (moveLeft.Down)
            {
                camPos += currentLeft * clock.DeltaSeconds * moveSpeed;
            }

            if (moveRight.Down)
            {
                camPos -= currentLeft * clock.DeltaSeconds * moveSpeed;
            }

            if (moveUp.Down || moveUpPad.Down)
            {
                camPos += Vector3.Up * clock.DeltaSeconds * moveSpeed;
            }

            if (moveDown.Down || moveDownPad.Down)
            {
                camPos += Vector3.Down * clock.DeltaSeconds * moveSpeed;
            }
        }

        public Vector3 Position
        {
            get
            {
                return camPos;
            }
            set
            {
                camPos = value;
            }
        }

        public Quaternion Orientation => camRot;
    }
}