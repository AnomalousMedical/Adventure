using Adventure.Assets.Music;
using Adventure.Assets.World;
using Adventure.Menu;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.ShaderSets;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class Airship : IDisposable, IAnimationListener
    {
        public class Description : SceneObjectDesc
        {
            public GamepadId GamepadId { get; set; } = GamepadId.Pad1;

            public EventLayers EventLayer { get; set; } = EventLayers.Airship;

            public EventLayers LandEventLayer { get; set; } = EventLayers.WorldMap;

            public ISprite Sprite { get; set; } = AirshipSprite.CreateSprite();

            public SpriteMaterialDescription SpriteMaterial { get; set; } = AirshipSprite.CreateMaterial();
        }

        public record Text
        (
            String Broken,
            String TakeOff,
            String Land
        );

        private const float AirshipOffset = 0.05f; //Other props work of the tenths place, so they won't intersect with this offset
        private SpriteInstance spriteInstance;
        private readonly EventSprite sprite;
        private TLASInstanceData instanceData;
        private readonly RTInstances<WorldMapScene> rtInstances;
        private readonly Persistence persistence;
        private readonly IBepuScene<WorldMapScene> bepuScene;
        private readonly IContextMenu contextMenu;
        private readonly EventManager eventManager;
        private readonly CameraMover cameraMover;
        private readonly IDestructionRequest destructionRequest;
        private readonly IBackgroundMusicPlayer backgroundMusicPlayer;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly EventLayer eventLayer;
        private readonly EventLayer landEventLayer;
        private readonly ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier;
        private readonly IWorldMapManager worldMapManager;
        private readonly ILanguageService languageService;
        private readonly IAnimationService<WorldMapScene> animationService;
        private readonly WrappingCharacterMenuPositionTracker<WorldMapScene> characterMenuPositionTracker;
        private readonly CharacterMenuPositionEntry characterMenuPositionEntry;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private TaskCompletionSource graphicsReady = new TaskCompletionSource();
        private bool graphicsActive = false;
        private Vector3 cameraOffset = new Vector3(0, 3, -12);
        private Quaternion cameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 15f);
        private WorldMapInstance map;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        private Rect worldRect;
        private Vector3 descriptionScale;

        private GamepadId gamepadId;

        ButtonEvent moveForward;
        ButtonEvent moveBackward;
        ButtonEvent moveRight;
        ButtonEvent moveLeft;

        float moveSpeed = 10.0f;
        bool active = false;

        private Vector3 floatOffset = new Vector3(0f, 0f, 0f);
        private float upDownAnimationAmount = 0.0f;
        private const float TwoPi = MathF.PI * 2;

        public bool Active => active;

        public Airship
        (
            Description description,
            IScopedCoroutine coroutine,
            RTInstances<WorldMapScene> rtInstances,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            Persistence persistence,
            ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier,
            IBepuScene<WorldMapScene> bepuScene,
            IContextMenu contextMenu,
            EventManager eventManager,
            CameraMover cameraMover,
            IDestructionRequest destructionRequest,
            IBackgroundMusicPlayer backgroundMusicPlayer,
            SpriteInstanceFactory spriteInstanceFactory,
            IWorldMapManager worldMapManager,
            ILanguageService languageService,
            IAnimationService<WorldMapScene> animationService,
            WrappingCharacterMenuPositionTracker<WorldMapScene> characterMenuPositionTracker
        )
        {
            this.sprite = new EventSprite(description.Sprite);
            this.worldMapManager = worldMapManager;
            this.languageService = languageService;
            this.animationService = animationService;
            this.characterMenuPositionTracker = characterMenuPositionTracker;
            this.gamepadId = description.GamepadId;
            this.moveForward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_W });
            this.moveBackward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_S });
            this.moveRight = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_D });
            this.moveLeft = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_A });

            var scale = descriptionScale = description.Scale;
            var halfScale = scale.y / 2f;

            this.currentPosition = persistence.Current.Player.AirshipPosition ?? description.Translation + new Vector3(0f, halfScale, AirshipOffset);
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * scale;

            this.rtInstances = rtInstances;
            this.persistence = persistence;
            this.collidableIdentifier = collidableIdentifier;
            this.bepuScene = bepuScene;
            this.contextMenu = contextMenu;
            this.eventManager = eventManager;
            this.cameraMover = cameraMover;
            this.destructionRequest = destructionRequest;
            this.backgroundMusicPlayer = backgroundMusicPlayer;
            this.spriteInstanceFactory = spriteInstanceFactory;

            characterMenuPositionEntry = new CharacterMenuPositionEntry(() => this.currentPosition + cameraOffset, () => this.cameraAngle, () =>
            {
                
            },
            GetMagicHitLocation: () => this.currentPosition + new Vector3(0f, 0f, -0.05f),
            GetScale: () => this.currentScale);
            this.characterMenuPositionTracker.SetOverrideEntry(characterMenuPositionEntry);

            //Events
            eventManager.addEvent(moveForward);
            eventManager.addEvent(moveBackward);
            eventManager.addEvent(moveLeft);
            eventManager.addEvent(moveRight);

            eventLayer = eventManager[description.EventLayer];
            landEventLayer = eventManager[description.LandEventLayer];

            this.instanceData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Airship"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale),
            };

            this.sprite.AnimationChanged += Sprite_AnimationChanged;

            coroutine.RunTask(async () =>
            {
                try
                {
                    using (var destructionBlock = destructionRequest.BlockDestruction()) //Block destruction until coroutine is finished and this is disposed.
                    {
                        this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);
                    }

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
            this.characterMenuPositionTracker.UnsetOverrideEntry(characterMenuPositionEntry);
            animationService.RemoveListener(this);
            this.sprite.AnimationChanged -= Sprite_AnimationChanged;
            StopAirshipMode();
            spriteInstanceFactory.TryReturn(spriteInstance);
            DestroyGraphics();
            eventManager.removeEvent(moveForward);
            eventManager.removeEvent(moveBackward);
            eventManager.removeEvent(moveLeft);
            eventManager.removeEvent(moveRight);
            DestroyPhysics();
        }

        public void CenterCamera()
        {
            if (persistence.Current.Player.InAirship)
            {
                cameraMover.SetPosition(this.currentPosition + this.cameraOffset, cameraAngle);
            }
        }

        private void CreateGraphics()
        {
            if (!graphicsActive && map != null)
            {
                graphicsActive = true;

                rtInstances.AddSprite(sprite, this.instanceData, spriteInstance);
                rtInstances.AddTlasBuild(this.instanceData);

                rtInstances.AddShaderTableBinder(Bind);
            }
        }

        private void DestroyGraphics()
        {
            if (graphicsActive)
            {
                graphicsActive = false;

                rtInstances.RemoveSprite(sprite);

                rtInstances.RemoveTlasBuild(instanceData);
                rtInstances.RemoveShaderTableBinder(Bind);
            }
        }

        private void SyncGraphics()
        {
            instanceData.Transform = new InstanceMatrix(currentPosition + map.Transforms[0] + floatOffset, currentOrientation, currentScale);
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.instanceData.InstanceName, sbt, tlas, sprite);
        }

        public void CreatePhysics()
        {
            if (!physicsCreated)
            {
                physicsCreated = true;
                var shape = new Box(0.75f, 1000, 0.25f); //TODO: Each one creates its own, try to load from resources
                shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

                staticHandle = bepuScene.Simulation.Statics.Add(
                    new StaticDescription(
                        currentPosition.ToSystemNumerics(),
                        Quaternion.Identity.ToSystemNumerics(),
                        shapeIndex));

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
                if (persistence.Current.PlotItems.Contains(PlotItems.AirshipKey))
                {
                    contextMenu.HandleContext(languageService.Current.Airship.TakeOff, TakeOff, player.GamepadId);
                }
                else
                {
                    contextMenu.HandleContext(languageService.Current.Airship.Broken, TakeOff, player.GamepadId);
                }
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(TakeOff);
        }

        private void TakeOff(ContextMenuArgs args)
        {
            if (persistence.Current.PlotItems.Contains(PlotItems.AirshipKey))
            {
                if (!active)
                {
                    persistence.Current.Player.InAirship = true;
                    contextMenu.ClearContext(TakeOff);
                    eventLayer.makeFocusLayer();
                    active = true;
                    worldMapManager.SetPlayerVisible(false);
                    backgroundMusicPlayer.SetBackgroundSong(AirshipMusic.File);
                    upDownAnimationAmount = 0.0f;
                    floatOffset = new Vector3(0f, 0f, 0f);
                    animationService.AddListener(this);
                    characterMenuPositionTracker.UseOverrideEntry = true;
                }
            }
        }

        private void Land(ContextMenuArgs args)
        {
            if (active)
            {
                persistence.Current.Player.InAirship = false;
                characterMenuPositionTracker.UseOverrideEntry = false;
                var cell = map.GetCellForLocation(currentPosition);
                var center = map.GetCellCenterpoint(cell);
                center.z += AirshipOffset;
                currentPosition = center;
                currentPosition.y += currentScale.y / 2.0f;
                this.persistence.Current.Player.AirshipPosition = this.currentPosition;
                worldMapManager.MovePlayer(center + new Vector3(0f, 0f, -0.35f));
                upDownAnimationAmount = 0.0f;
                floatOffset = new Vector3(0f, 0f, 0f);
                animationService.RemoveListener(this);
                StopAirshipMode();
                SyncGraphics();
                DestroyPhysics();
                CreatePhysics();
                switch (sprite.CurrentAnimationName)
                {
                    case "up":
                    case "down":
                        sprite.SetAnimation("default");
                        break;
                }
            }
        }

        private void StopAirshipMode()
        {
            if (active)
            {
                active = false;
                contextMenu.ClearContext(Land);
                landEventLayer.makeFocusLayer();
                worldMapManager.SetPlayerVisible(true);
                backgroundMusicPlayer.SetBackgroundSong(WorldMapMusic.File);
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

                if (lStick.y > 0.7f)
                {
                    sprite.SetAnimation("up");
                }
                else if (lStick.y < -0.7f)
                {
                    sprite.SetAnimation("down");
                }
                else if (lStick.x > 0)
                {
                    sprite.SetAnimation("right");
                }
                else if (lStick.x < 0)
                {
                    sprite.SetAnimation("left");
                }

                var stickOffset = new Vector3(lStick.x, 0f, lStick.y);

                currentPosition += Vector3.Forward * lStick.y * clock.DeltaSeconds * moveSpeed;
                currentPosition -= Vector3.Left * lStick.x * clock.DeltaSeconds * moveSpeed;

                var offset = new Vector3(0, 0, 0);
                if (currentPosition.x < worldRect.Left)
                {
                    offset.x = worldRect.Width;
                }
                else if (currentPosition.x > worldRect.Right)
                {
                    offset.x = -worldRect.Width;
                }

                //There is a quadrandt mismatch, rects are in the third and the world is in the first
                if (currentPosition.z < worldRect.Top)
                {
                    offset.z = worldRect.Height;
                }
                if (currentPosition.z > worldRect.Bottom)
                {
                    offset.z = -worldRect.Height;
                }

                currentPosition += offset;
                currentPosition.y = 3.14f;
                cameraMover.OffsetCurrentPosition(offset);

                this.persistence.Current.Player.AirshipPosition = this.currentPosition;

                SyncGraphics();
                cameraMover.SetInterpolatedGoalPosition(currentPosition + cameraOffset + stickOffset * 2.0f, cameraAngle);

                var cell = map.GetCellForLocation(currentPosition);
                if (map.CanLand(cell))
                {
                    contextMenu.HandleContext(languageService.Current.Airship.Land, Land, gamepadId);
                }
                else
                {
                    contextMenu.ClearContext(Land);
                }

                eventLayer.alertEventsHandled();
            }
            else
            {
                //This is a hack since active can go false, not really sure what effect this will have
                active = persistence.Current.Player.InAirship;
            }
        }

        private void Sprite_AnimationChanged(ISprite obj)
        {
            this.currentScale = AirshipSprite.GetScale(obj.CurrentAnimationName) * descriptionScale;
            SyncGraphics();
        }

        public void UpdateAnimation(Clock clock)
        {
            upDownAnimationAmount += 3.5f * clock.DeltaSeconds;
            if (upDownAnimationAmount > TwoPi)
            {
                upDownAnimationAmount -= TwoPi;
            }

            floatOffset.y = MathF.Sin(upDownAnimationAmount) * 0.02f;
            SyncGraphics();
        }
    }
}
