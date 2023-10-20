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

namespace Adventure.Exploration
{
    class PartyMemberTrigger : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public int ZoneIndex { get; set; }

            public int InstanceId { get; set; }

            public Vector3 MapOffset { get; set; }

            public String Sprite { get; set; }

            public Persistence.CharacterData PartyMember { get; set; }
        }

        public record struct PersistenceData(bool Found);

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly Persistence persistence;
        private readonly IExplorationMenu explorationMenu;
        private SpriteInstance spriteInstance;
        private readonly FrameEventSprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private int zoneIndex;
        private int instanceId;
        private PersistenceData state;
        private Persistence.CharacterData partyMember;
        private bool graphicsVisible = false;
        private bool graphicsLoaded;
        private readonly IPlayerSprite playerSpriteInfo;

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
            IExplorationMenu explorationMenu,
            IAssetFactory assetFactory)
        {
            playerSpriteInfo = assetFactory.CreatePlayer(description.Sprite ?? throw new InvalidOperationException($"You must include the {nameof(description.Sprite)} property in your description."));
            this.sprite = new FrameEventSprite(playerSpriteInfo.Animations);
            this.partyMember = description.PartyMember;
            this.zoneIndex = description.ZoneIndex;
            this.instanceId = description.InstanceId;
            this.state = persistence.Current.PartyMemberTriggers.GetData(zoneIndex, instanceId);
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.persistence = persistence;
            this.explorationMenu = explorationMenu;
            this.mapOffset = description.MapOffset;

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

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.SpriteMaterialDescription, sprite);

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
            if (!physicsCreated)
            {
                physicsCreated = true;
                var shape = new Box(currentScale.x, 1000, currentScale.z); //TODO: Each one creates its own, try to load from resources
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

        private void AddGraphics()
        {
            if (graphicsLoaded && !graphicsVisible)
            {
                graphicsVisible = true;

                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);
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
            //If something were to go wrong handing out treasure it would be lost, but the
            //other option opens it up to duplication
            state.Found = true;
            persistence.Current.PartyMemberTriggers.SetData(zoneIndex, instanceId, state);
            persistence.Current.Party.Members.Add(this.partyMember);

            RemoveGraphics();
            DestroyPhysics();
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }
    }
}
