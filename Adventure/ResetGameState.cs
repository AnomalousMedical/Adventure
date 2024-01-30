using DiligentEngine.RT;
using Engine.Platform;

namespace Adventure;

interface IResetGameState : IGameState
{
    void Link(ISetupGameState setupGameState);
}

class ResetGameState(RTInstances<EmptyScene> rtInstances)
    : IResetGameState
{
    ISetupGameState setupGameState;

    public RTInstances Instances => rtInstances;

    public void Link(ISetupGameState setupGameState)
    {
        this.setupGameState = setupGameState;
    }

    public void SetActive(bool active)
    {
        
    }

    public IGameState Update(Clock clock)
    {
        return setupGameState;
    }
}
