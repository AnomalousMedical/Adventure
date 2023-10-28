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

            public BiomeEnemy TriggerEnemy { get; set; }

            public int Area { get; set; }

            public int Zone { get; set; }

            public int Index { get; set; }

            public int BattleSeed { get; set; }

            public int EnemyLevel { get; set; }

            public bool IsBoss { get; set; }
        }

        public record struct PersistenceData(bool Dead, bool StolenFrom);
        private PersistenceData state;

        public record struct UniqueStolenTreasureData(bool Stolen);

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly Description description;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly IExplorationGameState explorationGameState;
        private readonly Persistence persistence;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private bool graphicsVisible = false;
        private bool graphicsLoaded = false;
        private List<ITreasure> stealTreasures;
        private bool hasUniqueStolenTreasure = false;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public int BattleSeed { get; }
        public int EnemyLevel { get; }
        public bool IsBoss { get; init; }

        public BiomeEnemy TriggerEnemy { get; set; }

        public BattleTrigger(
            RTInstances<ZoneScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<ZoneScene> bepuScene,
            Description description,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IExplorationGameState explorationGameState,
            Persistence persistence)
        {
            state = GetState(description, persistence);

            this.IsBoss = description.IsBoss;
            this.EnemyLevel = description.EnemyLevel;
            this.BattleSeed = description.BattleSeed;
            this.sprite = description.TriggerEnemy.Asset.CreateSprite();
            this.sprite.RandomizeFrameTime();
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.description = description;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.explorationGameState = explorationGameState;
            this.persistence = persistence;
            this.mapOffset = description.MapOffset;
            this.TriggerEnemy = description.TriggerEnemy;

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

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.TriggerEnemy.Asset.CreateMaterial(), sprite);

                graphicsLoaded = true;

                AddGraphics();
            });
        }

        public void BattleWon()
        {
            if (IsBoss)
            {
                persistence.Current.World.CompletedAreaLevels[description.Area] = EnemyLevel;
                explorationGameState.LevelUpWorld();
            }

            state.Dead = true;
            SetState(description, persistence, state);
            DestroyPhysics();
            RemoveGraphics();
        }

        public void Reset()
        {
            state = GetState(description, persistence);
            AddGraphics();
        }

        public void CreatePhysics()
        {
            if(this.state.Dead) { return; }

            if (!physicsCreated)
            {
                var x = MathF.Max(currentScale.x, 1);
                var z = MathF.Max(currentScale.z, 1);

                physicsCreated = true;
                var shape = new Box(x, 1000, z); //TODO: Each one creates its own, try to load from resources
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
            spriteInstanceFactory.TryReturn(spriteInstance);
            RemoveGraphics();
            DestroyPhysics();
        }

        private void AddGraphics()
        {
            if (!graphicsLoaded || state.Dead) { return; }

            if (!graphicsVisible)
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

        public void AddStealTreasure(ITreasure treasure)
        {
            if (state.StolenFrom)
            {
                return;
            }

            EnsureStealTreasures();
            stealTreasures.Add(treasure);
        }

        public void AddUniqueStealTreasure(ITreasure treasure)
        {
            if (state.StolenFrom)
            {
                return;
            }

            var uniqueStolenState = GetUniqueTreasureStolenState(description, persistence);
            if (!uniqueStolenState.Stolen)
            {
                //If the unique treasure is marked stolen, ignore it again forever
                hasUniqueStolenTreasure = true;
                EnsureStealTreasures();
                stealTreasures.Add(treasure);
            }
        }

        public IEnumerable<ITreasure> StealTreasure()
        {
            if (state.StolenFrom)
            {
                return Enumerable.Empty<ITreasure>();
            }

            if (stealTreasures == null)
            {
                //Return bag of money or something here, since so specific assigned treasure
                //but we don't have that yet
                return Enumerable.Empty<ITreasure>();
            }

            state.StolenFrom = true;
            SetState(description, persistence, state);

            if (hasUniqueStolenTreasure)
            {
                SetUniqueStolenTreasureState(description, persistence, new UniqueStolenTreasureData(true));
            }

            var treasures = stealTreasures;
            stealTreasures = null;

            return treasures;
        }

        private void EnsureStealTreasures()
        {
            if (stealTreasures == null)
            {
                stealTreasures = new List<ITreasure>(3);
            }
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
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }

        private static PersistenceData GetState(Description description, Persistence persistence)
        {
            PersistenceData result;
            if (description.IsBoss)
            {
                result = persistence.Current.BossBattleTriggers.GetData(description.Zone, description.Index);
            }
            else
            {
                result = persistence.Current.BattleTriggers.GetData(description.Zone, description.Index);
            }
            return result;
        }

        private static void SetState(Description description, Persistence persistence, PersistenceData data)
        {
            if (description.IsBoss)
            {
                persistence.Current.BossBattleTriggers.SetData(description.Zone, description.Index, data);
            }
            else
            {
                persistence.Current.BattleTriggers.SetData(description.Zone, description.Index, data);
            }
        }

        private static UniqueStolenTreasureData GetUniqueTreasureStolenState(Description description, Persistence persistence)
        {
            UniqueStolenTreasureData result;
            if (description.IsBoss)
            {
                result = persistence.Current.UniqueBossStolenTreasure.GetData(description.Zone, description.Index);
            }
            else
            {
                result = persistence.Current.UniqueStolenTreasure.GetData(description.Zone, description.Index);
            }
            return result;
        }

        private static void SetUniqueStolenTreasureState(Description description, Persistence persistence, UniqueStolenTreasureData data)
        {
            if (description.IsBoss)
            {
                persistence.Current.UniqueBossStolenTreasure.SetData(description.Zone, description.Index, data);
            }
            else
            {
                persistence.Current.UniqueStolenTreasure.SetData(description.Zone, description.Index, data);
            }
        }
    }
}
