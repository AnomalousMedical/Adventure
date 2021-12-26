﻿using Anomalous.OSPlatform;
using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using Engine.CameraMovement;
using Engine.Platform;
using SceneTest.Exploration.Menu;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTest
{
    class SceneTestUpdateListener : UpdateListener
    {
        private readonly RayTracingRenderer rayTracingRenderer;
        private readonly ITimeClock timeClock;
        private readonly ISharpGui sharpGui;
        private readonly ISwapChain swapChain;
        private readonly IDeviceContext immediateContext;

        private readonly IObjectResolverFactory objectResolverFactory;
        private readonly CameraMover cameraMover;
        private readonly Sky sky;
        private readonly FirstPersonFlyCamera flyCamera;
        private readonly RTGui rtGui;
        private IGameState gameState;

        public unsafe SceneTestUpdateListener
        (
            GraphicsEngine graphicsEngine,
            RayTracingRenderer rayTracingRenderer,
            ITimeClock timeClock,
            ISharpGui sharpGui,
            IObjectResolverFactory objectResolverFactory,
            CameraMover cameraMover,
            Sky sky,
            IFirstGameStateBuilder startState,
            FirstPersonFlyCamera flyCamera,
            RTGui rtGui
        )
        {
            flyCamera.Position = new Vector3(0, 0, -10);

            this.swapChain = graphicsEngine.SwapChain;
            this.immediateContext = graphicsEngine.ImmediateContext;
            this.rayTracingRenderer = rayTracingRenderer;
            this.timeClock = timeClock;
            this.sharpGui = sharpGui;
            this.objectResolverFactory = objectResolverFactory;
            this.cameraMover = cameraMover;
            this.sky = sky;
            this.flyCamera = flyCamera;
            this.rtGui = rtGui;
            this.gameState = startState.GetFirstGameState();
            this.gameState.SetActive(true);
        }

        public void exceededMaxDelta()
        {

        }

        public void loopStarting()
        {

        }

        public unsafe void sendUpdate(Clock clock)
        {

            //Update
            //flyCamera.UpdateInput(clock);
            timeClock.Update(clock);
            sharpGui.Begin(clock);
            var nextState = this.gameState.Update(clock);
            if (nextState != this.gameState)
            {
                this.gameState.SetActive(false);
                nextState.SetActive(true);
                this.gameState = nextState;
            }
            //rtGui.Update(clock);
            sharpGui.End();
            sky.UpdateLight(clock);

            var rtInstances = this.gameState.Instances;

            rtInstances.UpdateSprites(clock);

            //pbrRenderAttribs.AverageLogLum = sky.AverageLogLum;
            //Upate sun here
            var sunPos = cameraMover.SceneCenter + new Vector3(sky.SunPosition.x, sky.SunPosition.y, sky.SunPosition.z);
            var moonPos = cameraMover.SceneCenter + new Vector3(sky.MoonPosition.x, sky.MoonPosition.y, sky.MoonPosition.z);
            rayTracingRenderer.Render(rtInstances, cameraMover.Position, cameraMover.Orientation, new Vector4(sunPos.x, sunPos.y, sunPos.z, 0), new Vector4(moonPos.x, moonPos.y, moonPos.z, 0));

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

            objectResolverFactory.Flush();
        }
    }
}
