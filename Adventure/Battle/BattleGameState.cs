using DiligentEngine.RT;
using Engine.Platform;
using Adventure.GameOver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface IBattleGameState : IGameState
    {
        /// <summary>
        /// This is a circular link, so it must be set by the ExplorationGameState itself, which injects this class.
        /// </summary>
        void Link(IGameState returnState, IGameState gameOver);
        void SetBattleTrigger(BattleTrigger battleTrigger);
    }

    class BattleGameState : IBattleGameState
    {
        private readonly IBattleManager battleManager;
        private readonly RTInstances<IBattleManager> rtInstances;
        private readonly Party party;
        private IGameState gameOverState;
        private IGameState returnState;
        private BattleTrigger battleTrigger;
        private Random noTriggerRandom = new Random();

        public BattleGameState
        (
            IBattleManager battleManager,
            RTInstances<IBattleManager> rtInstances,
            Party party
        )
        {
            this.battleManager = battleManager;
            this.rtInstances = rtInstances;
            this.party = party;
        }

        public RTInstances Instances => rtInstances;

        public void Link(IGameState returnState, IGameState gameOver)
        {
            this.returnState = returnState;
            this.gameOverState = gameOver;
        }

        public void SetBattleTrigger(BattleTrigger battleTrigger)
        {
            this.battleTrigger = battleTrigger;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                int battleSeed;
                int level;
                if(battleTrigger == null)
                {
                    level = party.ActiveCharacterSheets.GetAverageLevel() * 4 / 5;
                    if (level < 1)
                    {
                        level = 1;
                    }
                    battleSeed = noTriggerRandom.Next(int.MinValue, int.MaxValue);
                }
                else
                {
                    level = battleTrigger.EnemyLevel;
                    battleSeed = battleTrigger.BattleSeed;
                }

                battleManager.SetupBattle(battleSeed, level);
            }
            battleManager.SetActive(active);
        }

        public IGameState Update(Clock clock)
        {
            IGameState nextState = this;
            var result = battleManager.Update(clock);
            switch(result)
            {
                case IBattleManager.Result.GameOver:
                    nextState = gameOverState;
                    break;
                case IBattleManager.Result.ReturnToExploration:
                    nextState = returnState;
                    battleTrigger?.BattleWon();
                    break;
            }

            if(nextState != this)
            {
                battleTrigger = null;
            }

            return nextState;
        }
    }
}
