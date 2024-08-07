﻿using Adventure.Assets;
using Adventure.Menu;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using BepuPlugin.Characters;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class WorldMapPlayer : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            public Description()
            {
                Scale = new Vector3(0.35f, 0.35f, 1.0f);
            }

            public int PrimaryHand = RightHand;
            public int SecondaryHand = LeftHand;
            public EventLayers EventLayer = EventLayers.WorldMap;
            public GamepadId Gamepad = GamepadId.Pad1;
            public String PlayerSprite { get; set; }
            public CharacterSheet CharacterSheet { get; set; }
        }

        public const int RightHand = 0;
        public const int LeftHand = 1;

        private readonly RTInstances<WorldMapScene> rtInstances;
        private readonly TLASInstanceData tlasData;
        private readonly IDestructionRequest destructionRequest;
        private readonly IScopedCoroutine coroutine;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IBepuScene<WorldMapScene> bepuScene;
        private readonly EventManager eventManager;
        private readonly ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier;
        private readonly Persistence persistence;
        private readonly IAssetFactory assetFactory;
        private readonly FollowerManager followerManager;
        private readonly ICharacterMenuPositionTracker<WorldMapScene> characterMenuPositionTracker;
        private readonly IExplorationMenu explorationMenu;
        private readonly MultiCameraMover<WorldMapScene> multiCameraMover;
        private readonly PlayerCage<WorldMapScene> playerCage;
        private readonly KeybindService keybindService;
        private readonly EventLayer eventLayer;
        private readonly IObjectResolver objectResolver;
        private List<Follower<WorldMapScene>> followers = new List<Follower<WorldMapScene>>();

        private EventSprite sprite;
        private SpriteInstance spriteInstance;
        private bool graphicsActive = false;

        private IPlayerSprite playerSpriteInfo;
        private Attachment<WorldMapScene> mainHandItem;
        private Attachment<WorldMapScene> offHandItem;
        private Attachment<WorldMapScene> mainHandHand;
        private Attachment<WorldMapScene> offHandHand;

        private CharacterSheet characterSheet;

        private CharacterMover characterMover;
        private TypedIndex shapeIndex;

        private int primaryHand;
        private int secondaryHand;
        private GamepadId gamepadId;
        private bool allowJoystickInput = true;
        private MultiCameraMoverEntry multiCameraMoverEntry;
        private PlayerCageEntry playerCageEntry;

        ButtonEvent moveForward;
        ButtonEvent moveBackward;
        ButtonEvent moveRight;
        ButtonEvent moveLeft;

        private bool disposed;
        private Vector3 zoomedCameraOffset = new Vector3(0f, 1.7f * 0.35f, -2.8f * 0.35f);
        private Quaternion zoomedCameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 10f);

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public GamepadId GamepadId => gamepadId;

        private System.Numerics.Vector2 movementDir;
        private const float MovingBoundary = 0.001f;
        public bool IsMoving => !(movementDir.X < MovingBoundary && movementDir.X > -MovingBoundary
                             && movementDir.Y < MovingBoundary && movementDir.Y > -MovingBoundary);

        public CharacterSheet CharacterSheet => characterSheet;

        public record struct PersistedData
        {
            public Vector3? Location { get; set; }
        }

        private CharacterMenuPositionEntry characterMenuPositionEntry;

        public WorldMapPlayer
        (
            RTInstances<WorldMapScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            SpriteInstanceFactory spriteInstanceFactory,
            IObjectResolverFactory objectResolverFactory,
            IBepuScene<WorldMapScene> bepuScene,
            EventManager eventManager,
            Description description,
            ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier,
            Persistence persistence,
            IAssetFactory assetFactory,
            FollowerManager followerManager,
            ICharacterMenuPositionTracker<WorldMapScene> characterMenuPositionTracker,
            IExplorationMenu explorationMenu,
            MultiCameraMover<WorldMapScene> multiCameraMover,
            PlayerCage<WorldMapScene> playerCage, 
            KeybindService keybindService
        )
        {
            playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));

            this.assetFactory = assetFactory;
            this.followerManager = followerManager;
            this.characterMenuPositionTracker = characterMenuPositionTracker;
            this.explorationMenu = explorationMenu;
            this.multiCameraMover = multiCameraMover;
            this.playerCage = playerCage;
            this.keybindService = keybindService;
            this.followerManager.CharacterDistance = this.followerManager.CharacterDistance * description.Scale.x;
            this.characterSheet = description.CharacterSheet;
            this.moveForward = new ButtonEvent(description.EventLayer, keys: keybindService.GetKeyboardBinding(KeyBindings.MoveUp));
            this.moveBackward = new ButtonEvent(description.EventLayer, keys: keybindService.GetKeyboardBinding(KeyBindings.MoveDown));
            this.moveRight = new ButtonEvent(description.EventLayer, keys: keybindService.GetKeyboardBinding(KeyBindings.MoveRight));
            this.moveLeft = new ButtonEvent(description.EventLayer, keys: keybindService.GetKeyboardBinding(KeyBindings.MoveLeft));

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

            keybindService.KeybindChanged += KeybindService_KeybindChanged;

            //Sub objects
            objectResolver = objectResolverFactory.Create();

            characterSheet.OnMainHandModified += OnMainHandModified;
            characterSheet.OnOffHandModified += OnOffHandModified;
            characterSheet.OnBodyModified += CharacterSheet_OnBodyModified;

            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.coroutine = coroutine;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.bepuScene = bepuScene;
            this.bepuScene.OnUpdated += BepuScene_OnUpdated;
            this.eventManager = eventManager;
            this.collidableIdentifier = collidableIdentifier;
            this.persistence = persistence;
            this.assetFactory = assetFactory;
            var scale = description.Scale * sprite.BaseScale;
            var halfScale = scale.y / 2f;
            var startPos = persistence.Current.Player.WorldPosition[(int)gamepadId] ?? description.Translation + new Vector3(0f, halfScale, 0f);

            this.currentPosition = startPos;
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

            multiCameraMoverEntry = new MultiCameraMoverEntry()
            {
                Position = currentPosition,
                SpeedOffset = Vector3.Zero,
            };
            multiCameraMover.Add(multiCameraMoverEntry);

            playerCageEntry = new PlayerCageEntry()
            {
                Position = currentPosition
            };
            playerCage.Add(playerCageEntry);

            OnMainHandModified(characterSheet);
            OnOffHandModified(characterSheet);

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
                Speed = 3.5f,
            };

            //Because characters are dynamic, they require a defined BodyInertia. For the purposes of the demos, we don't want them to rotate or fall over, so the inverse inertia tensor is left at its default value of all zeroes.
            //This is effectively equivalent to giving it an infinite inertia tensor- in other words, no torque will cause it to rotate.
            var mass = 1f;
            var bodyDesc =
                BodyDescription.CreateDynamic(startPos.ToSystemNumerics(), new BodyInertia { InverseMass = 1f / mass },
                new CollidableDescription(shapeIndex, moverDesc.SpeculativeMargin),
                new BodyActivityDescription(-1000.0f));

            characterMover = bepuScene.CreateCharacterMover(bodyDesc, moverDesc);
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            collidableIdentifier.AddIdentifier(new CollidableReference(CollidableMobility.Dynamic, characterMover.BodyHandle), this);
            ref var collisionFilter = ref bepuScene.CollisionFilters.Allocate(characterMover.BodyHandle); //Still not sure this doesn't leak, but no demos show a deallocate call
            collisionFilter = new SubgroupCollisionFilter(10, 0);
            collisionFilter.DisableCollision(0);

            characterMenuPositionEntry = new CharacterMenuPositionEntry(() => this.currentPosition + zoomedCameraOffset, () => this.zoomedCameraAngle, () =>
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

                SetGraphicsActive(!persistence.Current.Player.InAirship);

                sprite.FrameChanged += Sprite_FrameChanged;
                sprite.AnimationChanged += Sprite_AnimationChanged;
                Sprite_FrameChanged(sprite);
                Sprite_AnimationChanged(sprite);
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
            SetGraphicsActive(false);
            characterMenuPositionTracker.Remove(characterSheet, characterMenuPositionEntry);
            objectResolver.Dispose();
            multiCameraMover.Remove(multiCameraMoverEntry);
            playerCage.Remove(playerCageEntry);
            keybindService.KeybindChanged -= KeybindService_KeybindChanged;
        }

        public void SetGraphicsActive(bool active)
        {
            if (graphicsActive != active)
            {
                if (active)
                {
                    rtInstances.AddTlasBuild(tlasData);
                    rtInstances.AddShaderTableBinder(Bind);
                    rtInstances.AddSprite(sprite, tlasData, spriteInstance);
                    graphicsActive = true;
                }
                else
                {
                    rtInstances.RemoveSprite(sprite);
                    rtInstances.RemoveShaderTableBinder(Bind);
                    rtInstances.RemoveTlasBuild(tlasData);
                    graphicsActive = false;
                }

                mainHandHand?.SetGraphicsActive(active);
                mainHandItem?.SetGraphicsActive(active);

                offHandHand?.SetGraphicsActive(active);
                offHandItem?.SetGraphicsActive(active);

                foreach(var follower in followers)
                {
                    follower.SetGraphicsActive(active);
                }
            }
        }

        public void MakeIdle()
        {
            characterMover.movementDirection.X = 0;
            characterMover.movementDirection.Y = 0;
            this.sprite.SetAnimation("stand-down");
            this.followerManager.LineUpBehindLeader(this.currentPosition);
            this.characterMover.SetVelocity(new System.Numerics.Vector3(0f, 0f, 0f));
            Sprite_FrameChanged(sprite);
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
            const float FullMovementAmount = 1.0f;
            const float SharedMovementAmount = 0.7f;

            //These events are owned by this class, so don't have to unsubscribe
            moveForward.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.Y = MathF.Abs(characterMover.movementDirection.X) > 0.01f ? SharedMovementAmount : FullMovementAmount;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveForward.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.Y > 0.5f)
                    {
                        characterMover.movementDirection.Y = 0;
                        if(characterMover.movementDirection.X > 0.01f)
                        {
                            characterMover.movementDirection.X = FullMovementAmount;
                        }
                        else if(characterMover.movementDirection.X < -0.01f)
                        {
                            characterMover.movementDirection.X = -FullMovementAmount;
                        }
                    }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveBackward.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.Y = MathF.Abs(characterMover.movementDirection.X) > 0.01f ? -SharedMovementAmount : -FullMovementAmount;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveBackward.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.Y < -0.5f)
                    {
                        characterMover.movementDirection.Y = 0;
                        if (characterMover.movementDirection.X > 0.01f)
                        {
                            characterMover.movementDirection.X = FullMovementAmount;
                        }
                        else if (characterMover.movementDirection.X < -0.01f)
                        {
                            characterMover.movementDirection.X = -FullMovementAmount;
                        }
                    }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveLeft.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.X = MathF.Abs(characterMover.movementDirection.Y) > 0.01f ? -SharedMovementAmount : -FullMovementAmount;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveLeft.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.X < 0.5f)
                    {
                        characterMover.movementDirection.X = 0;
                        if (characterMover.movementDirection.Y > 0.01f)
                        {
                            characterMover.movementDirection.Y = FullMovementAmount;
                        }
                        else if (characterMover.movementDirection.Y < -0.01f)
                        {
                            characterMover.movementDirection.Y = -FullMovementAmount;
                        }
                    }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
            moveRight.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed && !explorationMenu.Handled)
                {
                    characterMover.movementDirection.X = MathF.Abs(characterMover.movementDirection.Y) > 0.01f ? SharedMovementAmount : FullMovementAmount;
                    l.alertEventsHandled();
                    allowJoystickInput = false;
                }
            };
            moveRight.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    if (characterMover.movementDirection.X > 0.5f)
                    {
                        characterMover.movementDirection.X = 0;
                        if (characterMover.movementDirection.Y > 0.01f)
                        {
                            characterMover.movementDirection.Y = FullMovementAmount;
                        }
                        else if (characterMover.movementDirection.Y < -0.01f)
                        {
                            characterMover.movementDirection.Y = -FullMovementAmount;
                        }
                    }
                    l.alertEventsHandled();
                    allowJoystickInput = moveForward.Up && moveBackward.Up && moveLeft.Up && moveRight.Up;
                }
            };
        }

        public void SetLocation(in Vector3 location)
        {
            var finalLoc = location + new Vector3(0f, currentScale.y / 2f, 0f);

            bepuScene.RemoveFromInterpolation(characterMover.BodyHandle);
            this.characterMover.SetLocation(finalLoc.ToSystemNumerics());
            this.characterMover.SetVelocity(new System.Numerics.Vector3(0f, 0f, 0f));
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            this.currentPosition = finalLoc;
            multiCameraMoverEntry.Position = this.currentPosition;
            playerCageEntry.Position = this.currentPosition;
            this.followerManager.LeaderMoved(this.currentPosition, IsMoving);
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
            Sprite_FrameChanged(sprite);
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        private bool biasUpDown = true;

        private void BepuScene_OnUpdated(IBepuScene obj)
        {
            bepuScene.GetInterpolatedPosition(characterMover.BodyHandle, ref this.currentPosition, ref this.currentOrientation);
            this.persistence.Current.Player.WorldPosition[(int)gamepadId] = this.currentPosition;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
            this.followerManager.LeaderMoved(this.currentPosition, IsMoving);
            Sprite_FrameChanged(sprite);

            var movementDir = characterMover.movementDirection;
            const float ChangeDirXThreshold = 0.80f;
            const float ChangeDirYThreshold = 0.80f;
            var movingUp = movementDir.Y > 0;
            var movingDown = movementDir.Y < 0;
            if (biasUpDown && movingUp && (movementDir.X < ChangeDirYThreshold && movementDir.X > -ChangeDirYThreshold))
            {
                SetCurrentAnimation("up");
                biasUpDown = true;
            }
            else if (biasUpDown && movingDown && (movementDir.X < ChangeDirYThreshold && movementDir.X > -ChangeDirYThreshold))
            {
                SetCurrentAnimation("down");
                biasUpDown = true;
            }
            else if (movementDir.X > 0 && !(!biasUpDown && (movementDir.Y > ChangeDirXThreshold || movementDir.Y < -ChangeDirXThreshold)))
            {
                SetCurrentAnimation("right");
                biasUpDown = false;
            }
            else if (movementDir.X < 0 && !(!biasUpDown && (movementDir.Y > ChangeDirXThreshold || movementDir.Y < -ChangeDirXThreshold)))
            {
                SetCurrentAnimation("left");
                biasUpDown = false;
            }
            else if (movingUp)
            {
                SetCurrentAnimation("up");
                biasUpDown = true;
            }
            else if (movingDown)
            {
                SetCurrentAnimation("down");
                biasUpDown = true;
            }
            this.movementDir = movementDir;


            multiCameraMoverEntry.Position = this.currentPosition;
            multiCameraMoverEntry.SpeedOffset = (characterMover.LinearVelocity / characterMover.speed) * 0.55f;
            playerCageEntry.Position = this.currentPosition;
        }

        private void SetCurrentAnimation(string name)
        {
            sprite.SetAnimation(name);
            mainHandItem?.SetAnimation(name);
            offHandItem?.SetAnimation(name);
        }

        bool forceStopOnMenuHandled;
        private void EventLayer_OnUpdate(EventLayer eventLayer)
        {
            if (explorationMenu.Handled)
            {
                var lastForceStop = forceStopOnMenuHandled;
                forceStopOnMenuHandled = true;
                if (forceStopOnMenuHandled != lastForceStop)
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
                    case Player.RightHand:
                        mainHandHand.SetAnimation(obj.CurrentAnimationName + "-r-hand");
                        break;
                    case Player.LeftHand:
                        mainHandHand.SetAnimation(obj.CurrentAnimationName + "-l-hand");
                        break;
                }
            }

            if (offHandHand != null)
            {
                switch (secondaryHand)
                {
                    case Player.RightHand:
                        offHandHand.SetAnimation(obj.CurrentAnimationName + "-r-hand");
                        break;
                    case Player.LeftHand:
                        offHandHand.SetAnimation(obj.CurrentAnimationName + "-l-hand");
                        break;
                }
            }

            mainHandItem?.SetAnimation(obj.CurrentAnimationName);
            offHandItem?.SetAnimation(obj.CurrentAnimationName);
        }

        private void Sprite_FrameChanged(ISprite obj)
        {
            var frame = obj.GetCurrentFrame();

            Vector3 offset;
            var scale = this.currentScale;

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
                mainHandItem = objectResolver.Resolve<Attachment<WorldMapScene>, IAttachment.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.MainHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                    o.Light = asset.CreateLight();
                    if (o.Light != null)
                    {
                        o.Light.Length *= this.currentScale.x;
                    }
                    o.LightAttachmentChannel = asset.LightAttachmentChannel;
                });
            }

            if (characterSheet.MainHand?.ShowHand != false)
            {
                if (mainHandHand == null)
                {
                    mainHandHand = objectResolver.Resolve<Attachment<WorldMapScene>, IAttachment.Description>(o =>
                    {
                        o.Sprite = new Sprite(playerSpriteInfo.Animations)
                        {
                            BaseScale = new Vector3(0.1875f, 0.1875f, 1.0f)
                        };
                        o.SpriteMaterial = playerSpriteInfo.Tier1;
                    });

                    mainHandItem.SetAnimation(sprite.CurrentAnimationName);
                }
            }
            else if (mainHandHand != null)
            {
                mainHandHand.RequestDestruction();
                mainHandHand = null;
            }
            mainHandHand?.SetGraphicsActive(graphicsActive);
            mainHandItem?.SetGraphicsActive(graphicsActive);
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
        }

        private void OnOffHandModified(CharacterSheet obj)
        {
            offHandItem?.RequestDestruction();
            offHandItem = null;
            if (characterSheet.OffHand?.Sprite != null)
            {
                offHandItem = objectResolver.Resolve<Attachment<WorldMapScene>, IAttachment.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.OffHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                    o.Light = asset.CreateLight();
                    if (o.Light != null)
                    {
                        o.Light.Length *= this.currentScale.x;
                    }
                    o.LightAttachmentChannel = asset.LightAttachmentChannel;
                });

                offHandItem.SetAnimation(sprite.CurrentAnimationName);
            }

            if (characterSheet.OffHand?.ShowHand != false)
            {
                if (offHandHand == null)
                {
                    offHandHand = objectResolver.Resolve<Attachment<WorldMapScene>, IAttachment.Description>(o =>
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
            offHandHand?.SetGraphicsActive(graphicsActive);
            offHandItem?.SetGraphicsActive(graphicsActive);
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
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
                var followerInstance = this.objectResolver.Resolve<Follower<WorldMapScene>, FollowerDescription>(c =>
                {
                    c.Translation = this.currentPosition;
                    c.Scale = this.currentScale;
                    c.PlayerSprite = follower.PlayerSprite;
                    c.CharacterSheet = follower.CharacterSheet;
                    c.FollowerManager = followerManager;
                    c.StartVisible = this.graphicsActive;
                    c.ZoomedCameraOffset = this.zoomedCameraOffset;
                    c.CameraAngle = this.zoomedCameraAngle;
                    c.PlayerGamepadId = gamepadId;
                });
                this.followers.Add(followerInstance);
            }
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

        private void KeybindService_KeybindChanged(KeybindService service, KeyBindings binding)
        {
            switch (binding)
            {
                case KeyBindings.MoveUp:
                    moveForward.clearButtons();
                    moveForward.addButtons(service.GetKeyboardBinding(binding));
                    break;
                case KeyBindings.MoveDown:
                    moveBackward.clearButtons();
                    moveBackward.addButtons(service.GetKeyboardBinding(binding));
                    break;
                case KeyBindings.MoveLeft:
                    moveLeft.clearButtons();
                    moveLeft.addButtons(service.GetKeyboardBinding(binding));
                    break;
                case KeyBindings.MoveRight:
                    moveRight.clearButtons();
                    moveRight.addButtons(service.GetKeyboardBinding(binding));
                    break;
            }
        }
    }
}
