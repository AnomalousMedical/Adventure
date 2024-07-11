using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Adventure.Menu;
using Adventure.Services;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class EndGameTrigger : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public int ZoneIndex { get; set; }

            public Vector3 MapOffset { get; set; }

            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }

            public int RespawnBossIndex { get; set; }
        }

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly IExplorationGameState explorationGameState;
        private readonly Persistence persistence;
        private SpriteInstance spriteInstance;
        private bool graphicsLoaded = false;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly ICollidableTypeIdentifier<ZoneScene> collidableIdentifier;
        private readonly Vector3 mapOffset;
        private readonly int respawnBossIndex;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private bool graphicsCreated = false;
        private int zoneIndex;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public EndGameTrigger
        (
            RTInstances<ZoneScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<ZoneScene> bepuScene,
            Description description,
            ICollidableTypeIdentifier<ZoneScene> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            IExplorationGameState explorationGameState,
            Persistence persistence
        )
        {
            this.sprite = description.Sprite;
            this.zoneIndex = description.ZoneIndex;
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.explorationGameState = explorationGameState;
            this.persistence = persistence;
            this.mapOffset = description.MapOffset;
            this.respawnBossIndex = description.RespawnBossIndex;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Key"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);
                this.graphicsLoaded = true;

                AddGraphics();
            });
        }

        public void Reset()
        {
            //Does nothing, no state to reset
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

        public void Dispose()
        {
            DestroyGraphics();
            DestroyPhysics();
            spriteInstanceFactory.TryReturn(spriteInstance);
        }

        private void AddGraphics()
        {
            if (!graphicsCreated)
            {
                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);

                graphicsCreated = true;
            }
        }

        private void DestroyGraphics()
        {
            if (graphicsCreated)
            {
                rtInstances.RemoveSprite(sprite);
                rtInstances.RemoveShaderTableBinder(Bind);
                rtInstances.RemoveTlasBuild(tlasData);
                graphicsCreated = false;
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
                contextMenu.HandleContext("End Game", EndGame, player.GamepadId);
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(EndGame);
        }

        private void EndGame(ContextMenuArgs args)
        {
            contextMenu.ClearContext(EndGame);

            //Respawn final boss
            var bossState = persistence.Current.BossBattleTriggers.GetData(zoneIndex, 0);
            bossState.Dead = false;
            persistence.Current.BossBattleTriggers.SetData(zoneIndex, respawnBossIndex, bossState);

            explorationGameState.RequestVictory();

            //DestroyGraphics();
            //DestroyPhysics();
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }
    }
}
