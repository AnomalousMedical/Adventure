using Adventure.Battle;
using Adventure.GameOver;
using Adventure.WorldMap;
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
            IWorldMapGameState worldMap,
            ISetupGameState setup
        )
        {
            setup.Link(exploration);
            exploration.Link(battle, worldMap);
            battle.Link(exploration, gameOver);
            gameOver.Link(setup);
            worldMap.Link(exploration);
        }
    }
}
