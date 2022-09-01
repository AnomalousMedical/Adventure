using Adventure.Battle;
using Adventure.Exploration.Menu;
using Adventure.Services;
using Adventure.WorldMap;
using BepuPlugin;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using RpgMath;
using System;

namespace Adventure
{
    interface IExplorationGameState : IGameState
    {
        bool AllowBattles { get; set; }

        void Link(IBattleGameState battleState, IWorldMapGameState worldMapState);

        /// <summary>
        /// Request a battle with a given trigger. The trigger can be null.
        /// </summary>
        /// <param name="battleTrigger"></param>
        void RequestBattle(BattleTrigger battleTrigger = null);
        void SetExplorationEvent(Func<Clock, bool> explorationEvent);
        void RequestWorldMap();
        void LevelUpWorld();
    }

    class ExplorationGameState : IExplorationGameState, IDisposable
    {
        private readonly IBepuScene<IExplorationGameState> bepuScene;
        private readonly IZoneManager zoneManager;
        private readonly RTInstances<IZoneManager> rtInstances;
        private readonly IExplorationMenu explorationMenu;
        private readonly IContextMenu contextMenu;
        private readonly EventManager eventManager;
        private readonly IBackgroundMusicPlayer backgroundMusicPlayer;
        private readonly ITimeClock timeClock;
        private readonly Persistence persistence;
        private readonly IWorldDatabase worldDatabase;
        private readonly ILevelCalculator levelCalculator;
        private IBattleGameState battleState;
        private IWorldMapGameState worldMapState;
        private IGameState nextState; //This is changed per update to be the next game state
        private Func<Clock, bool> explorationEvent;

        public RTInstances Instances => rtInstances;

        public ExplorationGameState
        (
            ICoroutineRunner coroutineRunner,
            IBepuScene<IExplorationGameState> bepuScene,
            IZoneManager zoneManager,
            RTInstances<IZoneManager> rtInstances,
            IExplorationMenu explorationMenu,
            IContextMenu contextMenu,
            EventManager eventManager,
            IBackgroundMusicPlayer backgroundMusicPlayer,
            ITimeClock timeClock,
            Persistence persistence,
            IWorldDatabase worldDatabase,
            ILevelCalculator levelCalculator
        )
        {
            this.bepuScene = bepuScene;
            this.zoneManager = zoneManager;
            this.rtInstances = rtInstances;
            this.explorationMenu = explorationMenu;
            this.contextMenu = contextMenu;
            this.eventManager = eventManager;
            this.backgroundMusicPlayer = backgroundMusicPlayer;
            this.timeClock = timeClock;
            this.persistence = persistence;
            this.worldDatabase = worldDatabase;
            this.levelCalculator = levelCalculator;
            if (!persistence.Current.Player.InWorld)
            {
                coroutineRunner.RunTask(zoneManager.Restart());
            }
        }

        public void Dispose()
        {
            zoneManager.ZoneChanged -= ZoneManager_ZoneChanged;
            timeClock.DayStarted -= TimeClock_DayStarted;
            timeClock.NightStarted -= TimeClock_NightStarted;
        }

        public void Link(IBattleGameState battleState, IWorldMapGameState worldMapState)
        {
            this.battleState = battleState;
            this.worldMapState = worldMapState;
        }

        public void SetActive(bool active)
        {
            //Stopping them both directions
            zoneManager.StopPlayer();
            if (active)
            {
                eventManager[EventLayers.Exploration].makeFocusLayer();
                zoneManager.ZoneChanged += ZoneManager_ZoneChanged;
                timeClock.DayStarted += TimeClock_DayStarted;
                timeClock.NightStarted += TimeClock_NightStarted;
                ZoneManager_ZoneChanged(null);
                zoneManager.CenterCamera();
            }
            else
            {
                zoneManager.ZoneChanged -= ZoneManager_ZoneChanged;
                timeClock.DayStarted -= TimeClock_DayStarted;
                timeClock.NightStarted -= TimeClock_NightStarted;
            }
        }

        public bool AllowBattles { get; set; } = true;

        public void RequestBattle(BattleTrigger battleTrigger)
        {
            if (AllowBattles)
            {
                battleState.SetBattleTrigger(battleTrigger);
                nextState = battleState;
            }
            else
            {
                battleTrigger?.BattleWon();
            }
        }

        public void RequestWorldMap()
        {
            nextState = worldMapState;
        }

        public void SetExplorationEvent(Func<Clock, bool> explorationEvent)
        {
            this.explorationEvent = explorationEvent;
        }

        public IGameState Update(Clock clock)
        {
            nextState = this;

            if(explorationEvent != null)
            {
                if (!explorationEvent.Invoke(clock))
                {
                    explorationEvent = null;
                }
            }
            else if (explorationMenu.Update(this))
            {
                //If menu did something
            }
            else
            {
                if (zoneManager.Current?.PhysicsActive == true)
                {
                    bepuScene.Update(clock, new System.Numerics.Vector3(0, 0, 1));
                    zoneManager.Update();
                }
                contextMenu.Update();
            }

            return nextState;
        }

        public void LevelUpWorld()
        {
            var current = persistence.Current.World.Level;
            var levelDelta = worldDatabase.GetLevelDelta(current);
            current += levelDelta;
            persistence.Current.World.Level = current;
            foreach (var sheet in persistence.Current.Party.Members)
            {
                while (sheet.CharacterSheet.Level < current)
                {
                    //TODO: Change how characters level, for now just use "fighter"
                    sheet.CharacterSheet.LevelUp(levelCalculator);
                }
            }
        }

        private void TimeClock_NightStarted(TimeClock obj)
        {
            var song = zoneManager.Current.Biome.BgMusicNight;
            backgroundMusicPlayer.SetBackgroundSong(song);
        }

        private void TimeClock_DayStarted(TimeClock obj)
        {
            var song = zoneManager.Current.Biome.BgMusic;
            backgroundMusicPlayer.SetBackgroundSong(song);
        }

        private void ZoneManager_ZoneChanged(IZoneManager obj)
        {
            var song = zoneManager.Current.Biome.BgMusic;
            if (!timeClock.IsDay)
            {
                song = zoneManager.Current.Biome.BgMusicNight;
            }
            backgroundMusicPlayer.SetBackgroundSong(song);
        }
    }
}
