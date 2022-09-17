﻿using BepuPhysics;
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
    class PlotItemPlaceable : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public int ZoneIndex { get; set; }

            public int InstanceId { get; set; }

            public Vector3 MapOffset { get; set; }

            public Sprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }

            public PlotItems PlotItem { get; set; }
        }

        private readonly RTInstances<IZoneManager> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly Persistence persistence;
        private SpriteInstance spriteInstance;
        private readonly Sprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<IExplorationGameState> bepuScene;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private bool graphicsCreated = false;
        private bool graphicsLoaded = false;
        private int zoneIndex;
        private int instanceId;
        private bool taken = false;
        private PlotItems plotItem;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public PlotItemPlaceable(
            RTInstances<IZoneManager> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<IExplorationGameState> bepuScene,
            Description description,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            Persistence persistence)
        {
            this.sprite = description.Sprite;
            this.zoneIndex = description.ZoneIndex;
            this.instanceId = description.InstanceId;
            this.plotItem = description.PlotItem;
            this.taken = persistence.Current.PlotItems.Contains(plotItem);
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.persistence = persistence;
            this.mapOffset = description.MapOffset;

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

                if (!taken)
                {
                    AddGraphics();
                }
            });
        }

        public void Reset()
        {
            this.taken = persistence.Current.PlotItems.Contains(plotItem);
            if (!taken)
            {
                AddGraphics();
            }
            else
            {
                DestroyGraphics();
            }
        }

        public void CreatePhysics()
        {
            if (!taken && !physicsCreated)
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

        public void Dispose()
        {
            DestroyGraphics();
            DestroyPhysics();
            spriteInstanceFactory.TryReturn(spriteInstance);
        }

        private void AddGraphics()
        {
            if(!graphicsLoaded || taken) { return; }

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
                if (!taken)
                {
                    contextMenu.HandleContext("Take", Take, player.GamepadId);
                }
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Take);
        }

        private void Take(ContextMenuArgs args)
        {
            contextMenu.ClearContext(Take);
            taken = true;
            persistence.Current.PlotItems.Add(plotItem);
            DestroyGraphics();
            DestroyPhysics();
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }
    }
}
