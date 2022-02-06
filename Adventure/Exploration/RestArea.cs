﻿using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using Adventure.Exploration.Menu;
using Adventure.Services;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class RestArea : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public int ZoneIndex { get; set; }

            public int InstanceId { get; set; }

            public Vector3 MapOffset { get; set; }

            public Sprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }
        }

        private readonly RTInstances<IZoneManager> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly Persistence persistence;
        private readonly IZoneManager zoneManager;
        private readonly IExplorationGameState explorationGameState;
        private readonly ITimeClock timeClock;
        private SpriteInstance spriteInstance;
        private readonly Sprite sprite;
        private readonly TLASBuildInstanceData tlasData;
        private readonly IBepuScene bepuScene;
        private readonly ICollidableTypeIdentifier collidableIdentifier;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool disposed = false;
        private int levelIndex;
        private int instanceId;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public RestArea(
            RTInstances<IZoneManager> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene bepuScene,
            Description description,
            ICollidableTypeIdentifier collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            Persistence persistence,
            IZoneManager zoneManager,
            IExplorationGameState explorationGameState,
            ITimeClock timeClock)
        {
            this.sprite = description.Sprite;
            this.levelIndex = description.ZoneIndex;
            this.instanceId = description.InstanceId;
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.persistence = persistence;
            this.zoneManager = zoneManager;
            this.explorationGameState = explorationGameState;
            this.timeClock = timeClock;
            this.mapOffset = description.MapOffset;
            var shape = new Box(description.Scale.x, 1000, description.Scale.z); //TODO: Each one creates its own, try to load from resources
            shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

            staticHandle = bepuScene.Simulation.Statics.Add(
                new StaticDescription(
                    new System.Numerics.Vector3(description.Translation.x, description.Translation.y, description.Translation.z),
                    new System.Numerics.Quaternion(description.Orientation.x, description.Orientation.y, description.Orientation.z, description.Orientation.w),
                    new CollidableDescription(shapeIndex, 0.1f)));

            RegisterCollision();

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASBuildInstanceData()
            {
                InstanceName = RTId.CreateId("RestArea"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial);

                if (this.disposed)
                {
                    this.spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }

                this.tlasData.pBLAS = spriteInstance.Instance.BLAS.Obj;
                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite);
            });
        }

        public void BattleWon()
        {
            this.RequestDestruction();
        }

        public void Dispose()
        {
            disposed = true;
            spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
            bepuScene.Simulation.Shapes.Remove(shapeIndex);
            bepuScene.Simulation.Statics.Remove(staticHandle);
        }

        public void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        public void SetZonePosition(in Vector3 zonePosition)
        {
            bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
            bepuScene.Simulation.Statics.Remove(this.staticHandle);
            currentPosition = zonePosition + mapOffset;

            staticHandle = bepuScene.Simulation.Statics.Add(
            new StaticDescription(
                currentPosition.ToSystemNumerics(),
                Quaternion.Identity.ToSystemNumerics(),
                new CollidableDescription(shapeIndex, 0.1f)));
            RegisterCollision();

            var totalScale = sprite.BaseScale * currentScale;
            currentPosition.y += totalScale.y / 2;
        }

        private void RegisterCollision()
        {
            bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), collisionEvent: HandleCollision, endEvent: HandleCollisionEnd);
        }

        private void HandleCollision(CollisionEvent evt)
        {
            contextMenu.HandleContext("Rest", Rest);
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Rest);
        }

        private void Rest()
        {
            contextMenu.ClearContext(Rest);
            persistence.BattleTriggers.ClearData();
            timeClock.SetTimeRatio(100);

            persistence.Player.RespawnLevel = levelIndex;
            persistence.Player.RespawnPosition = zoneManager.GetPlayerLoc();

            long? endTime = null;

            explorationGameState.SetExplorationEvent(c =>
            {
                if(endTime == null)
                {
                    endTime = c.CurrentTimeMicro + (long)(Clock.SecondsToMicro * 1.3f);
                }

                if(c.CurrentTimeMicro > endTime)
                {
                    zoneManager.RebuildPhysics();

                    foreach (var member in persistence.Party.Members)
                    {
                        member.CharacterSheet.Rest();
                    }

                    timeClock.ResetTimeFactor();
                    return false;
                }

                return true;
            });
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite.GetCurrentFrame());
        }
    }
}
