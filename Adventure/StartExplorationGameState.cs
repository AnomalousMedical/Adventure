using Adventure.Menu;
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
        private readonly IExplorationMenu explorationMenu;
        private IGameState nextState;
        private bool finished = false;

        public RTInstances Instances => rtInstances;

        public StartExplorationGameState
        (
            IZoneManager zoneManager,
            ICoroutineRunner coroutineRunner,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            RTInstances<EmptyScene> rtInstances,
            IExplorationMenu explorationMenu
        )
        {
            this.zoneManager = zoneManager;
            this.coroutineRunner = coroutineRunner;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.rtInstances = rtInstances;
            this.explorationMenu = explorationMenu;
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

            explorationMenu.Update();

            if (finished)
            {
                next = this.nextState;
            }
            return next;
        }
    }
}
