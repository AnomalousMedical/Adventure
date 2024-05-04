using Adventure.Items;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using System;

namespace Adventure.Battle.Skills
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
        private readonly PickUpTreasureMenu pickUpTreasureMenu;

        public StealEffect(PickUpTreasureMenu pickUpTreasureMenu, Description description)
        {
            this.battleManager = description.BattleManager;
            this.pickUpTreasureMenu = pickUpTreasureMenu;
            battleManager.AllowActivePlayerGui = false;
            var treasures = battleManager.Steal();
            pickUpTreasureMenu.GatherTreasures(treasures, TimeSpan.FromSeconds(1), (ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState) =>
            {
                var useTarget = battleManager.GetTargetForStats(user);
                treasure.Use(inventory, inventoryFunctions, description.BattleManager, description.ObjectResolver, description.Coroutine, description.Attacker, useTarget, gameState);
            });
        }

        public bool Finished { get; private set; }

        public void Update(Clock clock)
        {
            var padId = battleManager.GetActivePlayer()?.GamepadId ?? GamepadId.Pad1;
            if (pickUpTreasureMenu.Update(padId))
            {
                battleManager.AllowActivePlayerGui = true;
                Finished = true;
            }
        }
    }
}
