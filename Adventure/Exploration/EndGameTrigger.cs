using Adventure.Menu;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.ShaderSets;
using DiligentEngine.RT.Sprites;
using Engine;
using System;

namespace Adventure;

class EndGameTrigger : IDisposable, IZonePlaceable
{
    public class Description : SceneObjectDesc
    {
        public int ZoneIndex { get; set; }

        public Vector3 MapOffset { get; set; }

        public int RespawnBossIndex { get; set; }
    }

    private readonly RTInstances<ZoneScene> rtInstances;
    private readonly IDestructionRequest destructionRequest;
    private readonly SpriteInstanceFactory spriteInstanceFactory;
    private readonly IContextMenu contextMenu;
    private readonly IExplorationGameState explorationGameState;
    private readonly Persistence persistence;
    private readonly CubeBLAS cubeBLAS;
    private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
    private readonly TypedLightManager<ZoneScene> lightManager;
    private PrimaryHitShader primaryHitShader;
    private bool graphicsLoaded = false;
    private readonly TLASInstanceData tlasData;
    private readonly IBepuScene<ZoneScene> bepuScene;
    private readonly ICollidableTypeIdentifier<ZoneScene> collidableIdentifier;
    private readonly Vector3 mapOffset;
    private readonly int respawnBossIndex;
    private StaticHandle staticHandle;
    private TypedIndex shapeIndex;
    private bool physicsCreated = false;
    private bool graphicsCreated = false;
    private int zoneIndex;

    private Vector3 currentPosition;
    private Quaternion currentOrientation;
    private Vector3 currentScale;

    private BlasInstanceData blasInstanceData;

    private Light light;

    public EndGameTrigger
    (
        RTInstances<ZoneScene> rtInstances,
        IDestructionRequest destructionRequest,
        IScopedCoroutine coroutine,
        IBepuScene<ZoneScene> bepuScene,
        Description description,
        ICollidableTypeIdentifier<ZoneScene> collidableIdentifier,
        SpriteInstanceFactory spriteInstanceFactory,
        IContextMenu contextMenu,
        IExplorationGameState explorationGameState,
        Persistence persistence,
        CubeBLAS cubeBLAS,
        PrimaryHitShader.Factory primaryHitShaderFactory,
        TypedLightManager<ZoneScene> lightManager
    )
    {
        this.zoneIndex = description.ZoneIndex;
        this.rtInstances = rtInstances;
        this.destructionRequest = destructionRequest;
        this.bepuScene = bepuScene;
        this.collidableIdentifier = collidableIdentifier;
        this.spriteInstanceFactory = spriteInstanceFactory;
        this.contextMenu = contextMenu;
        this.explorationGameState = explorationGameState;
        this.persistence = persistence;
        this.cubeBLAS = cubeBLAS;
        this.primaryHitShaderFactory = primaryHitShaderFactory;
        this.lightManager = lightManager;
        this.mapOffset = description.MapOffset;
        this.respawnBossIndex = description.RespawnBossIndex;

        this.currentPosition = description.Translation;
        this.currentOrientation = description.Orientation * new Quaternion(3.14f / 4f, 3.14f / 4f, 3.14f / 4f);
        this.currentScale = new Vector3(2.0f, 2.0f, 2.0f);

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

            await cubeBLAS.WaitForLoad();

            this.primaryHitShader = await this.primaryHitShaderFactory.Checkout();

            blasInstanceData = GlassInstanceDataCreator.Create(new Vector3(0.93f, 0.22f, 0.83f), 0.5f, new Vector2(1.5f, 1.02f), new Color(0.93f, 0.33f, 0.29f));
            blasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, false, false, false, isGlass: true);
            tlasData.pBLAS = cubeBLAS.Instance.BLAS.Obj;

            light = new Light()
            {
                Color = Color.FromRGB(0xff0ee4),
                Length = 10.7f,
                Position = (currentPosition + new Vector3(0.0f, currentScale.y + 2f, 0.0f)).ToVector4()
            };

            this.graphicsLoaded = true;

            AddGraphics();
        });
    }

    public void Reset()
    {
        //Does nothing, no state to reset
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
        DestroyGraphics();
        DestroyPhysics();
        primaryHitShaderFactory.TryReturn(primaryHitShader);
    }

    private void AddGraphics()
    {
        if (!graphicsCreated)
        {
            rtInstances.AddTlasBuild(tlasData);
            rtInstances.AddShaderTableBinder(Bind);
            lightManager.AddLight(light);

            graphicsCreated = true;
        }
    }

    private void DestroyGraphics()
    {
        if (graphicsCreated)
        {
            lightManager.RemoveLight(light);
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
            contextMenu.HandleContext("End Game", EndGame, player.GamepadId);
        }
    }

    private void HandleCollisionEnd(CollisionEvent evt)
    {
        contextMenu.ClearContext(EndGame);
    }

    private void EndGame(ContextMenuArgs args)
    {
        contextMenu.ClearContext(EndGame);

        //Respawn final boss
        var bossState = persistence.Current.BossBattleTriggers.GetData(zoneIndex, 0);
        bossState.Dead = false;
        persistence.Current.BossBattleTriggers.SetData(zoneIndex, respawnBossIndex, bossState);

        //Reset gold piles, this is worldwide, but only used in final area
        persistence.Current.GoldPiles.ClearData();

        explorationGameState.RequestVictory();
    }

    private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
    {
        blasInstanceData.vertexOffset = cubeBLAS.Instance.VertexOffset;
        blasInstanceData.indexOffset = cubeBLAS.Instance.IndexOffset;
        fixed (BlasInstanceData* ptr = &blasInstanceData)
        {
            primaryHitShader.BindSbt(tlasData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
        }
    }
}
