using Adventure.Assets.SoundEffects;
using Adventure.Menu;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;

namespace Adventure;

class TreasureTrigger : IDisposable, IZonePlaceable
{
    public class Description : SceneObjectDesc
    {
        public int ZoneIndex { get; set; }

        public int InstanceId { get; set; }

        public Vector3 MapOffset { get; set; }

        public ISprite Sprite { get; set; }

        public SpriteMaterialDescription SpriteMaterial { get; set; }

        public ITreasure Treasure { get; set; }
    }

    public record struct TreasureTriggerPersistenceData(bool Open);

    private readonly RTInstances<ZoneScene> rtInstances;
    private readonly IDestructionRequest destructionRequest;
    private readonly SpriteInstanceFactory spriteInstanceFactory;
    private readonly IContextMenu contextMenu;
    private readonly Persistence persistence;
    private readonly IExplorationMenu explorationMenu;
    private readonly TreasureMenu treasureMenu;
    private readonly ISoundEffectPlayer soundEffectPlayer;
    private SpriteInstance spriteInstance;
    private readonly ISprite sprite;
    private readonly TLASInstanceData tlasData;
    private readonly IBepuScene<ZoneScene> bepuScene;
    private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
    private readonly Vector3 mapOffset;
    private StaticHandle staticHandle;
    private TypedIndex shapeIndex;
    private bool physicsCreated = false;
    private int zoneIndex;
    private int instanceId;
    private TreasureTriggerPersistenceData state;
    private List<ITreasure> treasure = new List<ITreasure>();

    private Vector3 currentPosition;
    private Quaternion currentOrientation;
    private Vector3 currentScale;

    public TreasureTrigger(
        RTInstances<ZoneScene> rtInstances,
        IDestructionRequest destructionRequest,
        IScopedCoroutine coroutine,
        IBepuScene<ZoneScene> bepuScene,
        Description description,
        ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
        SpriteInstanceFactory spriteInstanceFactory,
        IContextMenu contextMenu,
        Persistence persistence,
        IExplorationMenu explorationMenu, 
        TreasureMenu treasureMenu,
        ISoundEffectPlayer soundEffectPlayer)
    {
        this.sprite = description.Sprite;
        this.zoneIndex = description.ZoneIndex;
        this.instanceId = description.InstanceId;
        this.state = persistence.Current.TreasureTriggers.GetData(zoneIndex, instanceId);
        this.rtInstances = rtInstances;
        this.destructionRequest = destructionRequest;
        this.bepuScene = bepuScene;
        this.collidableIdentifier = collidableIdentifier;
        this.spriteInstanceFactory = spriteInstanceFactory;
        this.contextMenu = contextMenu;
        this.persistence = persistence;
        this.explorationMenu = explorationMenu;
        this.treasureMenu = treasureMenu;
        this.soundEffectPlayer = soundEffectPlayer;
        this.mapOffset = description.MapOffset;
        this.treasure.Add(description.Treasure);

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

            this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);

            rtInstances.AddTlasBuild(tlasData);
            rtInstances.AddShaderTableBinder(Bind);
            rtInstances.AddSprite(sprite, tlasData, spriteInstance);

            if (state.Open)
            {
                sprite.SetAnimation("open");
            }
        });
    }

    public void Reset()
    {
        this.state = persistence.Current.TreasureTriggers.GetData(zoneIndex, instanceId);
        if (state.Open)
        {
            sprite.SetAnimation("open");
        }
        else
        {
            sprite.SetAnimation("closed");
        }
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
        spriteInstanceFactory.TryReturn(spriteInstance);
        rtInstances.RemoveSprite(sprite);
        rtInstances.RemoveShaderTableBinder(Bind);
        rtInstances.RemoveTlasBuild(tlasData);
        DestroyPhysics();
    }

    public void AddTreasure(ITreasure item)
    {
        this.treasure.Add(item);
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
            if (!state.Open)
            {
                contextMenu.HandleContext("Open", Open, player.GamepadId);
            }
        }
    }

    private void HandleCollisionEnd(CollisionEvent evt)
    {
        contextMenu.ClearContext(Open);
    }

    private void Open(ContextMenuArgs args)
    {
        contextMenu.ClearContext(Open);
        sprite.SetAnimation("open");
        //If something were to go wrong handing out treasure it would be lost, but the
        //other option opens it up to duplication
        state.Open = true;
        persistence.Current.TreasureTriggers.SetData(zoneIndex, instanceId, state);
        treasureMenu.GatherTreasures(treasure);
        explorationMenu.RequestSubMenu(treasureMenu, args.GamepadId);
        soundEffectPlayer.PlaySound(OpenChestSoundEffect.Instance);
    }

    private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
    {
        spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
    }
}
