﻿using Anomalous.OSPlatform;
using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using Engine.CameraMovement;
using Engine.Platform;
using Adventure.Exploration.Menu;
using Adventure.Services;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure
{
    class SceneTestUpdateListener : UpdateListener
    {
        private readonly RayTracingRenderer rayTracingRenderer;
        private readonly ITimeClock timeClock;
        private readonly ISharpGui sharpGui;
        private readonly ISwapChain swapChain;
        private readonly IDeviceContext immediateContext;

        private readonly IObjectResolverFactory objectResolverFactory;
        private readonly Sky sky;
        private readonly FlyCameraManager flyCameraManager;
        private readonly CameraMover cameraMover;
        private IGameState gameState;

        public unsafe SceneTestUpdateListener
        (
            GraphicsEngine graphicsEngine,
            RayTracingRenderer rayTracingRenderer,
            ITimeClock timeClock,
            ISharpGui sharpGui,
            IObjectResolverFactory objectResolverFactory,
            Sky sky,
            IFirstGameStateBuilder startState,
            ShaderPreloader shaderPreloader, //Need this injected to create it
            BackgroundMusicManager backgroundMusicManager, //Need this injected to create it
            FlyCameraManager flyCameraManager,
            CameraMover cameraMover
        )
        {

            this.swapChain = graphicsEngine.SwapChain;
            this.immediateContext = graphicsEngine.ImmediateContext;
            this.rayTracingRenderer = rayTracingRenderer;
            this.timeClock = timeClock;
            this.sharpGui = sharpGui;
            this.objectResolverFactory = objectResolverFactory;
            this.sky = sky;
            this.flyCameraManager = flyCameraManager;
            this.cameraMover = cameraMover;
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
            timeClock.Update(clock);
            sharpGui.Begin(clock);

            var nextState = this.gameState.Update(clock);
            if (nextState != this.gameState)
            {
                this.gameState.SetActive(false);
                nextState.SetActive(true);
                this.gameState = nextState;
            }

            sharpGui.End();
            sky.UpdateLight(clock);

            flyCameraManager.Update(clock);

            var rtInstances = this.gameState.Instances;

            rtInstances.UpdateSprites(clock);

            rayTracingRenderer.Render(rtInstances, cameraMover.Position, cameraMover.Orientation);

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
