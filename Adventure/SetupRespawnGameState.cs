using Adventure.Menu;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;

namespace Adventure;

interface ISetupRespawnGameState : IGameState
{
    void Link(IGameState next);
}

class SetupRespawnGameState
(
    IZoneManager zoneManager,
    ICoroutineRunner coroutineRunner,
    RTInstances<EmptyScene> emptySceneInstances,
    RTInstances<ZoneScene> explorationSceneInstances,
    FadeScreenMenu fadeScreenMenu,
    IExplorationMenu explorationMenu
) : ISetupRespawnGameState
{
    private IGameState nextState;
    private bool finished = false;

    private RTInstances rtInstances = emptySceneInstances;
    public RTInstances Instances => rtInstances;

    public void Link(IGameState next)
    {
        this.nextState = next;
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            rtInstances = emptySceneInstances;
            finished = false;
            coroutineRunner.RunTask(async () =>
            {
                await zoneManager.WaitForCurrent();

                rtInstances = explorationSceneInstances;

                await fadeScreenMenu.ShowAndWaitAndClose(1.0f, 0.0f, 0.6f, GamepadId.Pad1);

                finished = true;
            });
        }
    }

    public IGameState Update(Clock clock)
    {
        explorationMenu.Update();

        IGameState next = this;

        if (finished)
        {
            next = this.nextState;
        }
        return next;
    }

}
