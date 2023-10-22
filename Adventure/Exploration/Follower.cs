using Adventure.Assets;
using Adventure.Services;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using RpgMath;
using System;

namespace Adventure
{
    class Follower : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            public int PrimaryHand = RightHand;
            public int SecondaryHand = LeftHand;
            public String PlayerSprite { get; set; }
            public CharacterSheet CharacterSheet { get; set; }

            public FollowerManager FollowerManager { get; set; }
        }

        public const int RightHand = 0;
        public const int LeftHand = 1;

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly TLASInstanceData tlasData;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IAssetFactory assetFactory;
        private readonly FollowerManager followerManager;
        private readonly IObjectResolver objectResolver;

        private FrameEventSprite sprite;
        private SpriteInstance spriteInstance;

        private Attachment<ZoneScene> mainHandItem;
        private Attachment<ZoneScene> offHandItem;

        private IPlayerSprite playerSpriteInfo;
        private Attachment<ZoneScene> mainHandHand;
        private Attachment<ZoneScene> offHandHand;

        private CharacterSheet characterSheet;

        private int primaryHand;
        private int secondaryHand;

        private bool disposed;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        private System.Numerics.Vector2 movementDir;
        private const float MovingBoundary = 0.001f;
        public bool IsMoving => !(movementDir.X < MovingBoundary && movementDir.X > -MovingBoundary
                             && movementDir.Y < MovingBoundary && movementDir.Y > -MovingBoundary);

        public record struct PersistedData
        {
            public Vector3? Location { get; set; }
        }

        class FollowerNode : IFollowerNode
        {
            private Follower follower;

            public FollowerNode(Follower follower)
            {
                this.follower = follower;
            }

            public Vector3 CurrentLocation => follower.currentPosition;

            public void UpdateLocation(FollowerManagerArgs args)
            {
                follower.SetLocation(args.NewLocation);
            }
        }
        private FollowerNode followerNode;

        public Follower
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
            IAssetFactory assetFactory
        )
        {
            playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));

            this.followerManager = description.FollowerManager;
            this.followerNode = new FollowerNode(this);
            this.followerManager.AddFollower(followerNode);

            this.assetFactory = assetFactory;
            this.characterSheet = description.CharacterSheet;

            this.primaryHand = description.PrimaryHand;
            this.secondaryHand = description.SecondaryHand;

            sprite = new FrameEventSprite(playerSpriteInfo.Animations);

            //Sub objects
            objectResolver = objectResolverFactory.Create();

            characterSheet.OnMainHandModified += OnMainHandModified;
            characterSheet.OnOffHandModified += OnOffHandModified;

            OnMainHandModified(characterSheet);
            OnOffHandModified(characterSheet);


            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.assetFactory = assetFactory;
            var scale = description.Scale * sprite.BaseScale;
            var halfScale = scale.y / 2f;
            var startPos = persistence.Current.Player.Position ?? description.Translation + new Vector3(0f, halfScale, 0f);

            this.currentPosition = startPos;
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Follower"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.SpriteMaterialDescription, sprite);

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
            characterSheet.OnMainHandModified -= OnMainHandModified;
            characterSheet.OnOffHandModified -= OnOffHandModified;
            sprite.FrameChanged -= Sprite_FrameChanged;
            sprite.AnimationChanged -= Sprite_AnimationChanged;
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            objectResolver.Dispose();
        }

        public void SetLocation(in Vector3 location)
        {
            //var finalLoc = location + new Vector3(0f, sprite.BaseScale.y / 2f, 0f);

            this.currentPosition = location;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
            Sprite_FrameChanged(sprite);
        }

        public Vector3 GetLocation()
        {
            return this.currentPosition - new Vector3(0f, sprite.BaseScale.y / 2f, 0f);
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        private void Sprite_AnimationChanged(FrameEventSprite obj)
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
                mainHandItem = objectResolver.Resolve<Attachment<ZoneScene>, Attachment<ZoneScene>.Description>(o =>
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
                    mainHandHand = objectResolver.Resolve<Attachment<ZoneScene>, Attachment<ZoneScene>.Description>(o =>
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
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
        }

        private void OnOffHandModified(CharacterSheet obj)
        {
            offHandItem?.RequestDestruction();
            offHandItem = null;
            if (characterSheet.OffHand?.Sprite != null)
            {
                offHandItem = objectResolver.Resolve<Attachment<ZoneScene>, Attachment<ZoneScene>.Description>(o =>
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
                    offHandHand = objectResolver.Resolve<Attachment<ZoneScene>, Attachment<ZoneScene>.Description>(o =>
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
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
        }
    }
}
