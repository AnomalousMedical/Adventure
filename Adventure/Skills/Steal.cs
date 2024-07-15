using Adventure.Battle;
using Adventure.Items;
using Adventure.Menu;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using System;
using System.Linq;

namespace Adventure.Skills
{
    class Steal : ISkill
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            battleManager.AddDamageNumber(target, "Stealing", Color.White);
            return objectResolver.Resolve<StealEffect, StealEffect.Description>(o =>
            {
                o.BattleManager = battleManager;
                o.ObjectResolver = objectResolver;
                o.Coroutine = coroutine;
                o.Attacker = attacker;
            });
        }

        public string Name => "Steal";

        public long GetMpCost(bool triggered, bool triggerSpammed) => 0;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Melee;
    }

    class StealEffect : ISkillEffect
    {
        public class Description
        {
            public IBattleManager BattleManager { get; set; }
            public IObjectResolver ObjectResolver { get; set; }
            public IScopedCoroutine Coroutine { get; set; }
            public IBattleTarget Attacker { get; set; }
        }

        private readonly IBattleManager battleManager;
        private readonly IExplorationMenu explorationMenu;

        public StealEffect(StealMenu stealMenu, Description description, IExplorationMenu explorationMenu, IBattleManager battleManager)
        {
            this.battleManager = description.BattleManager;
            this.explorationMenu = explorationMenu;
            var treasures = battleManager.Steal();
            if (treasures.Any()) //Only show the menu if there are treasures, this disrupts the gui otherwise for the current player's turn
            {
                battleManager.AllowActivePlayerGui = false;
                stealMenu.SetTreasure(treasures, description.BattleManager, description.ObjectResolver, description.Coroutine, description.Attacker);
                var padId = battleManager.GetActivePlayer()?.GamepadId ?? GamepadId.Pad1;
                explorationMenu.RequestSubMenu(stealMenu, padId);
                battleManager.CancelTargeting();
            }
        }

        public bool Finished { get; private set; }

        public void Update(Clock clock)
        {            
            if (!explorationMenu.Update())
            {
                battleManager.AllowActivePlayerGui = true;
                Finished = true;
            }
        }
    }
}
