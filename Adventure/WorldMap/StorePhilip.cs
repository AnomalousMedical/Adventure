﻿using Adventure.Menu;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class StorePhilip : IDisposable, IWorldMapPlaceable
    {
        public class Description : SceneObjectDesc
        {
            public Vector3[] Transforms { get; set; }

            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }
        }

        private readonly RTInstances<WorldMapScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly IWorldMapManager worldMapManager;
        private readonly Persistence persistence;
        private readonly IWorldDatabase worldDatabase;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData[] tlasData;
        private readonly IBepuScene<WorldMapScene> bepuScene;
        private readonly ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private bool graphicsCreated = false;
        private readonly Vector3[] transforms;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public StorePhilip(
            RTInstances<WorldMapScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<WorldMapScene> bepuScene,
            Description description,
            ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            IWorldMapManager worldMapManager,
            Persistence persistence,
            IWorldDatabase worldDatabase)
        {
            this.sprite = description.Sprite;
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.worldMapManager = worldMapManager;
            this.persistence = persistence;
            this.worldDatabase = worldDatabase;
            this.transforms = description.Transforms;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASInstanceData[description.Transforms.Length];
            for (var i = 0; i < tlasData.Length; i++)
            {
                this.tlasData[i] = new TLASInstanceData()
                {
                    InstanceName = RTId.CreateId("StorePhilip"),
                    Mask = RtStructures.OPAQUE_GEOM_MASK,
                    Transform = new InstanceMatrix(finalPosition + description.Transforms[i], currentOrientation, currentScale)
                };
            }

            MoveToPosition();

            if (persistence.Current.PlotItems.Contains(PlotItems.Phase2Shop))
            {
                coroutine.RunTask(async () =>
                {
                    await Task.Delay(1);
                    RequestDestruction();
                });
            }
            else
            {
                coroutine.RunTask(async () =>
                {
                    using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                    this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);

                    foreach (var data in tlasData)
                    {
                        spriteInstance.UpdateBlas(data);
                        rtInstances.AddTlasBuild(data);
                    }

                    rtInstances.AddShaderTableBinder(Bind);
                    rtInstances.AddSprite(sprite);

                    graphicsCreated = true;
                });
            }
        }

        public void CreatePhysics()
        {
            if (!physicsCreated)
            {
                physicsCreated = true;
                var shape = new Box(0.25f, 1000, 0.25f); //TODO: Each one creates its own, try to load from resources
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
            if (graphicsCreated)
            {
                spriteInstanceFactory.TryReturn(spriteInstance);
                rtInstances.RemoveSprite(sprite);
                rtInstances.RemoveShaderTableBinder(Bind);
                foreach (var data in tlasData)
                {
                    rtInstances.RemoveTlasBuild(data);
                }
            }
            DestroyPhysics();
        }

        public void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        protected virtual void HandleCollision(CollisionEvent evt)
        {
            if (collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.A, out var player)
               || collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.B, out player))
            {
                contextMenu.HandleContext("Talk", Talk, player.GamepadId);
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Talk);
        }

        private void Talk(ContextMenuArgs args)
        {
            contextMenu.ClearContext(Talk);
            if (persistence.Current.PlotItems.Contains(PlotItems.Phase2Shop))
            {
                //Nothing actually happens here, you have the best store, but this should not appear anyway since this object won't spawn
            }
            else if (persistence.Current.PlotItems.Contains(PlotItems.Phase1Shop))
            {
                persistence.Current.PlotItems.Add(PlotItems.Phase2Shop);
                RequestDestruction();
            }
            else
            {
                persistence.Current.PlotItems.Add(PlotItems.Phase1Shop);
            }

            MoveToPosition();
        }

        private void MoveToPosition()
        {
            IntVector2 targetCell;

            if (persistence.Current.PlotItems.Contains(PlotItems.Phase1Shop))
            {
                targetCell = worldDatabase.StorePhilipLocations[1];
            }
            else
            {
                targetCell = worldDatabase.StorePhilipLocations[0];
            }

            currentPosition = worldMapManager.GetCellCenterpoint(targetCell);
            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            for (var i = 0; i < tlasData.Length; i++)
            {
                tlasData[i].Transform = new InstanceMatrix(finalPosition + transforms[i], currentOrientation, currentScale);
            }

            if (physicsCreated)
            {
                DestroyPhysics();
                CreatePhysics();
            }
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            foreach (var data in tlasData)
            {
                spriteInstance.Bind(data.InstanceName, sbt, tlas, sprite);
            }
        }
    }
}
