using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
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
