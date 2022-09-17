using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Adventure.Battle;
using Adventure.Services;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.CameraMovement;
using BepuPlugin;
using Adventure.Menu;

namespace Adventure.WorldMap
{
    interface IWorldMapGameState : IGameState
    {
        void Link(IExplorationGameState explorationState);
        void EnterZone(int zoneIndex);
    }

    class WorldMapGameState : IWorldMapGameState
    {
        private readonly RTInstances<IWorldMapGameState> rtInstances;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly Persistence persistence;
        private readonly IWorldMapManager worldMapManager;
        private readonly FlyCameraManager flyCameraManager;
        private readonly IBepuScene<IWorldMapGameState> bepuScene;
        private readonly IContextMenu contextMenu;
        private readonly IWorldDatabase worldDatabase;
        private readonly IExplorationMenu explorationMenu;
        private readonly EventManager eventManager;
        private readonly IBackgroundMusicPlayer backgroundMusicPlayer;
        private IExplorationGameState explorationState;
        private IGameState nextState;

        public RTInstances Instances => rtInstances;

        public WorldMapGameState
        (
            RTInstances<IWorldMapGameState> rtInstances,
            ICoroutineRunner coroutineRunner,
            IZoneManager zoneManager,
            Persistence persistence,
            IWorldMapManager worldMapManager,
            FlyCameraManager flyCameraManager,
            IBepuScene<IWorldMapGameState> bepuScene,
            IContextMenu contextMenu,
            IWorldDatabase worldDatabase,
            IExplorationMenu explorationMenu,
            EventManager eventManager,
            IBackgroundMusicPlayer backgroundMusicPlayer
        )
        {
            this.rtInstances = rtInstances;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.persistence = persistence;
            this.worldMapManager = worldMapManager;
            this.flyCameraManager = flyCameraManager;
            this.bepuScene = bepuScene;
            this.contextMenu = contextMenu;
            this.worldDatabase = worldDatabase;
            this.explorationMenu = explorationMenu;
            this.eventManager = eventManager;
            this.backgroundMusicPlayer = backgroundMusicPlayer;
        }

        public void Link(IExplorationGameState explorationState)
        {
            this.explorationState = explorationState;
        }

        public void SetActive(bool active)
        {
            persistence.Current.Player.InWorld = active;
            if (active)
            {
                eventManager[EventLayers.WorldMap].makeFocusLayer();
                nextState = this;
                persistence.Current.BattleTriggers.ClearData();
                if (persistence.Current.Player.WorldPosition == null)
                {
                    worldMapManager.MovePlayerToArea(persistence.Current.Player.LastArea);
                }
                worldMapManager.CenterCamera();
                backgroundMusicPlayer.SetBackgroundSong("Music/freepd/Kevin MacLeod - Witch Waltz.ogg");
            }
        }

        public void EnterZone(int zoneIndex)
        {
            persistence.Current.Player.Position = null;
            persistence.Current.Zone.CurrentIndex = zoneIndex;
            persistence.Current.Player.RespawnZone = zoneIndex;
            persistence.Current.Player.RespawnPosition = null;
            persistence.Current.Player.LastArea = worldDatabase.GetAreaBuilder(zoneIndex).Index;
            coroutineRunner.RunTask(zoneManager.Restart());
            nextState = this.explorationState;
        }

        public IGameState Update(Clock clock)
        {
            flyCameraManager.Update(clock);

            if (explorationMenu.Update(explorationState))
            {
                //If menu did something
            }
            else
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
}
