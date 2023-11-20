using Adventure.Services;
using Adventure.WorldMap;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System.Linq;

namespace Adventure
{
    interface ISetupGameState : IGameState
    {
        void Link(IExplorationGameState explorationGameState, IWorldMapGameState worldMapGameState);
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
        private IGameState nextState;
        private bool finished = false;

        public RTInstances Instances => rtInstances;

        private SharpText loading = new SharpText("Loading") { Color = Color.White };
        private IExplorationGameState explorationGameState;
        private IWorldMapGameState worldMapGameState;

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
            PartyMemberManager partyMemberManager
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
        }

        public void Link(IExplorationGameState explorationGameState, IWorldMapGameState worldMapGameState)
        {
            this.explorationGameState = explorationGameState;
            this.worldMapGameState = worldMapGameState;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                finished = false;

                var lastSeed = persistence.Current?.World.Seed;

                zoneManager.DestroyPlayers();
                this.persistenceWriter.Load();
                this.worldDatabase.Reset(persistence.Current.World.Seed);
                if(persistence.Current.Party.Members.Count == 0)
                {
                    var partyMember = this.worldDatabase.CreateParty().First();
                    partyMemberManager.AddToParty(partyMember);
                }
                timeClock.ResetToPersistedTime();
                var mapLoadTask = worldMapManager.SetupWorldMap(); //Task only needs await if world is loading

                coroutineRunner.RunTask(async () =>
                {
                    if (persistence.Current.Player.InWorld)
                    {
                        this.nextState = worldMapGameState;
                        await mapLoadTask;
                        rtInstances = worldInstances;
                    }
                    else
                    {
                        this.nextState = explorationGameState;
                        await zoneManager.Restart(lastSeed == persistence.Current.World.Seed); //When restarting if the world seed is the same allow hold zones
                        await zoneManager.WaitForCurrent();
                        rtInstances = zoneInstances;
                    }

                    await rayTracingRenderer.WaitForPipelineRebuild();
                    finished = true;
                });
            }
        }

        public IGameState Update(Clock clock)
        {
            IGameState next = this;

            var size = loading.GetDesiredSize(sharpGui);
            var rect = screenPositioner.GetCenterRect(size);
            loading.SetRect(rect);

            sharpGui.Text(loading);

            if (finished)
            {
                next = this.nextState;
            }
            return next;
        }
    }
}
