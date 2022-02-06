using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class BattleTrigger : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public Vector3 MapOffset { get; set; }

            public Sprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }

            public int Zone { get; set; }

            public int Index { get; set; }

            public int BattleSeed { get; set; }

            public int EnemyLevel { get; set; }
        }

        public record struct PersistenceData(bool Dead);
        private PersistenceData state;

        private readonly RTInstances<IZoneManager> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private SpriteInstance spriteInstance;
        private readonly Sprite sprite;
        private readonly TLASBuildInstanceData tlasData;
        private readonly IBepuScene bepuScene;
        private readonly Description description;
        private readonly ICollidableTypeIdentifier collidableIdentifier;
        private readonly IExplorationGameState explorationGameState;
        private readonly Persistence persistence;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        private bool notCreated = true;

        public int BattleSeed { get; }
        public int EnemyLevel { get; }

        public BattleTrigger(
            RTInstances<IZoneManager> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene bepuScene,
            Description description,
            ICollidableTypeIdentifier collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IExplorationGameState explorationGameState,
            Persistence persistence)
        {
            state = persistence.BattleTriggers.GetData(description.Zone, description.Index);
            if (state.Dead)
            {
                return;
            }

            this.EnemyLevel = description.EnemyLevel;
            this.BattleSeed = description.BattleSeed;
            this.notCreated = false;
            this.sprite = description.Sprite;
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.description = description;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.explorationGameState = explorationGameState;
            this.persistence = persistence;
            this.mapOffset = description.MapOffset;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASBuildInstanceData()
            {
                InstanceName = RTId.CreateId("BattleTrigger"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial);

                this.tlasData.pBLAS = spriteInstance.Instance.BLAS.Obj;
                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite);
            });
        }

        public void BattleWon()
        {
            state.Dead = true;
            persistence.BattleTriggers.SetData(description.Zone, description.Index, state);
            this.RequestDestruction();
        }

        public void CreatePhysics()
        {
            if (this.notCreated) { return; }

            if(this.state.Dead) { return; }

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

                bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), collisionEvent: HandleCollision);
            }
        }

        public void DestroyPhysics()
        {
            if (this.notCreated) { return; }

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
            if (this.notCreated) { return; }

            spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            DestroyPhysics();
        }

        public void RequestDestruction()
        {
            if (this.notCreated) { return; }
            this.destructionRequest.RequestDestruction();
        }

        public void SetZonePosition(in Vector3 zonePosition)
        {
            if (this.notCreated) { return; }
            currentPosition = zonePosition + mapOffset;

            var totalScale = sprite.BaseScale * currentScale;
            currentPosition.y += totalScale.y / 2;
            this.tlasData.Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale);
        }

        private void HandleCollision(CollisionEvent evt)
        {
            if (collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.A, out var _)
                || collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.B, out var _))
            {
                explorationGameState.RequestBattle(this);
            }
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite.GetCurrentFrame());
        }
    }
}
