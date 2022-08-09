using Adventure.Exploration.Menu;
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

            public WorldMapInstance Map { get; set; }

            public IWorldMapManager WorldMapManager { get; set; }
        }

        private readonly TLASInstanceData instanceData;
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
        private Vector3 cameraOffset = new Vector3(0, 3, -12);
        private Quaternion cameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 14f);
        private readonly WorldMapInstance map;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

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
            IBackgroundMusicPlayer backgroundMusicPlayer
        )
        {
            this.worldMapManager = description.WorldMapManager;
            this.map = description.Map;
            this.gamepadId = description.GamepadId;
            this.moveForward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_W });
            this.moveBackward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_S });
            this.moveRight = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_D });
            this.moveLeft = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_A });

            var scale = description.Scale;
            var halfScale = scale.y / 2f;

            this.currentPosition = persistence.Current.Player.AirshipPosition ?? persistence.Current.Player.WorldPosition ?? description.Translation + new Vector3(0f, halfScale, 0f);
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

            this.instanceData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Airship"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale)
            };

            CreatePhysics();

            if (persistence.Current.Player.InAirship)
            {
                TakeOff(null);
            }

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.cubeTexture = await textureManager.Checkout(new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Metal032_1K", reflective: true));

                var primaryHitShaderTask = primaryHitShaderFactory.Checkout();

                await Task.WhenAll
                (
                    cubeBLAS.WaitForLoad(),
                    primaryHitShaderTask
                );

                this.instanceData.pBLAS = cubeBLAS.Instance.BLAS.Obj;
                this.primaryHitShader = primaryHitShaderTask.Result;
                blasInstanceData = this.activeTextures.AddActiveTexture(this.cubeTexture);
                blasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(cubeTexture.NormalMapSRV != null, cubeTexture.PhysicalDescriptorMapSRV != null, cubeTexture.Reflective, cubeTexture.EmissiveSRV != null, false);
                rtInstances.AddTlasBuild(instanceData);
                rtInstances.AddShaderTableBinder(Bind);
            });
        }

        public void Dispose()
        {
            this.activeTextures.RemoveActiveTexture(this.cubeTexture);
            primaryHitShaderFactory.TryReturn(primaryHitShader);
            textureManager.TryReturn(cubeTexture);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(instanceData);

            eventManager.removeEvent(moveForward);
            eventManager.removeEvent(moveBackward);
            eventManager.removeEvent(moveLeft);
            eventManager.removeEvent(moveRight);

            eventLayer.OnUpdate -= EventLayer_OnUpdate; //Do have to remove this since its on the layer itself
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        public void SetTransform(in Vector3 trans, in Quaternion rot)
        {
            var hasPhysics = physicsCreated;
            if (hasPhysics)
            {
                DestroyPhysics();
            }
            this.instanceData.Transform = new InstanceMatrix(trans, rot);
            if (hasPhysics)
            {
                CreatePhysics();
            }
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            blasInstanceData.vertexOffset = cubeBLAS.Instance.VertexOffset;
            blasInstanceData.indexOffset = cubeBLAS.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &blasInstanceData)
            {
                primaryHitShader.BindSbt(instanceData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
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
            instanceData.Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale);
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

                this.persistence.Current.Player.AirshipPosition = this.currentPosition;

                instanceData.Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale);
                cameraMover.Position = currentPosition + cameraOffset;
                cameraMover.Orientation = cameraAngle;

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
