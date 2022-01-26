using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.GameOver
{
    interface IGameOverGameState : IGameState
    {
        void Link(IGameState explorationState);
    }
}