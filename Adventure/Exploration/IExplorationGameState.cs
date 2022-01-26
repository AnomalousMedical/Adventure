using Engine.Platform;
using Adventure.Battle;
using System;

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
}