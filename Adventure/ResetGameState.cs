using Adventure.Menu;
using DiligentEngine.RT;
using Engine.Platform;

namespace Adventure;

interface IResetGameState : IGameState
{
    void Link(ISetupGameState setupGameState, IExplorationMenu explorationMenu);
}

class ResetGameState
(
    RTInstances<EmptyScene> rtInstances
)
    : IResetGameState
{
    ISetupGameState setupGameState;
    IExplorationMenu explorationMenu;

    public RTInstances Instances => rtInstances;

    public void Link(ISetupGameState setupGameState, IExplorationMenu explorationMenu)
    {
        this.setupGameState = setupGameState;
        this.explorationMenu = explorationMenu;
    }

    public void SetActive(bool active)
    {
        
    }

    public IGameState Update(Clock clock)
    {
        explorationMenu.Update();

        return setupGameState;
    }
}
