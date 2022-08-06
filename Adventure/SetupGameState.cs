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
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly RTInstances rtInstances;
        private readonly Persistence persistence;
        private IGameState nextState;
        private bool finished = false;

        public RTInstances Instances => rtInstances;

        private SharpText loading = new SharpText("Loading") { Color = Color.White };

        public SetupGameState
        (
            IZoneManager zoneManager,
            ICoroutineRunner coroutineRunner,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            RTInstances<IZoneManager> rtInstances,
            Persistence persistence
        )
        {
            this.zoneManager = zoneManager;
            this.coroutineRunner = coroutineRunner;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.rtInstances = rtInstances;
            this.persistence = persistence;
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
