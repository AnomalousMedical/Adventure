using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface IFirstGameStateBuilder
    {
        IGameState GetFirstGameState();
    }

    class FirstGameStateBuilder : IFirstGameStateBuilder
    {
        private readonly IGameState gameState;

        public FirstGameStateBuilder(ISetupGameState gameState)
        {
            this.gameState = gameState;
        }

        public IGameState GetFirstGameState()
        {
            return gameState;
        }
    }
}
