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
using System.Threading.Tasks;

namespace Adventure
{
    class FollowerDescription : SceneObjectDesc
    {
        public int PrimaryHand = Player.RightHand;

        public int SecondaryHand = Player.LeftHand;

        public String PlayerSprite { get; set; }

        public CharacterSheet CharacterSheet { get; set; }

        public FollowerManager FollowerManager { get; set; }

        public bool StartVisible { get; set; } = true;
    }

    class Follower<T> : IDisposable
    {
        private readonly RTInstances<T> rtInstances;
        private readonly TLASInstanceData tlasData;
        private readonly IDestructionRequest destructionRequest;
        private readonly IScopedCoroutine coroutine;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IAssetFactory assetFactory;
        private readonly FollowerManager followerManager;
        private readonly IObjectResolver objectResolver;

        private FrameEventSprite sprite;
        private SpriteInstance spriteInstance;

        private Attachment<T> mainHandItem;
        private Attachment<T> offHandItem;

        private IPlayerSprite playerSpriteInfo;
        private Attachment<T> mainHandHand;
        private Attachment<T> offHandHand;

        private CharacterSheet characterSheet;

        private int primaryHand;
        private int secondaryHand;

        private bool disposed;
        private bool graphicsActive;
        private bool graphicsReady;
        private bool makeGraphicsActive;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public record struct PersistedData
        {
            public Vector3? Location { get; set; }
        }

        class FollowerNode : IFollowerNode
        {
            private Follower<T> follower;

            public FollowerNode(Follower<T> follower)
            {
                this.follower = follower;
            }

            public void UpdateLocation(FollowerManagerArgs args)
            {
                follower.SetLocationAndMovement(args.NewLocation, args.MovementDirection, args.Moving);
            }
        }
        private FollowerNode followerNode;

        public Follower
        (
            RTInstances<T> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            SpriteInstanceFactory spriteInstanceFactory,
            IObjectResolverFactory objectResolverFactory,
            FollowerDescription description,
            Persistence persistence,
            IAssetFactory assetFactory
        )
        {
            playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));

            this.makeGraphicsActive = description.StartVisible;
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
            characterSheet.OnBodyModified += CharacterSheet_OnBodyModified;

            OnMainHandModified(characterSheet);
            OnOffHandModified(characterSheet);


            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.coroutine = coroutine;
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

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.Tier1, sprite);
                graphicsReady = true;

                if (this.disposed)
                {
                    this.spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }

                SetGraphicsActive(makeGraphicsActive);

                sprite.AnimationChanged += Sprite_AnimationChanged;
                sprite.FrameChanged += Sprite_FrameChanged;
                Sprite_AnimationChanged(sprite);
                Sprite_FrameChanged(sprite);
            });
        }

        public void Dispose()
        {
            disposed = true;
            this.followerManager.RemoveFollower(followerNode);
            characterSheet.OnBodyModified -= CharacterSheet_OnBodyModified;
            characterSheet.OnMainHandModified -= OnMainHandModified;
            characterSheet.OnOffHandModified -= OnOffHandModified;
            sprite.FrameChanged -= Sprite_FrameChanged;
            sprite.AnimationChanged -= Sprite_AnimationChanged;
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            SetGraphicsActive(false);
            objectResolver.Dispose();
        }

        public void SetLocationAndMovement(in Vector3 location, in Vector3 movementDir, bool moving)
        {
            //var finalLoc = location + new Vector3(0f, sprite.BaseScale.y / 2f, 0f);

            if (movementDir.z > 0.3f)
            {
                var anim = moving ? "up" : "stand-up";
                sprite.SetAnimation(anim);
                mainHandItem?.SetAnimation(anim);
                offHandItem?.SetAnimation(anim);
            }
            else if (movementDir.z < -0.3f)
            {
                var anim = moving ? "down" : "stand-down";
                sprite.SetAnimation(anim);
                mainHandItem?.SetAnimation(anim);
                offHandItem?.SetAnimation(anim);
            }
            else if (movementDir.x > 0)
            {
                var anim = moving ? "right" : "stand-right";
                sprite.SetAnimation(anim);
                mainHandItem?.SetAnimation(anim);
                offHandItem?.SetAnimation(anim);
            }
            else if (movementDir.x < 0)
            {
                var anim = moving ? "left" : "stand-left";
                sprite.SetAnimation(anim);
                mainHandItem?.SetAnimation(anim);
                offHandItem?.SetAnimation(anim);
            }

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

        public void SetGraphicsActive(bool active)
        {
            makeGraphicsActive = active;
            if (graphicsActive != active && graphicsReady)
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
                mainHandItem = objectResolver.Resolve<Attachment<T>, Attachment<T>.Description>(o =>
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
                    mainHandHand = objectResolver.Resolve<Attachment<T>, Attachment<T>.Description>(o =>
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
                offHandItem = objectResolver.Resolve<Attachment<T>, Attachment<T>.Description>(o =>
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
                    offHandHand = objectResolver.Resolve<Attachment<T>, Attachment<T>.Description>(o =>
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
    }
}
