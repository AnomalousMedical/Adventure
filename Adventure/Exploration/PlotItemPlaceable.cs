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
using Adventure.Assets;
using Adventure.Assets.World;

namespace Adventure
{
    class PlotItemPlaceable : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public int ZoneIndex { get; set; }

            public int InstanceId { get; set; }

            public Vector3 MapOffset { get; set; }

            public PlotItems PlotItem { get; set; }
        }

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly Persistence persistence;
        private readonly TypedLightManager<ZoneScene> lightManager;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly ICollidableTypeIdentifier<ZoneScene> collidableIdentifier;
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
        private Light light;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public PlotItemPlaceable(
            RTInstances<ZoneScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<ZoneScene> bepuScene,
            Description description,
            ICollidableTypeIdentifier<ZoneScene> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            Persistence persistence,
            TypedLightManager<ZoneScene> lightManager)
        {
            ISpriteAsset asset;

            switch (description.PlotItem)
            {
                case PlotItems.AirshipWheel:
                    asset = new ShipWheel();
                    break;
                case PlotItems.AirshipFuel:
                    asset = new ShipFuel();
                    break;
                default:
                    asset = new RoundKey();
                    break;
            }

            this.sprite = asset.CreateSprite();
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
            this.lightManager = lightManager;
            this.mapOffset = description.MapOffset;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("PlotItem"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition, currentOrientation, currentScale)
            };

            light = asset.CreateLight();
            if (light != null)
            {
                var lightPosition = this.currentPosition;
                if (asset.LightAttachmentChannel != null)
                {
                    var lightAttachment = sprite.GetCurrentFrame().Attachments[asset.LightAttachmentChannel.Value];
                    lightPosition += lightAttachment.translate;
                }
                light.Position = lightPosition.ToVector4();
            }

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(asset.CreateMaterial(), sprite);
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
            if(!graphicsLoaded || taken) { return; }

            if (!graphicsCreated)
            {
                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);
                if (light != null)
                {
                    lightManager.AddLight(light);
                }

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
                if (light != null)
                {
                    lightManager.RemoveLight(light);
                }

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
