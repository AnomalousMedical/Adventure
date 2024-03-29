﻿using Anomalous.OSPlatform;
using DiligentEngine;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using BepuPhysics.Collidables;
using Microsoft.Extensions.Logging;
using Engine.CameraMovement;
using DiligentEngine.RT;
using BepuPlugin;
using SharpGui;

namespace RTBepuDemo
{
    class BepuUpdateListener : UpdateListener, IDisposable
    {
        private readonly NativeOSWindow window;
        private readonly RayTracingRenderer renderer;
        private readonly FirstPersonFlyCamera cameraControls;
        private readonly GraphicsEngine graphicsEngine;
        private readonly IObjectResolverFactory objectResolverFactory;
        private readonly IBepuScene<BepuUpdateListener> bepuScene;
        private readonly RTGui gui;
        private readonly ISharpGui sharpGui;
        private readonly RTInstances rtInstances;
        private IObjectResolver objectResolver;
        private List<BodyPositionSync> bodyPositionSyncs = new List<BodyPositionSync>();

        private long nextCubeSpawnTime = long.MaxValue;
        private long nextCubeSpawnFrequency = 1 * Clock.SecondsToMicro;

        Box boxShape;
        BodyInertia boxInertia;
        //END

        public BepuUpdateListener(
            NativeOSWindow window,
            RayTracingRenderer renderer,
            FirstPersonFlyCamera cameraControls,
            GraphicsEngine graphicsEngine,
            IObjectResolverFactory objectResolverFactory,
            IBepuScene<BepuUpdateListener> bepuScene,
            RTGui gui,
            ISharpGui sharpGui,
            CubeBLAS cubeBLAS,
            ICoroutineRunner coroutine,
            RTInstances rtInstances
        )
        {
            this.window = window;
            this.renderer = renderer;
            this.cameraControls = cameraControls;
            this.graphicsEngine = graphicsEngine;
            this.objectResolverFactory = objectResolverFactory;
            this.bepuScene = bepuScene;
            this.gui = gui;
            this.sharpGui = sharpGui;
            this.rtInstances = rtInstances;
            coroutine.RunTask(async () =>
            {
                await cubeBLAS.WaitForLoad();

                cameraControls.Position = new Vector3(0, 2, -11);
                SetupBepu();
                this.objectResolver = objectResolverFactory.Create();

                objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
                {

                });

                for (var x = -20; x < 20; ++x)
                {
                    for (var z = -20; z < 20; ++z)
                    {
                        objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
                        {
                            o.Transform = new InstanceMatrix(new Vector3(x, -1.5f, z), Quaternion.Identity);
                            o.TextureIndex = 4;
                        });
                    }
                }

                nextCubeSpawnTime = 0; //Start cubes spawining
            });
        }

        public void Dispose()
        {
            this.objectResolver.Dispose();
        }

        private void SetupBepu()
        {
            var simulation = bepuScene.Simulation;

            //Drop boxes on a big static box.
            boxShape = new Box(1, 1, 1);
            boxInertia = boxShape.ComputeInertia(1);

            simulation.Statics.Add(new StaticDescription(new System.Numerics.Vector3(0, 0, 0), simulation.Shapes.Add(new Box(1, 1, 1))));

            simulation.Statics.Add(new StaticDescription(new System.Numerics.Vector3(0, -6, 0), simulation.Shapes.Add(new Box(100, 10, 100))));
        }

        private void CreateBox(Box box, BodyInertia boxInertia, System.Numerics.Vector3 position)
        {
            var body = objectResolver.Resolve<BodyPositionSync, BodyPositionSync.Desc>(o =>
            {
                o.position = position;
                o.box = box;
                o.boxInertia = boxInertia;
                o.IsGlass = true;
            });

            bodyPositionSyncs.Add(body);
        }

        public void exceededMaxDelta()
        {

        }

        public void loopStarting()
        {

        }

        public unsafe void sendUpdate(Clock clock)
        {
            var swapChain = graphicsEngine.SwapChain;
            var immediateContext = graphicsEngine.ImmediateContext;

            cameraControls.UpdateInput(clock);
            UpdatePhysics(clock);
            sharpGui.Begin(clock);
            gui.Update(clock);
            sharpGui.End();
            objectResolverFactory.Flush();

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

        private unsafe void UpdatePhysics(Clock clock)
        {
            if (clock.CurrentTimeMicro > nextCubeSpawnTime)
            {
                float highest = 2;
                //Find highest box
                foreach(var body in bodyPositionSyncs) //Need a more generic way to handle the instances, more like the other method
                {
                    var world = body.GetWorldPosition();
                    world.y += 1.5f;
                    if(world.y > highest)
                    {
                        highest = world.y;
                    }
                }

                CreateBox(boxShape, boxInertia, new System.Numerics.Vector3(-0.8f, highest, 0.8f));
                nextCubeSpawnTime += nextCubeSpawnFrequency;
                window.Title = $"RTBepuDemo - {bodyPositionSyncs.Count} boxes.";
            }

            bepuScene.Update(clock, new System.Numerics.Vector3(0, 0, 1));

            foreach (var body in bodyPositionSyncs)
            {
                body.SyncPhysics(bepuScene);
            }
        }
    }
}
