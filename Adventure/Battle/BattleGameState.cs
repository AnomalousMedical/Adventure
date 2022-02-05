﻿using DiligentEngine.RT;
using Engine.Platform;
using Adventure.GameOver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    class BattleGameState : IBattleGameState
    {
        private readonly IBattleManager battleManager;
        private readonly RTInstances<IBattleManager> rtInstances;
        private IGameState gameOverState;
        private IGameState returnState;
        private BattleTrigger battleTrigger;
        private Random noTriggerRandom = new Random();

        public BattleGameState
        (
            IBattleManager battleManager,
            RTInstances<IBattleManager> rtInstances
        )
        {
            this.battleManager = battleManager;
            this.rtInstances = rtInstances;
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
                battleManager.SetupBattle(battleTrigger?.BattleSeed ?? noTriggerRandom.Next());
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
