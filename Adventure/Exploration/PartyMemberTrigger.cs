using Adventure.Menu;
using Adventure.Services;
using BepuPhysics.Collidables;
using BepuPhysics;
using BepuPlugin;
using DiligentEngine.RT.Sprites;
using DiligentEngine.RT;
using DiligentEngine;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adventure.Assets;
using RpgMath;

namespace Adventure.Exploration
{
    class PartyMemberTrigger : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public int PrimaryHand = RightHand;

            public int SecondaryHand = LeftHand;

            public int ZoneIndex { get; set; }

            public int InstanceId { get; set; }

            public Vector3 MapOffset { get; set; }

            public String Sprite { get; set; }

            public PartyMember PartyMember { get; set; }
        }

        public const int RightHand = 0;
        public const int LeftHand = 1;

        public record struct PartyMemberTriggerPersistenceData(bool Found);

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly IScopedCoroutine coroutine;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly Persistence persistence;
        private readonly IAssetFactory assetFactory;
        private readonly TextDialog textDialog;
        private readonly PartyMemberManager partyMemberManager;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private int zoneIndex;
        private int instanceId;
        private PartyMemberTriggerPersistenceData state;
        private PartyMember partyMember;
        private bool graphicsVisible = false;
        private bool graphicsLoaded;
        private readonly IObjectResolver objectResolver;
        private readonly IPlayerSprite playerSpriteInfo;
        private int primaryHand;
        private int secondaryHand;

        private Attachment<ZoneScene> mainHandItem;
        private Attachment<ZoneScene> offHandItem;
        private Attachment<ZoneScene> mainHandHand;
        private Attachment<ZoneScene> offHandHand;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public PartyMemberTrigger(
            RTInstances<ZoneScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<ZoneScene> bepuScene,
            Description description,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            Persistence persistence,
            IAssetFactory assetFactory,
            IObjectResolverFactory objectResolverFactory,
            TextDialog textDialog,
            PartyMemberManager partyMemberManager)
        {
            objectResolver = objectResolverFactory.Create();
            playerSpriteInfo = assetFactory.CreatePlayer(description.Sprite ?? throw new InvalidOperationException($"You must include the {nameof(description.Sprite)} property in your description."));
            this.sprite = new Sprite(playerSpriteInfo.Animations);
            this.partyMember = description.PartyMember;
            this.zoneIndex = description.ZoneIndex;
            this.instanceId = description.InstanceId;
            this.state = persistence.Current.PartyMemberTriggers.GetData(zoneIndex, instanceId);
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.coroutine = coroutine;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.persistence = persistence;
            this.assetFactory = assetFactory;
            this.textDialog = textDialog;
            this.partyMemberManager = partyMemberManager;
            this.mapOffset = description.MapOffset;
            this.primaryHand = description.PrimaryHand;
            this.secondaryHand = description.SecondaryHand;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("BattleTrigger"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.GetTier(partyMember.CharacterData.CharacterSheet.EquipmentTier), sprite);

                graphicsLoaded = true;

                if (!state.Found)
                {
                    AddGraphics();
                }
            });
        }

        public void Dispose()
        {
            RemoveGraphics();
            DestroyPhysics();

            spriteInstanceFactory.TryReturn(spriteInstance);
        }

        public void Reset()
        {
            this.state = persistence.Current.PartyMemberTriggers.GetData(zoneIndex, instanceId);
            if (state.Found)
            {
                RemoveGraphics();
                DestroyPhysics();
            }
            else
            {
                AddGraphics();
                CreatePhysics();
            }
        }

        public void CreatePhysics()
        {
            this.state = persistence.Current.PartyMemberTriggers.GetData(zoneIndex, instanceId);
            if (this.state.Found)
            {
                return;
            }

            if (!physicsCreated)
            {
                physicsCreated = true;
                var shape = new Box(currentScale.x, 1000, currentScale.z); //TODO: Each one creates its own, try to load from resources
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

        private void AddGraphics()
        {
            if (graphicsLoaded && !graphicsVisible)
            {
                graphicsVisible = true;

                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);

                //This version does not subscribe to character sheet events, since the trigger is not under anything's control
                OnMainHandModified(partyMember.CharacterData.CharacterSheet);
                OnOffHandModified(partyMember.CharacterData.CharacterSheet);
            }
        }

        private void RemoveGraphics()
        {
            if (graphicsVisible)
            {
                graphicsVisible = false;
                rtInstances.RemoveSprite(sprite);
                rtInstances.RemoveShaderTableBinder(Bind);
                rtInstances.RemoveTlasBuild(tlasData);

                mainHandItem?.RequestDestruction();
                mainHandItem = null;

                offHandItem?.RequestDestruction();
                offHandItem = null;

                mainHandHand?.RequestDestruction();
                mainHandHand = null;

                offHandHand?.RequestDestruction();
                offHandHand = null;
            }
        }

        public void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        public void SetZonePosition(in Vector3 zonePosition)
        {
            currentPosition = zonePosition + mapOffset;
            currentPosition.y += currentScale.y / 2;
            this.tlasData.Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale);
            Sprite_FrameChanged(sprite);
        }

        private void HandleCollision(CollisionEvent evt)
        {
            if (collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.A, out var player)
               || collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.B, out player))
            {
                if (!state.Found)
                {
                    contextMenu.HandleContext("Hello", Recruit, player.GamepadId);
                }
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Recruit);
        }

        private void Recruit(ContextMenuArgs args)
        {
            contextMenu.ClearContext(Recruit);

            coroutine.RunTask(async () =>
            {
                await textDialog.ShowTextAndWait(this.partyMember.Greeting, args.GamepadId);

                //If something were to go wrong handing out the party member it would be lost, but the
                //other option opens it up to duplication
                state.Found = true;
                persistence.Current.PartyMemberTriggers.SetData(zoneIndex, instanceId, state);
                partyMemberManager.AddToParty(this.partyMember);

                RemoveGraphics();
                DestroyPhysics();
            });
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }

        private void OnMainHandModified(CharacterSheet characterSheet)
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

        private void OnOffHandModified(CharacterSheet characterSheet)
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
            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            var frame = obj.GetCurrentFrame();

            Vector3 offset;
            var scale = sprite.BaseScale * this.currentScale;

            var primaryAttach = frame.Attachments[this.primaryHand];
            offset = scale * primaryAttach.translate;
            offset = Quaternion.quatRotate(this.currentOrientation, offset) + finalPosition;
            mainHandItem?.SetPosition(offset, this.currentOrientation, scale);
            mainHandHand?.SetPosition(offset, this.currentOrientation, scale);

            var secondaryAttach = frame.Attachments[this.secondaryHand];
            offset = scale * secondaryAttach.translate;
            offset = Quaternion.quatRotate(this.currentOrientation, offset) + finalPosition;
            offHandItem?.SetPosition(offset, this.currentOrientation, scale);
            offHandHand?.SetPosition(offset, this.currentOrientation, scale);
        }
    }
}
