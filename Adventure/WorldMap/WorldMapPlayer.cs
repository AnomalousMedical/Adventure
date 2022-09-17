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

        private readonly RTInstances<IWorldMapGameState> rtInstances;
        private readonly TLASInstanceData tlasData;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IBepuScene<IWorldMapGameState> bepuScene;
        private readonly EventManager eventManager;
        private readonly CameraMover cameraMover;
        private readonly ICollidableTypeIdentifier<IWorldMapGameState> collidableIdentifier;
        private readonly Persistence persistence;
        private readonly IAssetFactory assetFactory;
        private readonly EventLayer eventLayer;
        private readonly IObjectResolver objectResolver;

        private FrameEventSprite sprite;
        private SpriteInstance spriteInstance;
        private bool graphicsActive = false;

        private IPlayerSprite playerSpriteInfo;
        private Attachment<IWorldMapGameState> mainHandItem;
        private Attachment<IWorldMapGameState> offHandItem;
        private Attachment<IWorldMapGameState> mainHandHand;
        private Attachment<IWorldMapGameState> offHandHand;

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
        ButtonEvent sprint;
        ButtonEvent jump;

        private bool disposed;
        private Vector3 cameraOffset = new Vector3(0, 5, -12);
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

        public WorldMapPlayer
        (
            RTInstances<IWorldMapGameState> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            SpriteInstanceFactory spriteInstanceFactory,
            IObjectResolverFactory objectResolverFactory,
            IBepuScene<IWorldMapGameState> bepuScene,
            EventManager eventManager,
            Description description,
            CameraMover cameraMover,
            ICollidableTypeIdentifier<IWorldMapGameState> collidableIdentifier,
            Persistence persistence,
            IAssetFactory assetFactory
        )
        {
            playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));

            this.assetFactory = assetFactory;
            this.characterSheet = description.CharacterSheet;
            this.moveForward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_W });
            this.moveBackward = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_S });
            this.moveRight = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_D });
            this.moveLeft = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_A });
            this.sprint = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_LSHIFT });
            this.jump = new ButtonEvent(description.EventLayer, keys: new KeyboardButtonCode[] { KeyboardButtonCode.KC_SPACE });

            this.primaryHand = description.PrimaryHand;
            this.secondaryHand = description.SecondaryHand;
            this.gamepadId = description.Gamepad;

            sprite = new FrameEventSprite(playerSpriteInfo.Animations);

            //Events
            eventManager.addEvent(moveForward);
            eventManager.addEvent(moveBackward);
            eventManager.addEvent(moveLeft);
            eventManager.addEvent(moveRight);
            eventManager.addEvent(sprint);
            eventManager.addEvent(jump);

            eventLayer = eventManager[description.EventLayer];
            eventLayer.OnUpdate += EventLayer_OnUpdate;

            SetupInput();

            //Sub objects
            objectResolver = objectResolverFactory.Create();

            characterSheet.OnMainHandModified += OnMainHandModified;
            characterSheet.OnOffHandModified += OnOffHandModified;

            OnMainHandModified(characterSheet);
            OnOffHandModified(characterSheet);


            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
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
            var startPos = persistence.Current.Player.WorldPosition ?? description.Translation + new Vector3(0f, halfScale, 0f);

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
                Speed = 2f,
            };

            //Because characters are dynamic, they require a defined BodyInertia. For the purposes of the demos, we don't want them to rotate or fall over, so the inverse inertia tensor is left at its default value of all zeroes.
            //This is effectively equivalent to giving it an infinite inertia tensor- in other words, no torque will cause it to rotate.
            var mass = 1f;
            var bodyDesc =
                BodyDescription.CreateDynamic(startPos.ToSystemNumerics(), new BodyInertia { InverseMass = 1f / mass },
                new CollidableDescription(shapeIndex, moverDesc.SpeculativeMargin),
                new BodyActivityDescription(shape.Radius * 0.02f));

            characterMover = bepuScene.CreateCharacterMover(bodyDesc, moverDesc);
            characterMover.sprint = true;
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            collidableIdentifier.AddIdentifier(new CollidableReference(CollidableMobility.Dynamic, characterMover.BodyHandle), this);
            if (!persistence.Current.Player.InAirship)
            {
                cameraMover.SetPosition(this.currentPosition + cameraOffset, cameraAngle);
            }

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.SpriteMaterialDescription, sprite);

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
            characterSheet.OnMainHandModified -= OnMainHandModified;
            characterSheet.OnOffHandModified -= OnOffHandModified;
            eventManager.removeEvent(moveForward);
            eventManager.removeEvent(moveBackward);
            eventManager.removeEvent(moveLeft);
            eventManager.removeEvent(moveRight);
            eventManager.removeEvent(sprint);
            eventManager.removeEvent(jump);

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
            objectResolver.Dispose();
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
            }
        }

        public void MakeIdle()
        {
            characterMover.movementDirection.X = 0;
            characterMover.movementDirection.Y = 0;
            this.sprite.SetAnimation("down");
        }

        public void CenterCamera()
        {
            if (!persistence.Current.Player.InAirship)
            {
                cameraMover.SetPosition(this.currentPosition + this.cameraOffset, cameraAngle);
            }
        }

        private void SetupInput()
        {
            //These events are owned by this class, so don't have to unsubscribe
            moveForward.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed)
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
                if (l.EventProcessingAllowed)
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
                if (l.EventProcessingAllowed)
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
                if (l.EventProcessingAllowed)
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
            jump.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    characterMover.tryJump = true;
                    l.alertEventsHandled();
                }
            };
            jump.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    characterMover.tryJump = false;
                    l.alertEventsHandled();
                }
            };
            sprint.FirstFrameDownEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    characterMover.sprint = false;
                    l.alertEventsHandled();
                }
            };
            sprint.FirstFrameUpEvent += l =>
            {
                if (l.EventProcessingAllowed)
                {
                    characterMover.sprint = true;
                    l.alertEventsHandled();
                }
            };
        }

        public void SetLocation(in Vector3 location)
        {
            var finalLoc = location + new Vector3(0f, sprite.BaseScale.y * currentScale.y / 2f, 0f);

            bepuScene.RemoveFromInterpolation(characterMover.BodyHandle);
            this.characterMover.SetLocation(finalLoc.ToSystemNumerics());
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            this.currentPosition = location;
            cameraMover.SetPosition(this.currentPosition + cameraOffset, cameraAngle);
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
            this.persistence.Current.Player.WorldPosition = this.currentPosition;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
            Sprite_FrameChanged(sprite);
            cameraMover.SetInterpolatedGoalPosition(this.currentPosition + cameraOffset, cameraAngle);

            var movementDir = characterMover.movementDirection;
            if (movementDir.Y > 0.3f)
            {
                sprite.SetAnimation("up");
            }
            else if (movementDir.Y < -0.3f)
            {
                sprite.SetAnimation("down");
            }
            else if (movementDir.X > 0)
            {
                sprite.SetAnimation("right");
            }
            else if (movementDir.X < 0)
            {
                sprite.SetAnimation("left");
            }
            this.movementDir = movementDir;
        }

        private void EventLayer_OnUpdate(EventLayer eventLayer)
        {
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
                    switch (sprite.CurrentAnimationName)
                    {
                        case "up":
                        case "down":
                        case "left":
                        case "right":
                            sprite.SetAnimation($"stand-{sprite.CurrentAnimationName}");
                            break;
                    }
                }

                eventLayer.alertEventsHandled();
            }
        }

        private void Sprite_AnimationChanged(FrameEventSprite obj)
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
        }

        private void Sprite_FrameChanged(FrameEventSprite obj)
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
                mainHandItem = objectResolver.Resolve<Attachment<IWorldMapGameState>, Attachment<IWorldMapGameState>.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.MainHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                });
            }

            if (characterSheet.MainHand?.ShowHand != false)
            {
                if (mainHandHand == null)
                {
                    mainHandHand = objectResolver.Resolve<Attachment<IWorldMapGameState>, Attachment<IWorldMapGameState>.Description>(o =>
                    {
                        o.Sprite = new Sprite(playerSpriteInfo.Animations)
                        {
                            BaseScale = new Vector3(0.1875f, 0.1875f, 1.0f)
                        };
                        o.SpriteMaterial = playerSpriteInfo.SpriteMaterialDescription;
                    });
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
                offHandItem = objectResolver.Resolve<Attachment<IWorldMapGameState>, Attachment<IWorldMapGameState>.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.OffHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                });
            }

            if (characterSheet.OffHand?.ShowHand != false)
            {
                if (offHandHand == null)
                {
                    offHandHand = objectResolver.Resolve<Attachment<IWorldMapGameState>, Attachment<IWorldMapGameState>.Description>(o =>
                    {
                        o.Sprite = new Sprite(playerSpriteInfo.Animations)
                        {
                            BaseScale = new Vector3(0.1875f, 0.1875f, 1.0f)
                        };
                        o.SpriteMaterial = playerSpriteInfo.SpriteMaterialDescription;
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
    }
}
