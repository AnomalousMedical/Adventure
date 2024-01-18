using Adventure.Battle;
using Adventure.GameOver;
using Adventure.Menu;
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
            ISetupGameState setup,
            ISetupRespawnGameState setupRespawnGameState, 
            IStartExplorationGameState startExplorationGameState,
            IExplorationMenu explorationMenu,
            IRootMenu rootMenu
        )
        {
            setup.Link(explorationMenu, rootMenu, exploration, worldMap, battle, gameOver);
            exploration.Link(battle, worldMap);
            battle.Link(exploration, gameOver);
            gameOver.Link(setupRespawnGameState);
            setupRespawnGameState.Link(exploration);
            worldMap.Link(exploration, startExplorationGameState);
            startExplorationGameState.Link(exploration);
        }
    }
}
