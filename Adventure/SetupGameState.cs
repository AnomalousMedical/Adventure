using Adventure.Services;
using Adventure.WorldMap;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using SharpGui;

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
        private readonly RTInstances<IZoneManager> zoneInstances;
        private readonly RTInstances<IWorldMapGameState> worldInstances;
        private RTInstances rtInstances;
        private readonly Persistence persistence;
        private readonly RayTracingRenderer rayTracingRenderer;
        private readonly IPersistenceWriter persistenceWriter;
        private readonly IWorldDatabase worldDatabase;
        private readonly ITimeClock timeClock;
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
            RTInstances<IZoneManager> zoneInstances,
            RTInstances<IWorldMapGameState> worldInstances,
            Persistence persistence,
            RayTracingRenderer rayTracingRenderer,
            IPersistenceWriter persistenceWriter,
            IWorldDatabase worldDatabase,
            ITimeClock timeClock
        )
        {
            this.zoneManager = zoneManager;
            this.worldMapManager = worldMapManager;
            this.coroutineRunner = coroutineRunner;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.zoneInstances = zoneInstances;
            this.worldInstances = worldInstances;
            this.persistence = persistence;
            this.rayTracingRenderer = rayTracingRenderer;
            this.persistenceWriter = persistenceWriter;
            this.worldDatabase = worldDatabase;
            this.timeClock = timeClock;
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

                this.persistenceWriter.Load();
                this.worldDatabase.Reset(persistence.Current.World.Seed);
                timeClock.ResetToPersistedTime();
                var mapLoadTask = worldMapManager.SetupWorldMap(); //Task only needs await if world is loading

                coroutineRunner.RunTask(async () =>
                {
                    if (persistence.Current.Player.InWorld)
                    {
                        this.nextState = worldMapGameState;
                        rtInstances = worldInstances;
                        await mapLoadTask;
                    }
                    else
                    {
                        this.nextState = explorationGameState;
                        rtInstances = zoneInstances;
                        await zoneManager.Restart(lastSeed == persistence.Current.World.Seed); //When restarting if the world seed is the same allow hold zones
                        await zoneManager.WaitForCurrent();
                        await zoneManager.WaitForPrevious();
                        await zoneManager.WaitForNext();
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
