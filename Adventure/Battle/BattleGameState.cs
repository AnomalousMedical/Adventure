using DiligentEngine.RT;
using Engine.Platform;
using Adventure.GameOver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adventure.Items.Creators;

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
        private readonly PotionCreator potionCreator;
        private IGameState gameOverState;
        private IGameState returnState;
        private BattleTrigger battleTrigger;
        private Random noTriggerRandom = new Random();

        public BattleGameState
        (
            IBattleManager battleManager,
            RTInstances<IBattleManager> rtInstances,
            Party party,
            PotionCreator potionCreator
        )
        {
            this.battleManager = battleManager;
            this.rtInstances = rtInstances;
            this.party = party;
            this.potionCreator = potionCreator;
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
                bool boss = false;
                Func<IEnumerable<ITreasure>> stealCb;
                BiomeEnemy triggerEnemy = null;
                if(battleTrigger == null)
                {
                    level = party.GetAverageLevel() * 4 / 5;
                    if (level < 1)
                    {
                        level = 1;
                    }
                    battleSeed = noTriggerRandom.Next(int.MinValue, int.MaxValue);
                    boss = true;
                    var hasTreasure = true;
                    stealCb = () =>
                    {
                        if (hasTreasure)
                        {
                            hasTreasure = false;
                            return new[] { new Treasure(potionCreator.CreateManaPotion(level)) };
                        }
                        else
                        {
                            return Enumerable.Empty<Treasure>();
                        }
                    };
                }
                else
                {
                    level = battleTrigger.EnemyLevel;
                    battleSeed = battleTrigger.BattleSeed;
                    boss = battleTrigger.IsBoss;
                    stealCb = battleTrigger.StealTreasure;
                    triggerEnemy = battleTrigger.TriggerEnemy;
                }

                battleManager.SetupBattle(battleSeed, level, boss, stealCb, triggerEnemy);
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
