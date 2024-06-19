using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using BepuPlugin.Characters;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using Adventure.Assets;
using Adventure.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RpgMath;
using Adventure.Menu;

namespace Adventure
{
    class Player : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            public int PrimaryHand = RightHand;
            public int SecondaryHand = LeftHand;
            public EventLayers EventLayer = EventLayers.Exploration;
            public GamepadId Gamepad = GamepadId.Pad1;
            public String PlayerSprite { get; set; }
            public CharacterSheet CharacterSheet { get; set; }
        }

        public const int RightHand = 0;
        public const int LeftHand = 1;

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly TLASInstanceData tlasData;
        private readonly IDestructionRequest destructionRequest;
        private readonly IScopedCoroutine coroutine;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly EventManager eventManager;
        private readonly CameraMover cameraMover;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly Persistence persistence;
        private readonly IAssetFactory assetFactory;
        private readonly FollowerManager followerManager;
        private readonly CharacterMenuPositionTracker<ZoneScene> characterMenuPositionTracker;
        private readonly IExplorationMenu explorationMenu;
        private readonly EventLayer eventLayer;
        private readonly IObjectResolver objectResolver;
        private List<Follower<ZoneScene>> followers = new List<Follower<ZoneScene>>();

        private EventSprite sprite;
        private SpriteInstance spriteInstance;

        private Attachment<ZoneScene> mainHandItem;
        private Attachment<ZoneScene> offHandItem;

        private IPlayerSprite playerSpriteInfo;
        private Attachment<ZoneScene> mainHandHand;
        private Attachment<ZoneScene> offHandHand;

        private CharacterSheet characterSheet;

        private CharacterMover characterMover;
        private TypedIndex shapeIndex;

        private int primaryHand;
        private int secondaryHand;
        private GamepadId gamepadId;
        private bool allowJoystickInput = true;

        ButtonEvent moveForward;
        ButtonEvent moveBackward;
        ButtonEvent moveRight;
        ButtonEvent moveLeft;

        private bool disposed;
        private Vector3 cameraOffset = new Vector3(0, 5, -12);
        private Vector3 zoomedCameraOffset = new Vector3(0, 2, -2.8f);
        private Quaternion cameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 8f);

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public GamepadId GamepadId => gamepadId;

        private System.Numerics.Vector2 movementDir;
        private const float MovingBoundary = 0.001f;
        public bool IsMoving => !(movementDir.X < MovingBoundary && movementDir.X > -MovingBoundary
                             && movementDir.Y < MovingBoundary && movementDir.Y > -MovingBoundary);

        public record struct PersistedData
        {
            public Vector3? Location { get; set; }
        }

        private CharacterMenuPositionEntry characterMenuPositionEntry;

        public Player
        (
            RTInstances<ZoneScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            SpriteInstanceFactory spriteInstanceFactory,
            IObjectResolverFactory objectResolverFactory,
            IBepuScene<ZoneScene> bepuScene,
            EventManager eventManager,
            Description description,
            CameraMover cameraMover,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
            Persistence persistence,
            IAssetFactory assetFactory,
            FollowerManager followerManager,
            CharacterMenuPositionTracker<ZoneScene> characterMenuPositionTracker,
            IExplorationMenu explorationMenu
        )
        {
            playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));

            this.assetFactory = assetFactory;
            this.followerManager = followerManager;
            this.characterMenuPositionTracker = characterMenuPositionTracker;
            this.explorationMenu = explorationMenu;
            this.characterSheet = description.CharacterSheet;
            this.moveForward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_W });
            this.moveBackward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_S });
            this.moveRight = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_D });
            this.moveLeft = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_A });

            this.primaryHand = description.PrimaryHand;
            this.secondaryHand = description.SecondaryHand;
            this.gamepadId = description.Gamepad;

            sprite = new EventSprite(new Sprite(playerSpriteInfo.Animations));

            //Events
            eventManager.addEvent(moveForward);
            eventManager.addEvent(moveBackward);
            eventManager.addEvent(moveLeft);
            eventManager.addEvent(moveRight);

            eventLayer = eventManager[description.EventLayer];
            eventLayer.OnUpdate += EventLayer_OnUpdate;

            SetupInput();

            //Sub objects
            objectResolver = objectResolverFactory.Create();

            characterSheet.OnMainHandModified += OnMainHandModified;
            characterSheet.OnOffHandModified += OnOffHandModified;
            characterSheet.OnBodyModified += CharacterSheet_OnBodyModified;

            OnMainHandModified(characterSheet);
            OnOffHandModified(characterSheet);


            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.coroutine = coroutine;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.bepuScene = bepuScene;
            this.bepuScene.OnUpdated += BepuScene_OnUpdated;
            this.eventManager = eventManager;
            this.cameraMover = cameraMover;
            this.collidableIdentifier = collidableIdentifier;
            this.persistence = persistence;
            this.assetFactory = assetFactory;
            var scale = description.Scale * sprite.BaseScale;
            var halfScale = scale.y / 2f;
            var startPos = persistence.Current.Player.Position ?? description.Translation + new Vector3(0f, halfScale, 0f);

            this.currentPosition = startPos;
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Player"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale)
            };

            //Character Mover
            var shape = new Sphere(halfScale); //Each character creates a shape, try to load from resources somehow
            shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

            var moverDesc = new CharacterMoverDescription()
            {
                MinimumSupportDepth = shape.Radius * -0.01f,
                MaximumHorizontalForce = 100,
                Speed = 7f
            };

            //Because characters are dynamic, they require a defined BodyInertia. For the purposes of the demos, we don't want them to rotate or fall over, so the inverse inertia tensor is left at its default value of all zeroes.
            //This is effectively equivalent to giving it an infinite inertia tensor- in other words, no torque will cause it to rotate.
            var mass = 1f;
            var bodyDesc =
                BodyDescription.CreateDynamic(startPos.ToSystemNumerics(), new BodyInertia { InverseMass = 1f / mass },
                new CollidableDescription(shapeIndex, moverDesc.SpeculativeMargin),
                new BodyActivityDescription(shape.Radius * 0.02f));

            characterMover = bepuScene.CreateCharacterMover(bodyDesc, moverDesc);
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            collidableIdentifier.AddIdentifier(new CollidableReference(CollidableMobility.Dynamic, characterMover.BodyHandle), this);

            characterMenuPositionEntry = new CharacterMenuPositionEntry(() => this.currentPosition + zoomedCameraOffset, () => this.cameraAngle, () =>
            {
                sprite.SetAnimation("stand-down");
                Sprite_FrameChanged(sprite);
            },
            GetMagicHitLocation: () => this.currentPosition + new Vector3(0f, 0f, -0.05f),
            GetScale: () => this.currentScale);
            characterMenuPositionTracker.Set(characterSheet, characterMenuPositionEntry);

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.GetTier(characterSheet.EquipmentTier), sprite);

                if (this.disposed)
                {
                    this.spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }

                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);

                sprite.AnimationChanged += Sprite_AnimationChanged;
                sprite.FrameChanged += Sprite_FrameChanged;
                Sprite_AnimationChanged(sprite);
                Sprite_FrameChanged(sprite);
            });
        }

        public void Dispose()
        {
            disposed = true;
            characterSheet.OnBodyModified -= CharacterSheet_OnBodyModified;
            characterSheet.OnMainHandModified -= OnMainHandModified;
            characterSheet.OnOffHandModified -= OnOffHandModified;
            eventManager.removeEvent(moveForward);
            eventManager.removeEvent(moveBackward);
            eventManager.removeEvent(moveLeft);
            eventManager.removeEvent(moveRight);

            eventLayer.OnUpdate -= EventLayer_OnUpdate; //Do have to remove this since its on the layer itself

            this.bepuScene.OnUpdated -= BepuScene_OnUpdated;
            collidableIdentifier.RemoveIdentifier(new CollidableReference(CollidableMobility.Dynamic, characterMover.BodyHandle));
            bepuScene.RemoveFromInterpolation(characterMover.BodyHandle);
            bepuScene.DestroyCharacterMover(characterMover);
            bepuScene.Simulation.Shapes.Remove(shapeIndex);
            sprite.FrameChanged -= Sprite_FrameChanged;
            sprite.AnimationChanged -= Sprite_AnimationChanged;
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            objectResolver.Dispose();
            characterMenuPositionTracker.Remove(characterSheet, characterMenuPositionEntry);
        }

        public void StopMovement()
        {
            characterMover.movementDirection.X = 0;
            characterMover.movementDirection.Y = 0;
            this.movementDir = characterMover.movementDirection;
            bepuScene.RemoveFromInterpolation(characterMover.BodyHandle);
            this.characterMover.SetLocation(this.currentPosition.ToSystemNumerics());
            this.characterMover.SetVelocity(new System.Numerics.Vector3(0f, 0f, 0f));
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            ChangeToStoppedAnimation();
            followerManager.LeaderMoved(this.currentPosition, IsMoving);
        }

        private void SetupInput()
        {
            //These events are owned by this class, so don't have to unsubscribe
            moveForward.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.Y = 1;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                    this.sprite.SetAnimation("up");
                }
            };
            moveForward.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.Y > 0.5f) { characterMover.movementDirection.Y = 0; }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveBackward.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.Y = -1;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                    this.sprite.SetAnimation("down");
                }
            };
            moveBackward.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.Y < -0.5f) { characterMover.movementDirection.Y = 0; }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveLeft.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.X = -1;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                    this.sprite.SetAnimation("left");
                }
            };
            moveLeft.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.X < 0.5f) { characterMover.movementDirection.X = 0; }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveRight.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.X = 1;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                    this.sprite.SetAnimation("right");
                }
            };
            moveRight.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.X > 0.5f) { characterMover.movementDirection.X = 0; }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
        }

        public void SetLocation(in Vector3 location, Zone.Alignment alignment)
        {
            var finalLoc = location + new Vector3(0f, sprite.BaseScale.y / 2f, 0f);

            bepuScene.RemoveFromInterpolation(characterMover.BodyHandle);
            this.characterMover.SetLocation(finalLoc.ToSystemNumerics());
            this.characterMover.SetVelocity(new System.Numerics.Vector3(0f, 0f, 0f));
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            this.currentPosition = finalLoc;
            this.persistence.Current.Player.Position = this.currentPosition;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
            this.followerManager.LineUpBehindLeader(this.currentPosition, alignment);
            string animation;
            switch (alignment)
            {
                case Zone.Alignment.NorthSouth:
                    animation = "stand-down";
                    break;
                case Zone.Alignment.SouthNorth:
                    animation = "stand-up";
                    break;
                case Zone.Alignment.WestEast:
                    animation = "stand-right";
                    break;
                default:
                case Zone.Alignment.EastWest:
                    animation = "stand-left";
                    break;
            }
            this.sprite.SetAnimation(animation);
            Sprite_FrameChanged(sprite);
        }

        public void CenterCamera()
        {
            cameraMover.SetPosition(currentPosition + cameraOffset, cameraAngle);
        }

        public void OffsetCamera(in Vector3 offset)
        {
            cameraMover.OffsetCurrentPosition(offset);
            cameraMover.OffsetPosition(offset);
        }

        /// <summary>
        /// Restoring the location from persistence is a special case.
        /// If there is nothing in the persistence the safetyPosition is used either from
        /// the front or the end position. If the player has never started the game before
        /// they will be forced to the start position.
        /// </summary>
        public void RestorePersistedLocation(in Vector3 startSafetyPosition, in Vector3 endSafetyPosition, bool startEnd, Zone.Alignment alignment)
        {
            if (startEnd)
            {
                alignment = Zone.GetEndAlignment(alignment);
            }
            var location = persistence.Current.Player.Position;
            if (location == null)
            {
                //Game has started and first zone is complete
                if (persistence.Current.Player.Started && persistence.Current.World.CompletedAreaLevels.ContainsKey(0))
                {
                    SetLocation(startEnd ? endSafetyPosition : startSafetyPosition, alignment);
                }
                else
                {
                    SetLocation(startSafetyPosition, alignment);
                    persistence.Current.Player.Started = true;
                }
            }
            else
            {
                bepuScene.RemoveFromInterpolation(characterMover.BodyHandle);
                this.characterMover.SetLocation(location.Value.ToSystemNumerics());
                bepuScene.AddToInterpolation(characterMover.BodyHandle);
                this.currentPosition = location.Value;
                this.persistence.Current.Player.Position = this.currentPosition;
                this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
                this.followerManager.LeaderMoved(this.currentPosition, IsMoving);
                Sprite_FrameChanged(sprite);
            }
        }

        public Vector3 GetLocation()
        {
            return this.currentPosition - new Vector3(0f, sprite.BaseScale.y / 2f, 0f);
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        private void BepuScene_OnUpdated(IBepuScene obj)
        {
            bepuScene.GetInterpolatedPosition(characterMover.BodyHandle, ref this.currentPosition, ref this.currentOrientation);
            this.persistence.Current.Player.Position = this.currentPosition;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
            this.followerManager.LeaderMoved(this.currentPosition, IsMoving);
            Sprite_FrameChanged(sprite);

            var movementDir = characterMover.movementDirection;
            if (movementDir.Y > 0.3f)
            {
                sprite.SetAnimation("up");
                mainHandItem?.SetAnimation("up");
                offHandItem?.SetAnimation("up");
            }
            else if (movementDir.Y < -0.3f)
            {
                sprite.SetAnimation("down");
                mainHandItem?.SetAnimation("down");
                offHandItem?.SetAnimation("down");
            }
            else if (movementDir.X > 0)
            {
                sprite.SetAnimation("right");
                mainHandItem?.SetAnimation("right");
                offHandItem?.SetAnimation("right");
            }
            else if (movementDir.X < 0)
            {
                sprite.SetAnimation("left");
                mainHandItem?.SetAnimation("left");
                offHandItem?.SetAnimation("left");
            }
            this.movementDir = movementDir;

            var speedOffset = characterMover.LinearVelocity / characterMover.speed;
            speedOffset.y = 0;
            cameraMover.SetInterpolatedGoalPosition(this.currentPosition + cameraOffset + speedOffset * 1.15f, cameraAngle);
        }

        bool forceStopOnMenuHandled = false;
        private void EventLayer_OnUpdate(EventLayer eventLayer)
        {
            if (explorationMenu.Handled)
            {
                var lastForceStop = forceStopOnMenuHandled;
                forceStopOnMenuHandled = true;
                if(forceStopOnMenuHandled != lastForceStop)
                {
                    StopMovement();
                }
                return;
            }

            forceStopOnMenuHandled = false;

            if (eventLayer.EventProcessingAllowed)
            {
                if (allowJoystickInput)
                {
                    var pad = eventLayer.getGamepad(gamepadId);
                    var movementDir = pad.LStick;
                    characterMover.movementDirection = movementDir.ToSystemNumerics();
                }

                if (characterMover.movementDirection.X == 0 && characterMover.movementDirection.Y == 0)
                {
                    ChangeToStoppedAnimation();
                }

                eventLayer.alertEventsHandled();
            }
        }

        private void ChangeToStoppedAnimation()
        {
            switch (sprite.CurrentAnimationName)
            {
                case "up":
                case "down":
                case "left":
                case "right":
                    var animation = $"stand-{sprite.CurrentAnimationName}";
                    sprite.SetAnimation(animation);
                    mainHandItem?.SetAnimation(animation);
                    offHandItem?.SetAnimation(animation);
                    break;
            }
        }

        private void Sprite_AnimationChanged(ISprite obj)
        {
            if (mainHandHand != null)
            {
                switch (primaryHand)
                {
                    case RightHand:
                        mainHandHand.SetAnimation(obj.CurrentAnimationName + "-r-hand");
                        break;
                    case LeftHand:
                        mainHandHand.SetAnimation(obj.CurrentAnimationName + "-l-hand");
                        break;
                }
            }

            if (offHandHand != null)
            {
                switch (secondaryHand)
                {
                    case RightHand:
                        offHandHand.SetAnimation(obj.CurrentAnimationName + "-r-hand");
                        break;
                    case LeftHand:
                        offHandHand.SetAnimation(obj.CurrentAnimationName + "-l-hand");
                        break;
                }
            }
        }

        private void Sprite_FrameChanged(ISprite obj)
        {
            var frame = obj.GetCurrentFrame();

            Vector3 offset;
            var scale = sprite.BaseScale * this.currentScale;

            var primaryAttach = frame.Attachments[this.primaryHand];
            offset = scale * primaryAttach.translate;
            offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
            mainHandItem?.SetPosition(offset, this.currentOrientation, scale);
            mainHandHand?.SetPosition(offset, this.currentOrientation, scale);

            var secondaryAttach = frame.Attachments[this.secondaryHand];
            offset = scale * secondaryAttach.translate;
            offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
            offHandItem?.SetPosition(offset, this.currentOrientation, scale);
            offHandHand?.SetPosition(offset, this.currentOrientation, scale);
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }

        private void OnMainHandModified(CharacterSheet obj)
        {
            mainHandItem?.RequestDestruction();
            mainHandItem = null;
            if (characterSheet.MainHand?.Sprite != null)
            {
                mainHandItem = objectResolver.Resolve<Attachment<ZoneScene>, IAttachment.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.MainHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                    o.Light = asset.CreateLight();
                    o.LightAttachmentChannel = asset.LightAttachmentChannel;
                });
            }

            if (characterSheet.MainHand?.ShowHand != false)
            {
                if (mainHandHand == null)
                {
                    mainHandHand = objectResolver.Resolve<Attachment<ZoneScene>, IAttachment.Description>(o =>
                    {
                        o.Sprite = new Sprite(playerSpriteInfo.Animations)
                        {
                            BaseScale = new Vector3(0.1875f, 0.1875f, 1.0f)
                        };
                        o.SpriteMaterial = playerSpriteInfo.Tier1;
                    });
                }
            }
            else if (mainHandHand != null)
            {
                mainHandHand.RequestDestruction();
                mainHandHand = null;
            }
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
        }

        private void OnOffHandModified(CharacterSheet obj)
        {
            offHandItem?.RequestDestruction();
            offHandItem = null;
            if (characterSheet.OffHand?.Sprite != null)
            {
                offHandItem = objectResolver.Resolve<Attachment<ZoneScene>, IAttachment.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.OffHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                    o.Light = asset.CreateLight();
                    o.LightAttachmentChannel = asset.LightAttachmentChannel;
                });
            }

            if (characterSheet.OffHand?.ShowHand != false)
            {
                if (offHandHand == null)
                {
                    offHandHand = objectResolver.Resolve<Attachment<ZoneScene>, IAttachment.Description>(o =>
                    {
                        o.Sprite = new Sprite(playerSpriteInfo.Animations)
                        {
                            BaseScale = new Vector3(0.1875f, 0.1875f, 1.0f)
                        };
                        o.SpriteMaterial = playerSpriteInfo.Tier1;
                    });
                }
            }
            else if (offHandHand != null)
            {
                offHandHand.RequestDestruction();
                offHandHand = null;
            }
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
        }

        private void CharacterSheet_OnBodyModified(CharacterSheet obj)
        {
             coroutine.RunTask(SwapSprites());
        }

        private async Task SwapSprites()
        {
            using var destructionBlock = destructionRequest.BlockDestruction();

            var loadingTier = characterSheet.EquipmentTier;
            var newSprite = await spriteInstanceFactory.Checkout(playerSpriteInfo.GetTier(loadingTier), sprite);

            if (this.disposed || loadingTier != characterSheet.EquipmentTier)
            {
                this.spriteInstanceFactory.TryReturn(newSprite);
                return; //Stop loading
            }

            rtInstances.RemoveSprite(sprite);
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            this.spriteInstance = newSprite;
            rtInstances.AddSprite(sprite, tlasData, spriteInstance);
        }

        public void CreateFollowers(IEnumerable<Persistence.CharacterData> newFollowers)
        {
            foreach (var follower in this.followers)
            {
                follower.RequestDestruction();
            }
            followers.Clear();

            foreach (var follower in newFollowers)
            {
                var followerInstance = this.objectResolver.Resolve<Follower<ZoneScene>, FollowerDescription>(c =>
                {
                    c.Translation = this.currentPosition;
                    c.PlayerSprite = follower.PlayerSprite;
                    c.CharacterSheet = follower.CharacterSheet;
                    c.FollowerManager = followerManager;
                    c.ZoomedCameraOffset = this.zoomedCameraOffset;
                    c.CameraAngle = this.cameraAngle;
                });
                this.followers.Add(followerInstance);
            }
        }
    }
}
