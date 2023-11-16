using Adventure.Services;
using Adventure.WorldMap;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using SharpGui;

namespace Adventure
{
    interface IStartExplorationGameState : IGameState
    {
        void Link(IGameState next);
    }

    class StartExplorationGameState : IStartExplorationGameState
    {
        private readonly IZoneManager zoneManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private RTInstances rtInstances;
        private IGameState nextState;
        private bool finished = false;

        public RTInstances Instances => rtInstances;

        private SharpText loading = new SharpText("Loading") { Color = Color.White };

        public StartExplorationGameState
        (
            IZoneManager zoneManager,
            ICoroutineRunner coroutineRunner,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            RTInstances<EmptyScene> rtInstances
        )
        {
            this.zoneManager = zoneManager;
            this.coroutineRunner = coroutineRunner;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.rtInstances = rtInstances;
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
                    await zoneManager.WaitForCurrent();
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
