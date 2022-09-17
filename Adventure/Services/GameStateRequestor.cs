using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface IGameStateRequestor
    {
        IGameState GetRequestedGameState();
        void RequestGameState(IGameState gameState);
    }

    class GameStateRequestor : IGameStateRequestor
    {
        private IGameState gameState;

        public void RequestGameState(IGameState gameState)
        {
            this.gameState = gameState;
        }

        public IGameState GetRequestedGameState()
        {
            var ret = this.gameState;
            gameState = null;
            return ret;
        }
    }
}
