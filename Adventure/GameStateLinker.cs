using Adventure.Battle;
using Adventure.GameOver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class GameStateLinker : IGameStateLinker
    {
        public GameStateLinker
        (
            IExplorationGameState exploration,
            IBattleGameState battle,
            IGameOverGameState gameOver,
            ISetupGameState setup
        )
        {
            setup.Link(exploration);
            exploration.Link(battle);
            battle.Link(exploration, gameOver);
            gameOver.Link(setup);
        }
    }
}
