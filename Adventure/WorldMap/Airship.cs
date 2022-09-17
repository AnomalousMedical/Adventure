using Adventure.Menu;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class Airship : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            public GamepadId GamepadId { get; set; } = GamepadId.Pad1;

            public EventLayers EventLayer { get; set; } = EventLayers.Airship;

            public EventLayers LandEventLayer { get; set; } = EventLayers.WorldMap;
        }

        private TLASInstanceData[] instanceData;
        private readonly CubeBLAS cubeBLAS;
        private readonly RTInstances<IWorldMapGameState> rtInstances;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RayTracingRenderer renderer;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly Persistence persistence;
        private PrimaryHitShader primaryHitShader;
        private CC0TextureResult cubeTexture;
        private BlasInstanceData blasInstanceData;
        private readonly IBepuScene<IWorldMapGameState> bepuScene;
        private readonly IContextMenu contextMenu;
        private readonly EventManager eventManager;
        private readonly CameraMover cameraMover;
        private readonly IDestructionRequest destructionRequest;
        private readonly IBackgroundMusicPlayer backgroundMusicPlayer;
        private readonly EventLayer eventLayer;
        private readonly EventLayer landEventLayer;
        private readonly ICollidableTypeIdentifier<IWorldMapGameState> collidableIdentifier;
        private readonly IWorldMapManager worldMapManager;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private TaskCompletionSource graphicsReady = new TaskCompletionSource();
        private bool graphicsActive = false;
        private Vector3 cameraOffset = new Vector3(0, 5, -12);
        private Quaternion cameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 8f);
        private WorldMapInstance map;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        private Rect worldRect;

        private GamepadId gamepadId;
        private bool allowJoystickInput = true;

        ButtonEvent moveForward;
        ButtonEvent moveBackward;
        ButtonEvent moveRight;
        ButtonEvent moveLeft;

        float moveSpeed = 10.0f;
        bool active = false;

        public bool Active => active;

        public Airship
        (
            Description description,
            CubeBLAS cubeBLAS,
            IScopedCoroutine coroutine,
            RTInstances<IWorldMapGameState> rtInstances,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RayTracingRenderer renderer,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            Persistence persistence,
            ICollidableTypeIdentifier<IWorldMapGameState> collidableIdentifier,
            IBepuScene<IWorldMapGameState> bepuScene,
            IContextMenu contextMenu,
            EventManager eventManager,
            CameraMover cameraMover,
            IDestructionRequest destructionRequest,
            IBackgroundMusicPlayer backgroundMusicPlayer,
            IWorldMapManager worldMapManager
        )
        {
            this.worldMapManager = worldMapManager;
            this.gamepadId = description.GamepadId;
            this.moveForward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_W });
            this.moveBackward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_S });
            this.moveRight = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_D });
            this.moveLeft = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_A });

            var scale = description.Scale;
            var halfScale = scale.y / 2f;

            this.currentPosition = persistence.Current.Player.AirshipPosition ?? description.Translation + new Vector3(0f, halfScale, 0f);
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

            this.cubeBLAS = cubeBLAS;
            this.rtInstances = rtInstances;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.renderer = renderer;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.persistence = persistence;
            this.collidableIdentifier = collidableIdentifier;
            this.bepuScene = bepuScene;
            this.contextMenu = contextMenu;
            this.eventManager = eventManager;
            this.cameraMover = cameraMover;
            this.destructionRequest = destructionRequest;
            this.backgroundMusicPlayer = backgroundMusicPlayer;

            //Events
            eventManager.addEvent(moveForward);
            eventManager.addEvent(moveBackward);
            eventManager.addEvent(moveLeft);
            eventManager.addEvent(moveRight);

            eventLayer = eventManager[description.EventLayer];
            eventLayer.OnUpdate += EventLayer_OnUpdate;

            landEventLayer = eventManager[description.LandEventLayer];
            SetupInput();

            this.instanceData = new TLASInstanceData[0];

            coroutine.RunTask(async () =>
            {
                try
                {
                    using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                    this.cubeTexture = await textureManager.Checkout(new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Metal032_1K", reflective: true));

                    var primaryHitShaderTask = primaryHitShaderFactory.Checkout();

                    await Task.WhenAll
                    (
                        cubeBLAS.WaitForLoad(),
                        primaryHitShaderTask
                    );

                    this.primaryHitShader = primaryHitShaderTask.Result;
                    blasInstanceData = this.activeTextures.AddActiveTexture(this.cubeTexture);
                    blasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(cubeTexture.NormalMapSRV != null, cubeTexture.PhysicalDescriptorMapSRV != null, cubeTexture.Reflective, cubeTexture.EmissiveSRV != null, false);

                    graphicsReady.SetResult();
                }
                catch (Exception ex)
                {
                    graphicsReady.SetException(ex);
                }
            });
        }

        public async Task SetMap(WorldMapInstance map)
        {
            DestroyPhysics();
            DestroyGraphics();

            this.map = map;
            worldRect = new Rect(0, 0, map.MapSize.x, map.MapSize.y);

            await graphicsReady.Task;

            this.currentPosition = persistence.Current.Player.AirshipPosition ?? map.AirshipStartPoint + new Vector3(0f, currentScale.y / 2f, 0f);

            CreateGraphics();

            if (persistence.Current.Player.InAirship)
            {
                TakeOff(null);
                cameraMover.SetPosition(this.currentPosition + this.cameraOffset, cameraAngle);
            }
            else
            {
                SyncGraphics();
                CreatePhysics();
            }
        }

        public void Dispose()
        {
            this.activeTextures.RemoveActiveTexture(this.cubeTexture);
            primaryHitShaderFactory.TryReturn(primaryHitShader);
            textureManager.TryReturn(cubeTexture);
            DestroyGraphics();
            eventManager.removeEvent(moveForward);
            eventManager.removeEvent(moveBackward);
            eventManager.removeEvent(moveLeft);
            eventManager.removeEvent(moveRight);

            eventLayer.OnUpdate -= EventLayer_OnUpdate; //Do have to remove this since its on the layer itself
        }

        private void CreateGraphics()
        {
            if(!graphicsActive && map != null)
            {
                graphicsActive = true;

                this.instanceData = new TLASInstanceData[map.Transforms.Length];
                for (var i = 0; i < instanceData.Length; i++)
                {
                    this.instanceData[i] = new TLASInstanceData()
                    {
                        InstanceName = RTId.CreateId("Airship"),
                        Mask = RtStructures.OPAQUE_GEOM_MASK,
                        Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale),
                        pBLAS = cubeBLAS.Instance.BLAS.Obj
                    };
                    rtInstances.AddTlasBuild(this.instanceData[i]);
                }

                rtInstances.AddShaderTableBinder(Bind);
            }
        }

        private void DestroyGraphics()
        {
            if (graphicsActive)
            {
                graphicsActive = false;

                foreach (var data in instanceData)
                {
                    rtInstances.RemoveTlasBuild(data);
                }
                rtInstances.RemoveShaderTableBinder(Bind);
            }
        }

        private void SyncGraphics()
        {
            var numTransforms = map.Transforms.Length;
            for(var i = 0; i < numTransforms; ++i)
            {
                instanceData[i].Transform = new InstanceMatrix(currentPosition + map.Transforms[i], currentOrientation, currentScale);
            }
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            blasInstanceData.vertexOffset = cubeBLAS.Instance.VertexOffset;
            blasInstanceData.indexOffset = cubeBLAS.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &blasInstanceData)
            {
                foreach (var data in instanceData)
                {
                    primaryHitShader.BindSbt(data.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
                }
            }
        }

        public void CreatePhysics()
        {
            if (!physicsCreated)
            {
                physicsCreated = true;
                var shape = new Box(0.25f, 1000, 0.25f); //TODO: Each one creates its own, try to load from resources
                shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

                staticHandle = bepuScene.Simulation.Statics.Add(
                    new StaticDescription(
                        currentPosition.ToSystemNumerics(),
                        Quaternion.Identity.ToSystemNumerics(),
                        new CollidableDescription(shapeIndex, 0.1f)));

                bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), collisionEvent: HandleCollision, endEvent: HandleCollisionEnd);
            }
        }

        public void DestroyPhysics()
        {
            if (physicsCreated)
            {
                physicsCreated = false;
                bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
                bepuScene.Simulation.Shapes.Remove(shapeIndex);
                bepuScene.Simulation.Statics.Remove(staticHandle);
            }
        }

        private void HandleCollision(CollisionEvent evt)
        {
            if (collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.A, out var player)
               || collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.B, out player))
            {
                contextMenu.HandleContext("Take Off", TakeOff, player.GamepadId);
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(TakeOff);
        }

        private void TakeOff(ContextMenuArgs args)
        {
            persistence.Current.Player.InAirship = true;
            contextMenu.ClearContext(TakeOff);
            eventLayer.makeFocusLayer();
            active = true;
            currentPosition.y = 3.14f;
            DestroyPhysics();
            SyncGraphics();
            worldMapManager.SetPlayerVisible(false);
            backgroundMusicPlayer.SetBattleTrack("Music/freepd/Fireworks - Alexander Nakarada.ogg");
        }

        private void Land(ContextMenuArgs args)
        {
            persistence.Current.Player.InAirship = false;
            contextMenu.ClearContext(Land);
            landEventLayer.makeFocusLayer();
            active = false;
            var cell = map.GetCellForLocation(currentPosition);
            var center = map.GetCellCenterpoint(cell);
            currentPosition = center;
            currentPosition.y += currentScale.y / 2.0f;
            this.persistence.Current.Player.AirshipPosition = this.currentPosition;
            SyncGraphics();
            CreatePhysics();
            worldMapManager.MovePlayer(center + new Vector3(0f, 0f, -0.35f));
            worldMapManager.SetPlayerVisible(true);
            backgroundMusicPlayer.SetBattleTrack(null);
        }

        private void SetupInput()
        {
            //These events are owned by this class, so don't have to unsubscribe
            moveForward.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveForward.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveBackward.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveBackward.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveLeft.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveLeft.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveRight.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveRight.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
        }

        private void EventLayer_OnUpdate(EventLayer eventLayer)
        {
            if (eventLayer.EventProcessingAllowed)
            {
                if (allowJoystickInput)
                {
                    var pad = eventLayer.getGamepad(gamepadId);
                    var movementDir = pad.LStick;
                    //characterMover.movementDirection = movementDir.ToSystemNumerics();
                }

                eventLayer.alertEventsHandled();
            }
        }

        public void UpdateInput(Clock clock)
        {
            if (active)
            {
                Vector2 lStick = new Vector2();
                bool readJoystick = true;
                if (moveForward.Down)
                {
                    lStick.y = 1f;
                    readJoystick = false;
                }

                if (moveBackward.Down)
                {
                    lStick.y = -1f;
                    readJoystick = false;
                }

                if (moveLeft.Down)
                {
                    lStick.x = -1f;
                    readJoystick = false;
                }

                if (moveRight.Down)
                {
                    lStick.x = 1f;
                    readJoystick = false;
                }

                if (readJoystick)
                {
                    var pad = eventLayer.getGamepad(gamepadId);
                    lStick = pad.LStick;
                }
                else
                {
                    lStick.normalize();
                }

                currentPosition += Vector3.Forward * lStick.y * clock.DeltaSeconds * moveSpeed;
                currentPosition -= Vector3.Left * lStick.x * clock.DeltaSeconds * moveSpeed;

                var offset = new Vector3(0, 0, 0);
                if(currentPosition.x < worldRect.Left)
                {
                    offset.x = worldRect.Width;
                }
                else if (currentPosition.x > worldRect.Right)
                {
                    offset.x = -worldRect.Width;
                }

                //There is a quadrandt mismatch, rects are in the third and the world is in the first
                if(currentPosition.z < worldRect.Top)
                {
                    offset.z = worldRect.Height;
                }
                if(currentPosition.z > worldRect.Bottom)
                {
                    offset.z = -worldRect.Height;
                }

                currentPosition += offset;
                cameraMover.OffsetCurrentPosition(offset);

                this.persistence.Current.Player.AirshipPosition = this.currentPosition;

                SyncGraphics();
                cameraMover.SetInterpolatedGoalPosition(currentPosition + cameraOffset, cameraAngle);

                var cell = map.GetCellForLocation(currentPosition);
                if (map.CanLand(cell))
                {
                    contextMenu.HandleContext("Land", Land, gamepadId);
                }
                else
                {
                    contextMenu.ClearContext(Land);
                }
            }
        }
    }
}
