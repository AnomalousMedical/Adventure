﻿using BepuPhysics;
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
    class Gate : IDisposable, IZonePlaceable
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

            public bool IsBoss { get; set; }
        }

        public record struct PersistenceData(bool Dead);
        private PersistenceData state;
        Persistence.PersistenceEntry<PersistenceData> persistentStorage;

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
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private bool graphicsVisible = false;
        private bool graphicsLoaded = false;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        private Quaternion blasRotation;
        private Vector3 blasOffset;

        public int BattleSeed { get; }
        public int EnemyLevel { get; }
        public bool IsBoss { get; init; }

        public Gate(
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
            //persistentStorage = description.IsBoss ? persistence.BossBattleTriggers : persistence.BattleTriggers;
            //state = persistentStorage.GetData(description.Zone, description.Index);

            this.IsBoss = description.IsBoss;
            this.EnemyLevel = description.EnemyLevel;
            this.BattleSeed = description.BattleSeed;
            this.sprite = description.Sprite;
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.description = description;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.explorationGameState = explorationGameState;
            this.mapOffset = description.MapOffset;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            blasRotation = new Quaternion(Vector3.UnitY, 0.48f * MathF.PI);
            blasOffset = new Vector3(-0.85f, 0f, 0f);

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASBuildInstanceData()
            {
                InstanceName = RTId.CreateId("Gate"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition + blasOffset, currentOrientation * blasRotation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial);

                this.tlasData.pBLAS = spriteInstance.Instance.BLAS.Obj;
                graphicsLoaded = true;

                AddGraphics();
            });
        }

        public void BattleWon()
        {
            state.Dead = true;
            //persistentStorage.SetData(description.Zone, description.Index, state);
            DestroyPhysics();
            RemoveGraphics();
        }

        public void Reset()
        {
            //state = persistentStorage.GetData(description.Zone, description.Index);
            //AddGraphics();
        }

        public void CreatePhysics()
        {
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
                rtInstances.AddSprite(sprite);
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
            this.tlasData.Transform = new InstanceMatrix(currentPosition + blasOffset, currentOrientation * blasRotation, currentScale);
        }

        private void HandleCollision(CollisionEvent evt)
        {
            
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite.GetCurrentFrame());
        }
    }
}
