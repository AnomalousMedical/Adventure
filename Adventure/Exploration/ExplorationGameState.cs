﻿using BepuPlugin;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Adventure.Battle;
using Adventure.Exploration.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
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
        private Func<Clock, bool> explorationEvent;

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
                if (battleTrigger != null)
                {
                    battleTrigger.BattleWon();
                    battleTrigger.RequestDestruction();
                }
            }
        }

        public void SetExplorationEvent(Func<Clock, bool> explorationEvent)
        {
            this.explorationEvent = explorationEvent;
        }

        public IGameState Update(Clock clock)
        {
            nextState = this;

            if(explorationEvent != null)
            {
                if (!explorationEvent.Invoke(clock))
                {
                    explorationEvent = null;
                }
            }
            else if (explorationMenu.Update(this))
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
