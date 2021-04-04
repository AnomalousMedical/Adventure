﻿using Anomalous.OSPlatform;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.GltfPbr;
using Engine;
using Engine.CameraMovement;
using Engine.Platform;
using SharpGui;
using SoundPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTest
{
    class SceneTestUpdateListener : UpdateListener, IDisposable
    {
        //Camera Settings
        float YFov = MathFloat.PI / 4.0f;
        float ZNear = 0.1f;
        float ZFar = 350f;

        private readonly NativeOSWindow window;
        private readonly EnvironmentMapBuilder envMapBuilder;
        private readonly IPbrCameraAndLight pbrCameraAndLight;
        private readonly VirtualFileSystem virtualFileSystem;
        private readonly FirstPersonFlyCamera cameraControls;
        private readonly SimpleShadowMapRenderer shadowMapRenderer;
        private readonly ITimeClock timeClock;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly ISwapChain swapChain;
        private readonly IRenderDevice renderDevice;
        private readonly IDeviceContext immediateContext;
        private readonly PbrRenderAttribs pbrRenderAttribs = PbrRenderAttribs.CreateDefault();

        private readonly SoundManager soundManager;
        private readonly SpriteManager sprites;
        private readonly IObjectResolverFactory objectResolverFactory;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly CameraMover cameraMover;
        private readonly ILevelManager levelManager;
        private readonly Sky sky;
        private readonly IObjectResolver objectResolver;

        private IGameState gameState;

        private PbrRenderer pbrRenderer;
        private AutoPtr<ITextureView> environmentMapSRV;

        private bool useFirstPersonCamera = false;

        public unsafe SceneTestUpdateListener(
            GraphicsEngine graphicsEngine,
            NativeOSWindow window,
            PbrRenderer m_GLTFRenderer,
            EnvironmentMapBuilder envMapBuilder,
            IPbrCameraAndLight pbrCameraAndLight,
            VirtualFileSystem virtualFileSystem,
            FirstPersonFlyCamera cameraControls,
            SimpleShadowMapRenderer shadowMapRenderer,
            ITimeClock timeClock,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            SoundManager soundManager,
            SpriteManager sprites,
            IObjectResolverFactory objectResolverFactory,
            ICoroutineRunner coroutineRunner,
            CameraMover cameraMover,
            ILevelManager levelManager,
            Sky sky,
            IFirstGameStateBuilder startState)
        {
            cameraControls.Position = new Vector3(0, 0, -12);

            this.pbrRenderer = m_GLTFRenderer;
            this.swapChain = graphicsEngine.SwapChain;
            this.renderDevice = graphicsEngine.RenderDevice;
            this.immediateContext = graphicsEngine.ImmediateContext;
            this.window = window;
            this.envMapBuilder = envMapBuilder;
            this.pbrCameraAndLight = pbrCameraAndLight;
            this.virtualFileSystem = virtualFileSystem;
            this.cameraControls = cameraControls;
            this.shadowMapRenderer = shadowMapRenderer;
            this.timeClock = timeClock;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.soundManager = soundManager;
            this.sprites = sprites;
            this.objectResolverFactory = objectResolverFactory;
            this.coroutineRunner = coroutineRunner;
            this.cameraMover = cameraMover;
            this.levelManager = levelManager;
            this.sky = sky;
            this.gameState = startState.GetFirstGameState();
            this.objectResolver = objectResolverFactory.Create();
            
            coroutineRunner.RunTask(Initialize());
        }

        public void Dispose()
        {
            objectResolver.Dispose();
            environmentMapSRV.Dispose();
        }

        async Task Initialize()
        {
            environmentMapSRV = envMapBuilder.BuildEnvMapView(renderDevice, immediateContext, "papermill/Fixed-", "png");

            pbrRenderer.PrecomputeCubemaps(renderDevice, immediateContext, environmentMapSRV.Obj);

            await levelManager.Initialize();
        }

        public void exceededMaxDelta()
        {

        }

        public void loopStarting()
        {

        }

        private void UpdateSprites(Clock clock)
        {
            foreach (var sprite in sprites)
            {
                sprite.Update(clock);
            }
        }

        public unsafe void sendUpdate(Clock clock)
        {
            IEnumerable<SceneObject> sceneObjects = gameState.SceneObjects;

            //Update
            timeClock.Update(clock);
            sharpGui.Begin(clock);
            var nextState = this.gameState.Update(clock);
            if(nextState != this.gameState)
            {
                this.gameState.SetActive(false);
                nextState.SetActive(true);
                this.gameState = nextState;
            }
            sharpGui.End();
            sky.UpdateLight(clock);
            UpdateSprites(clock);

            pbrRenderAttribs.AverageLogLum = sky.AverageLogLum;

            objectResolverFactory.Flush();

            //Render
            var pRTV = swapChain.GetCurrentBackBufferRTV();
            var pDSV = swapChain.GetDepthBufferDSV();
            var PreTransform = swapChain.GetDesc_PreTransform;
            var preTransformMatrix = CameraHelpers.GetSurfacePretransformMatrix(new Vector3(0, 0, 1), PreTransform);
            var cameraProjMatrix = CameraHelpers.GetAdjustedProjectionMatrix(YFov, ZNear, ZFar, window.WindowWidth, window.WindowHeight, PreTransform);

            //Draw Scene
            // Render shadow map
            shadowMapRenderer.BeginShadowMap(renderDevice, immediateContext, sky.LightDirection, Vector3.Zero, 90); //Centering scene on player seems to work the best
            foreach (var sceneObj in sceneObjects.Where(i => i.RenderShadow))
            {
                var pos = sceneObj.position - cameraMover.SceneCenter;
                shadowMapRenderer.RenderShadowMap(immediateContext, sceneObj.vertexBuffer, sceneObj.skinVertexBuffer, sceneObj.indexBuffer, sceneObj.numIndices, ref pos, ref sceneObj.orientation, ref sceneObj.scale);
            }

            //Render scene colors
            pbrRenderer.Begin(immediateContext);
            immediateContext.SetRenderTarget(pRTV, pDSV, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            // Clear the back buffer
            immediateContext.ClearRenderTarget(pRTV, sky.ClearColor, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            immediateContext.ClearDepthStencil(pDSV, CLEAR_DEPTH_STENCIL_FLAGS.CLEAR_DEPTH_FLAG, 1f, 0, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            // Set Light
            var WorldToShadowMapUVDepthMatrix = shadowMapRenderer.WorldToShadowMapUVDepthMatr;
            pbrCameraAndLight.SetLightAndShadow(sky.LightDirection, sky.LightColor, sky.LightIntensity, WorldToShadowMapUVDepthMatrix);

            // Set Camera
            if (useFirstPersonCamera)
            {
                cameraControls.UpdateInput(clock);
                pbrCameraAndLight.SetCameraPosition(cameraControls.Position, cameraControls.Orientation, preTransformMatrix, cameraProjMatrix);
            }
            else
            {
                pbrCameraAndLight.SetCameraPosition(cameraMover.Position - cameraMover.SceneCenter, cameraMover.Orientation, preTransformMatrix, cameraProjMatrix);
            }

            foreach (var sceneObj in sceneObjects)
            {
                pbrRenderAttribs.AlphaMode = sceneObj.pbrAlphaMode;
                pbrRenderAttribs.GetShadows = sceneObj.GetShadows;
                pbrRenderAttribs.IsSprite = sceneObj.Sprite != null;
                if (pbrRenderAttribs.IsSprite)
                {
                    var frame = sceneObj.Sprite.GetCurrentFrame();
                    pbrRenderAttribs.SpriteUVLeft = frame.Left;
                    pbrRenderAttribs.SpriteUVTop = frame.Top;
                    pbrRenderAttribs.SpriteUVRight = frame.Right;
                    pbrRenderAttribs.SpriteUVBottom = frame.Bottom;
                }

                var pos = sceneObj.position - cameraMover.SceneCenter;

                pbrRenderer.Render(immediateContext, sceneObj.shaderResourceBinding, sceneObj.vertexBuffer, sceneObj.skinVertexBuffer, sceneObj.indexBuffer, sceneObj.numIndices, ref pos, ref sceneObj.orientation, ref sceneObj.scale, pbrRenderAttribs);
            }

            this.shadowMapRenderer.RenderShadowMapVis(immediateContext);

            RenderGui();

            this.swapChain.Present(1);
        }

        private void RenderGui()
        {
            //Draw the gui
            var pDSV = swapChain.GetDepthBufferDSV();
            immediateContext.ClearDepthStencil(pDSV, CLEAR_DEPTH_STENCIL_FLAGS.CLEAR_DEPTH_FLAG, 1.0f, 0, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            sharpGui.Render(immediateContext);
        }
    }
}
