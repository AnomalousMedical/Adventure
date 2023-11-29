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
using Adventure.Menu;

namespace Adventure
{
    class Gate : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public Vector3 MapOffset { get; set; }

            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }

            public int Zone { get; set; }

            public int InstanceId { get; set; }

            public bool Rotated { get; set; }
        }

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly Description description;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly IContextMenu contextMenu;
        private readonly Persistence persistence;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private bool graphicsVisible = false;
        private bool graphicsLoaded = false;
        private Key.KeyPersistenceData state;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        private Quaternion blasRotation;
        private Vector3 blasOffset;

        public Gate
        (
            RTInstances<ZoneScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<ZoneScene> bepuScene,
            Description description,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            Persistence persistence
        )
        {
            this.state = persistence.Current.Keys.GetData(description.Zone, description.InstanceId);
            this.sprite = description.Sprite;
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.description = description;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.persistence = persistence;
            this.mapOffset = description.MapOffset;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            blasRotation = description.Rotated ? new Quaternion(Vector3.UnitY, 0.48f * MathF.PI) : Quaternion.Identity;
            blasOffset = description.Rotated ? new Vector3(-0.85f, 0f, 0f) : Vector3.Zero;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Gate"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition + blasOffset, currentOrientation * blasRotation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);

                graphicsLoaded = true;

                AddGraphics();
            });
        }

        public void Reset()
        {
            state = persistence.Current.Keys.GetData(description.Zone, description.InstanceId);
            if (state.GateOpened)
            {
                AddGraphics();
            }
            else
            {
                RemoveGraphics();
            }
        }

        public void CreatePhysics()
        {
            if(this.state.GateOpened) { return; }

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

        public void Dispose()
        {
            spriteInstanceFactory.TryReturn(spriteInstance);
            RemoveGraphics();
            DestroyPhysics();
        }

        private void AddGraphics()
        {
            if (!graphicsLoaded || state.GateOpened) { return; }

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
            this.tlasData.Transform = new InstanceMatrix(currentPosition + blasOffset, currentOrientation * blasRotation, currentScale);
        }

        private void HandleCollision(CollisionEvent evt)
        {
            if (collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.A, out var player)
             || collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.B, out player))
            {
                this.state = persistence.Current.Keys.GetData(description.Zone, description.InstanceId);
                if (!state.GateOpened)
                {
                    if (state.Taken)
                    {
                        contextMenu.HandleContext("Open", Open, player.GamepadId);
                    }
                    else
                    {
                        contextMenu.HandleContext("Need Key", Open, player.GamepadId);
                    }
                }
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Open);
        }

        private void Open(ContextMenuArgs args)
        {
            if (state.Taken)
            {
                state.GateOpened = true;
                persistence.Current.Keys.SetData(description.Zone, description.InstanceId, state);
                contextMenu.ClearContext(Open);
                RemoveGraphics();
                DestroyPhysics();
            }
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }
    }
}
