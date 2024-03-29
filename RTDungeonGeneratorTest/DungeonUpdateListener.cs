﻿using Anomalous.OSPlatform;
using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using Engine.CameraMovement;
using Engine.Platform;
using SharpGui;
using System;

namespace RTDungeonGeneratorTest
{
    class DungeonUpdateListener : UpdateListener, IDisposable
    {
        private readonly GraphicsEngine graphicsEngine;
        private readonly NativeOSWindow window;
        private readonly FirstPersonFlyCamera cameraControls;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IObjectResolverFactory objectResolverFactory;
        private readonly RayTracingRenderer renderer;
        private readonly RTInstances rtInstances;
        private readonly RTGui gui;
        private readonly IObjectResolver objectResolver;

        private SceneDungeon currentDungeon;
        private SharpButton nextScene = new SharpButton() { Text = "Next Scene" };
        private bool loadingLevel = false;
        private int currentSeed = 23;

        public DungeonUpdateListener
        (
            GraphicsEngine graphicsEngine,
            NativeOSWindow window,
            FirstPersonFlyCamera cameraControls,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            ICoroutineRunner coroutineRunner,
            IObjectResolverFactory objectResolverFactory,
            RayTracingRenderer renderer,
            RTInstances rtInstances,
            RTGui gui
        )
        {
            this.graphicsEngine = graphicsEngine;
            this.window = window;
            this.cameraControls = cameraControls;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.coroutineRunner = coroutineRunner;
            this.objectResolverFactory = objectResolverFactory;
            this.renderer = renderer;
            this.rtInstances = rtInstances;
            this.gui = gui;
            this.objectResolver = objectResolverFactory.Create();

            cameraControls.Position = new Vector3(0, 2, -11);
            Initialize();
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            coroutineRunner.RunTask(async () =>
            {
                loadingLevel = true;
                var dungeon = this.objectResolver.Resolve<SceneDungeon, SceneDungeon.Desc>(o =>
                {
                    o.Seed = currentSeed++;
                });
                await dungeon.WaitForLoad();
                currentDungeon?.RequestDestruction();
                currentDungeon = dungeon;
                loadingLevel = false;
            });
        }

        public void Dispose()
        {
            this.objectResolver.Dispose();
        }

        unsafe void Initialize()
        {
            SetupBepu();
        }

        private void SetupBepu()
        {
            
        }

        public void exceededMaxDelta()
        {

        }

        public void loopStarting()
        {

        }

        public unsafe void sendUpdate(Clock clock)
        {
            cameraControls.UpdateInput(clock);
            UpdateGui(clock);
            objectResolverFactory.Flush();
            Render();
        }

        private void UpdateGui(Clock clock)
        {
            sharpGui.Begin(clock);

            if (!loadingLevel)
            {
                var layout =
                    new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                    new MaxWidthLayout(scaleHelper.Scaled(300),
                    new ColumnLayout(nextScene) { Margin = new IntPad(10) }
                    ));
                var desiredSize = layout.GetDesiredSize(sharpGui);
                layout.SetRect(new IntRect(window.WindowWidth - desiredSize.Width, window.WindowHeight - desiredSize.Height, desiredSize.Width, desiredSize.Height));

                //Buttons
                if (sharpGui.Button(nextScene))
                {
                    LoadNextScene();
                }
            }

            gui.Update(clock);

            sharpGui.End();
        }

        private unsafe void Render()
        {
            var swapChain = graphicsEngine.SwapChain;
            var immediateContext = graphicsEngine.ImmediateContext;

            renderer.Render(rtInstances, cameraControls.Position, cameraControls.Orientation);

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

            swapChain.Present(1);
        }
    }
}
