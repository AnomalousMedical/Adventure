using Adventure.Battle;
using Adventure.GameOver;
using Adventure.Menu;
using Adventure.Services;
using Adventure.WorldMap;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure
{
    interface ISetupGameState : IGameState
    {
        void Link(IExplorationMenu explorationMenu, IRootMenu rootMenu, IExplorationGameState explorationGameState, IWorldMapGameState worldMapGameState, IBattleGameState battleGameState, IGameOverGameState gameOverGameState);
    }

    class SetupGameState : ISetupGameState
    {
        private readonly IZoneManager zoneManager;
        private readonly IWorldMapManager worldMapManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly RTInstances<ZoneScene> zoneInstances;
        private readonly RTInstances<WorldMapScene> worldInstances;
        private RTInstances rtInstances;
        private readonly Persistence persistence;
        private readonly RayTracingRenderer rayTracingRenderer;
        private readonly IPersistenceWriter persistenceWriter;
        private readonly IWorldDatabase worldDatabase;
        private readonly ITimeClock timeClock;
        private readonly PartyMemberManager partyMemberManager;
        private readonly ChooseCharacterMenu chooseCharacterMenu;
        private readonly FadeScreenMenu fadeScreenMenu;
        private IGameState nextState;
        private bool showLogo = true;

        public RTInstances Instances => rtInstances;

        private SharpText loading = new SharpText("Anomalous Adventure") { Color = Color.White };
        private IExplorationGameState explorationGameState;
        private IWorldMapGameState worldMapGameState;
        private IBattleGameState battleGameState;
        private IGameOverGameState gameOverGameState;
        private IExplorationMenu explorationMenu;
        private IRootMenu rootMenu;

        public SetupGameState
        (
            IZoneManager zoneManager,
            IWorldMapManager worldMapManager,
            ICoroutineRunner coroutineRunner,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            RTInstances<ZoneScene> zoneInstances,
            RTInstances<WorldMapScene> worldInstances,
            RTInstances<EmptyScene> emptySceneInstances,
            Persistence persistence,
            RayTracingRenderer rayTracingRenderer,
            IPersistenceWriter persistenceWriter,
            IWorldDatabase worldDatabase,
            ITimeClock timeClock,
            PartyMemberManager partyMemberManager,
            ChooseCharacterMenu chooseCharacterMenu,
            FontLoader fontLoader,
            FadeScreenMenu fadeScreenMenu
        )
        {
            this.zoneManager = zoneManager;
            this.worldMapManager = worldMapManager;
            this.coroutineRunner = coroutineRunner;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.zoneInstances = zoneInstances;
            this.worldInstances = worldInstances;
            this.rtInstances = emptySceneInstances;
            this.persistence = persistence;
            this.rayTracingRenderer = rayTracingRenderer;
            this.persistenceWriter = persistenceWriter;
            this.worldDatabase = worldDatabase;
            this.timeClock = timeClock;
            this.partyMemberManager = partyMemberManager;
            this.chooseCharacterMenu = chooseCharacterMenu;
            this.fadeScreenMenu = fadeScreenMenu;
            this.loading.Font = fontLoader.TitleFont;
        }

        public void Link(IExplorationMenu explorationMenu, IRootMenu rootMenu, IExplorationGameState explorationGameState, IWorldMapGameState worldMapGameState, IBattleGameState battleGameState, IGameOverGameState gameOverGameState)
        {
            this.rootMenu = rootMenu;
            this.explorationMenu = explorationMenu;
            this.explorationGameState = explorationGameState;
            this.worldMapGameState = worldMapGameState;
            this.battleGameState = battleGameState;
            this.gameOverGameState = gameOverGameState;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                nextState = null;
                showLogo = true;

                var lastSeed = persistence.Current?.World.Seed;

                zoneManager.DestroyPlayers();
                this.persistenceWriter.Load();
                this.worldDatabase.Reset(persistence.Current.World.Seed);

                timeClock.ResetToPersistedTime();

                var mapLoadTask = worldMapManager.SetupWorldMap(); //Task only needs await if world is loading
                coroutineRunner.RunTask(async () =>
                {
                    if (persistence.Current.Player.InWorld)
                    {
                        await mapLoadTask;
                        rtInstances = worldInstances;
                        await fadeScreenMenu.ShowAndWaitAndClose(1.0f, 0.0f, 0.6f, GamepadId.Pad1);
                        this.nextState = worldMapGameState;
                        showLogo = false;
                        //No world battles
                    }
                    else
                    {
                        await zoneManager.Restart(() => Task.CompletedTask, lastSeed == persistence.Current.World.Seed); //When restarting if the world seed is the same allow hold zones
                        await zoneManager.WaitForCurrent();
                        if (persistence.Current.Party.GameOver)
                        {
                            this.nextState = gameOverGameState;
                            showLogo = false;
                        }
                        else
                        {
                            rtInstances = zoneInstances;
                            if (persistence.Current.Player.InBattle)
                            {
                                fadeScreenMenu.Show(1.0f, 0.0f, 0.6f, GamepadId.Pad1, rootMenu);
                                await rootMenu.WaitForClose();
                                await fadeScreenMenu.ShowAndWait(0.0f, 1.0f, 0.6f, GamepadId.Pad1);
                                var battleTrigger = await zoneManager.FindTrigger(persistence.Current.Player.LastBattleIndex, persistence.Current.Player.LastBattleIsBoss);
                                battleGameState.SetBattleTrigger(battleTrigger);
                                this.nextState = battleGameState;
                                showLogo = false;

                                battleGameState.ShowExplorationMenu = true;
                                await fadeScreenMenu.ShowAndWait(1.0f, 0.0f, 0.6f, GamepadId.Pad1);
                                fadeScreenMenu.Close();
                                battleGameState.ShowExplorationMenu = false;
                            }
                            else if (persistence.Current.Party.Members.Count == 0)
                            {
                                chooseCharacterMenu.Reset();
                                chooseCharacterMenu.MoveCameraToCurrentTrigger();
                                fadeScreenMenu.Show(1.0f, 0.0f, 0.6f, GamepadId.Pad1, chooseCharacterMenu);
                                this.nextState = explorationGameState;
                                showLogo = false;
                            }
                            else
                            {
                                await fadeScreenMenu.ShowAndWaitAndClose(1.0f, 0.0f, 0.6f, GamepadId.Pad1);
                                this.nextState = explorationGameState;
                                showLogo = false;
                            }
                        }
                    }
                });
            }
        }

        public IGameState Update(Clock clock)
        {
            IGameState next = this;

            if (explorationMenu.Update())
            {
                if(nextState != null)
                {
                    next = this.nextState;
                }
            }
            else
            {
                if (nextState != null)
                {
                    next = this.nextState;
                }
                
                if(showLogo)
                {
                    var size = loading.GetDesiredSize(sharpGui);
                    var rect = screenPositioner.GetCenterRect(size);
                    loading.SetRect(rect);

                    sharpGui.Text(loading);
                }
            }
            return next;
        }
    }
}
