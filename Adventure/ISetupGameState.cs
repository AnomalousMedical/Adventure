namespace Adventure
{
    interface ISetupGameState : IGameState
    {
        void Link(IGameState nextState);
    }
}