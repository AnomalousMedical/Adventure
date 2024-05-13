using Adventure.Assets;
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
        private readonly CameraMover cameraMover;
        private readonly ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier;
        private readonly Persistence persistence;
        private readonly IAssetFactory assetFactory;
        private readonly FollowerManager followerManager;
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

        ButtonEvent moveForward;
        ButtonEvent moveBackward;
        ButtonEvent moveRight;
        ButtonEvent moveLeft;

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
            RTInstances<WorldMapScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            SpriteInstanceFactory spriteInstanceFactory,
            IObjectResolverFactory objectResolverFactory,
            IBepuScene<WorldMapScene> bepuScene,
            EventManager eventManager,
            Description description,
            CameraMover cameraMover,
            ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier,
            Persistence persistence,
            IAssetFactory assetFactory,
            FollowerManager followerManager
        )
        {
            playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));

            this.assetFactory = assetFactory;
            this.followerManager = followerManager;
            this.followerManager.CharacterDistance = this.followerManager.CharacterDistance * description.Scale.x;
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
            var startPos = persistence.Current.Player.WorldPosition ?? description.Translation + new Vector3(0f, halfScale, 0f);

            this.currentPosition = startPos;
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

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
                new BodyActivityDescription(shape.Radius * 0.02f));

            characterMover = bepuScene.CreateCharacterMover(bodyDesc, moverDesc);
            bepuScene.AddToInterpolation(characterMover.BodyHandle);
            collidableIdentifier.AddIdentifier(new CollidableReference(CollidableMobility.Dynamic, characterMover.BodyHandle), this);
            if (!persistence.Current.Player.InAirship)
            {
                cameraMover.SetPosition(this.currentPosition + cameraOffset, cameraAngle);
            }

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
            cameraMover.SetInterpolatedGoalPosition(this.currentPosition + cameraOffset + speedOffset * 0.55f, cameraAngle);
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
                            var animation = $"stand-{sprite.CurrentAnimationName}";
                            sprite.SetAnimation(animation);
                            mainHandItem?.SetAnimation(animation);
                            offHandItem?.SetAnimation(animation);
                            break;
                    }
                }

                eventLayer.alertEventsHandled();
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
                mainHandItem = objectResolver.Resolve<Attachment<WorldMapScene>, Attachment<WorldMapScene>.Description>(o =>
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
                    mainHandHand = objectResolver.Resolve<Attachment<WorldMapScene>, Attachment<WorldMapScene>.Description>(o =>
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
                offHandItem = objectResolver.Resolve<Attachment<WorldMapScene>, Attachment<WorldMapScene>.Description>(o =>
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
            }

            if (characterSheet.OffHand?.ShowHand != false)
            {
                if (offHandHand == null)
                {
                    offHandHand = objectResolver.Resolve<Attachment<WorldMapScene>, Attachment<WorldMapScene>.Description>(o =>
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
    }
}
