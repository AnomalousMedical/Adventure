using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
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
    class Torch : IDisposable, IZonePlaceable
    {
        public class Description : SceneObjectDesc
        {
            public int ZoneIndex { get; set; }

            public int InstanceId { get; set; }

            public Vector3 MapOffset { get; set; }

            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }
        }

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly IScopedCoroutine coroutine;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly IExplorationGameState explorationGameState;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly Persistence persistence;
        private readonly TypedLightManager<ZoneScene> lightManager;
        private readonly TextDialog textDialog;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly Vector3 mapOffset;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private int zoneIndex;
        private int instanceId;
        private TorchPersistenceData state;
        private Light light;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public record struct TorchPersistenceData(bool Lit);

        public Torch
        (
            RTInstances<ZoneScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<ZoneScene> bepuScene,
            Description description,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            IExplorationGameState explorationGameState,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
            Persistence persistence,
            TypedLightManager<ZoneScene> lightManager,
            TextDialog textDialog
        )
        {
            this.sprite = description.Sprite;
            this.zoneIndex = description.ZoneIndex;
            this.instanceId = description.InstanceId;
            this.state = persistence.Current.Torches.GetData(zoneIndex, instanceId);
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.coroutine = coroutine;
            this.bepuScene = bepuScene;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.explorationGameState = explorationGameState;
            this.collidableIdentifier = collidableIdentifier;
            this.persistence = persistence;
            this.lightManager = lightManager;
            this.textDialog = textDialog;
            this.mapOffset = description.MapOffset;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            this.light = new Light()
            {
                Color = Color.FromARGB(0xffce7f18),
                Length = 2.3f,
            };

            sprite.FrameChanged += Sprite_FrameChanged;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Torch"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(finalPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);

                if (state.Lit)
                {
                    LightTorch();
                }

                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);
            });
        }

        public void Reset()
        {
            //Nothing to reset
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
            if (state.Lit)
            {
                lightManager.RemoveLight(light);
            }
            sprite.FrameChanged -= Sprite_FrameChanged;
            spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            DestroyPhysics();
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
            if (!state.Lit
             && collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.A, out var player)
             || collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.B, out player))
            {
                contextMenu.HandleContext("Light", Light, player.GamepadId);
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Light);
        }

        private void Light(ContextMenuArgs args)
        {
            contextMenu.ClearContext(Light);
            if (!state.Lit)
            {
                state.Lit = true;
                persistence.Current.Torches.SetData(zoneIndex, instanceId, state);
                LightTorch();
                coroutine.RunTask(async () =>
                {
                    switch (persistence.Current.Torches.Entries.Count)
                    {
                        case 1:
                            await textDialog.ShowTextAndWait("A voice whispers in your ear:\n\"Find the two that remain...\"", args.GamepadId);
                            break;
                        case 2:
                            await textDialog.ShowTextAndWait("A voice whispers in your ear:\n\"Only one left now...\"", args.GamepadId);
                            break;
                        default:
                        case 3:
                            await textDialog.ShowTextAndWait("A voice whispers in your ear:\n\"All have been lit. Your prize is in the flames...\"", args.GamepadId);
                            await textDialog.ShowTextAndWait("You reach out and oddly the flames feel cool on your fingers. You reach into the flame and pull out the rune of fire.", args.GamepadId);
                            persistence.Current.PlotItems.Add(PlotItems.RuneOfFire);
                            break;
                    }
                });
            }
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }

        private void LightTorch()
        {
            sprite.SetAnimation("lit");
            lightManager.AddLight(light);
            Sprite_FrameChanged(null);
        }

        private void Sprite_FrameChanged(ISprite obj)
        {
            light.Position = (sprite.GetCurrentFrame().Attachments[0].translate + currentPosition).ToVector4();
        }
    }
}
