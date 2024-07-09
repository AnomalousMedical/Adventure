using Adventure.Assets.Music;
using Adventure.Menu;
using Adventure.Services;
using BepuPlugin;
using DiligentEngine.RT;
using Engine.Platform;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.WorldMap;

interface IWorldMapGameState : IGameState
{
    void ChangeToExplorationGameState();
    void Link(IGameState startExplorationGameState);
    Task SetupZone(Func<Task> waitForMainThreadWorkCb, int zoneIndex);
}

class WorldMapGameState
(
    RTInstances<WorldMapScene> rtInstances,
    IZoneManager zoneManager,
    Persistence persistence,
    IWorldMapManager worldMapManager,
    FlyCameraManager flyCameraManager,
    IBepuScene<WorldMapScene> bepuScene,
    IContextMenu contextMenu,
    IWorldDatabase worldDatabase,
    IExplorationMenu explorationMenu,
    EventManager eventManager,
    IBackgroundMusicPlayer backgroundMusicPlayer,
    RestManager restManager,
    BuffManager buffManager,
    TypedLightManager<WorldMapScene> typedLightManager,
    CharacterMenuPositionService characterMenuPositionService,
    IAnimationService<WorldMapScene> animationService
) : IWorldMapGameState
{
    private IGameState startExplorationGameState;
    private IGameState nextState;

    public RTInstances Instances => rtInstances;

    public void Link(IGameState startExplorationGameState)
    {
        this.startExplorationGameState = startExplorationGameState;
    }

    public void SetActive(bool active)
    {
        persistence.Current.Player.InWorld = active;
        if (active)
        {
            characterMenuPositionService.SetTrackerActive(typeof(WorldMapScene));
            eventManager[EventLayers.WorldMap].makeFocusLayer();
            nextState = this;
            persistence.Current.BattleTriggers.ClearData();
            if (persistence.Current.Player.WorldPosition.All(i => i == null))
            {
                worldMapManager.MovePlayerToArea(persistence.Current.Player.LastArea);
            }
            worldMapManager.CenterCamera();
            backgroundMusicPlayer.SetBackgroundSong(persistence.Current.Player.InAirship ? AirshipMusic.File : WorldMapMusic.File);
            worldMapManager.AllowBackgroundMusicChange = true;
        }
        typedLightManager.SetActive(active);
    }

    public async Task SetupZone(Func<Task> waitForMainThreadWorkCb, int zoneIndex)
    {
        persistence.Current.Player.Position[0] = null;
        persistence.Current.Player.Position[1] = null;
        persistence.Current.Player.Position[2] = null;
        persistence.Current.Player.Position[3] = null;
        persistence.Current.Zone.CurrentIndex = zoneIndex;
        persistence.Current.Player.RespawnZone = zoneIndex;
        persistence.Current.Player.LastArea = worldDatabase.GetAreaBuilder(zoneIndex).Index;
        await zoneManager.Restart(waitForMainThreadWorkCb);
    }

    public void ChangeToExplorationGameState()
    {
        worldMapManager.MovePlayerToArea(persistence.Current.Player.LastArea);
        worldMapManager.MakePlayerIdle();
        nextState = this.startExplorationGameState;
    }

    public IGameState Update(Clock clock)
    {
        flyCameraManager.Update(clock);
        buffManager.Update(clock);
        restManager.Update(clock);
        animationService.Update(clock);

        if (!restManager.Active && !explorationMenu.Update())
        {
            if (worldMapManager.PhysicsActive)
            {
                bepuScene.Update(clock, new System.Numerics.Vector3(0, 0, 1));
            }
            worldMapManager.Update(clock);
            contextMenu.Update();
        }

        return nextState;
    }
}
