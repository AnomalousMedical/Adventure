﻿using Anomalous.OSPlatform;
using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using Engine.CameraMovement;
using Engine.Platform;
using FreeImageAPI;
using SharpGui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSandbox
{
    class RTSandboxUpdateListener : UpdateListener
    {
        private readonly FirstPersonFlyCamera cameraControls;
        private readonly SandboxScene scene;
        private readonly RayTracingRenderer rayTracingRenderer;
        private readonly ISharpGui sharpGui;
        private readonly RTGui gui;
        private readonly RTInstances rtInstances;
        private readonly ISwapChain swapChain;
        private readonly IDeviceContext immediateContext;

        public unsafe RTSandboxUpdateListener
        (
            GraphicsEngine graphicsEngine,
            FirstPersonFlyCamera cameraControls,
            SandboxScene scene,
            RayTracingRenderer rayTracingRenderer,
            ISharpGui sharpGui,
            RTGui gui,
            RTInstances rtInstances
        )
        {
            this.cameraControls = cameraControls;
            this.scene = scene;
            this.rayTracingRenderer = rayTracingRenderer;
            this.sharpGui = sharpGui;
            this.gui = gui;
            this.rtInstances = rtInstances;
            this.swapChain = graphicsEngine.SwapChain;
            this.immediateContext = graphicsEngine.ImmediateContext;

            cameraControls.Position = new Vector3(0, 0, -10);
        }

        public void exceededMaxDelta()
        {

        }

        public void loopStarting()
        {

        }

        public void sendUpdate(Clock clock)
        {
            sharpGui.Begin(clock);
            gui.Update(clock);
            scene.Update(clock);
            cameraControls.UpdateInput(clock);
            sharpGui.End();

            rayTracingRenderer.Render(rtInstances, cameraControls.Position, cameraControls.Orientation);

            //This is the old clear loop, leaving in place in case we want or need the screen clear, but I think with pure rt there is no need
            //since we blit a texture to the full screen over and over.
            var pRTV = swapChain.GetCurrentBackBufferRTV();
            var pDSV = swapChain.GetDepthBufferDSV();
            immediateContext.SetRenderTarget(pRTV, pDSV, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            //var ClearColor = new Color(0.350f, 0.350f, 0.350f, 1.0f);

            // Clear the back buffer
            // Let the engine perform required state transitions
            //immediateContext.ClearRenderTarget(pRTV, ClearColor, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            immediateContext.ClearDepthStencil(pDSV, CLEAR_DEPTH_STENCIL_FLAGS.CLEAR_DEPTH_FLAG, 1.0f, 0, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            sharpGui.Render(immediateContext);

            this.swapChain.Present(1);
        }
    }
}
