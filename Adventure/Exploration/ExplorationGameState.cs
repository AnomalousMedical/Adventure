using BepuPlugin;
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
    interface IExplorationGameState : IGameState
    {
        bool AllowBattles { get; set; }

        void Link(IBattleGameState battleState);

        /// <summary>
        /// Request a battle with a given trigger. The trigger can be null.
        /// </summary>
        /// <param name="battleTrigger"></param>
        void RequestBattle(BattleTrigger battleTrigger = null);
        void SetExplorationEvent(Func<Clock, bool> explorationEvent);
    }

    class ExplorationGameState : IExplorationGameState
    {
        private readonly IBepuScene bepuScene;
        private readonly IZoneManager zoneManager;
        private readonly RTInstances<IZoneManager> rtInstances;
        private readonly IExplorationMenu explorationMenu;
        private readonly IContextMenu contextMenu;
        private IBattleGameState battleState;
        private IGameState nextState; //This is changed per update to be the next game state
        private Func<Clock, bool> explorationEvent;

        public RTInstances Instances => rtInstances;

        public ExplorationGameState(
            ICoroutineRunner coroutineRunner,
            IBepuScene bepuScene,
            IZoneManager zoneManager,
            RTInstances<IZoneManager> rtInstances,
            IExplorationMenu explorationMenu,
            IContextMenu contextMenu)
        {
            this.bepuScene = bepuScene;
            this.zoneManager = zoneManager;
            this.rtInstances = rtInstances;
            this.explorationMenu = explorationMenu;
            this.contextMenu = contextMenu;
            coroutineRunner.RunTask(zoneManager.Restart());
        }

        public void Link(IBattleGameState battleState)
        {
            this.battleState = battleState;
        }

        public void SetActive(bool active)
        {
            //Stopping them both directions
            zoneManager.StopPlayer();
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
                battleTrigger?.BattleWon();
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
