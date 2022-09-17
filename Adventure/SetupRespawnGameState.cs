using Adventure.Services;
using Adventure.WorldMap;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using SharpGui;

namespace Adventure
{
    interface ISetupRespawnGameState : IGameState
    {
        void Link(IGameState next);
    }

    class SetupRespawnGameState : ISetupRespawnGameState
    {
        private readonly IZoneManager zoneManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly RTInstances<ZoneScene> zoneInstances;
        private RTInstances rtInstances;
        private readonly RayTracingRenderer rayTracingRenderer;
        private IGameState nextState;
        private bool finished = false;

        public RTInstances Instances => rtInstances;

        private SharpText loading = new SharpText("Loading") { Color = Color.White };

        public SetupRespawnGameState
        (
            IZoneManager zoneManager,
            ICoroutineRunner coroutineRunner,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            RTInstances<ZoneScene> zoneInstances,
            RayTracingRenderer rayTracingRenderer
        )
        {
            this.zoneManager = zoneManager;
            this.coroutineRunner = coroutineRunner;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.zoneInstances = zoneInstances;
            this.rayTracingRenderer = rayTracingRenderer;
        }

        public void Link(IGameState next)
        {
            this.nextState = next;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                finished = false;
                coroutineRunner.RunTask(async () =>
                {
                    rtInstances = zoneInstances;
                    await zoneManager.WaitForCurrent();
                    await zoneManager.WaitForPrevious();
                    await zoneManager.WaitForNext();
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
