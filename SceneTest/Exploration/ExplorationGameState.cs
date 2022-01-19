﻿using BepuPlugin;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using SceneTest.Battle;
using SceneTest.Exploration.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneTest
{
    class ExplorationGameState : IExplorationGameState
    {
        private readonly IBepuScene bepuScene;
        private readonly ILevelManager levelManager;
        private readonly RTInstances<ILevelManager> rtInstances;
        private readonly IExplorationMenu explorationMenu;
        private readonly IContextMenu contextMenu;
        private IBattleGameState battleState;
        private IGameState nextState; //This is changed per update to be the next game state

        public RTInstances Instances => rtInstances;

        public ExplorationGameState(
            ICoroutineRunner coroutineRunner,
            IBepuScene bepuScene,
            ILevelManager levelManager,
            RTInstances<ILevelManager> rtInstances,
            IExplorationMenu explorationMenu,
            IContextMenu contextMenu)
        {
            this.bepuScene = bepuScene;
            this.levelManager = levelManager;
            this.rtInstances = rtInstances;
            this.explorationMenu = explorationMenu;
            this.contextMenu = contextMenu;
            coroutineRunner.RunTask(levelManager.Restart());
        }

        public void Link(IBattleGameState battleState)
        {
            this.battleState = battleState;
        }

        public void SetActive(bool active)
        {
            //Stopping them both directions
            levelManager.StopPlayer();
        }

        public bool AllowBattles { get; set; } = true;

        public void RequestBattle(BattleTrigger battleTrigger)
        {
            if (AllowBattles)
            {
                battleState.SetBattleTrigger(battleTrigger);
                nextState = battleState;
            }
            else
            {
                battleTrigger.BattleWon();
                battleTrigger?.RequestDestruction();
            }
        }

        public IGameState Update(Clock clock)
        {
            nextState = this;

            if (explorationMenu.Update(this))
            {
                //If menu did something
            }
            else
            {
                bepuScene.Update(clock, new System.Numerics.Vector3(0, 0, 1));
                contextMenu.Update();
            }

            return nextState;
        }
    }
}
