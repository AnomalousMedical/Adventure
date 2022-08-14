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
        private IGameState nextState;
        private bool finished = false;

        public RTInstances Instances => rtInstances;

        private SharpText loading = new SharpText("Loading") { Color = Color.White };

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
            RayTracingRenderer rayTracingRenderer
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
        }

        public void Link(IExplorationGameState explorationGameState, IWorldMapGameState worldMapGameState)
        {
            if (persistence.Current.Player.InWorld)
            {
                this.nextState = worldMapGameState;
            }
            else
            {
                this.nextState = explorationGameState;
            }
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                finished = false;
                coroutineRunner.RunTask(async () =>
                {
                    if (persistence.Current.Player.InWorld)
                    {
                        rtInstances = worldInstances;
                        await worldMapManager.WaitForWorldMapLoad();
                    }
                    else
                    {
                        rtInstances = zoneInstances;
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
