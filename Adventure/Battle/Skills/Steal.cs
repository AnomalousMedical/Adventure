using Engine;
using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Platform;
using SharpGui;
using Adventure.Services;

namespace Adventure.Battle.Skills
{
    class Steal : ISkill
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            battleManager.AddDamageNumber(target, "Stealing", Color.White);
            return objectResolver.Resolve<StealEffect>();
        }

        public string Name => "Steal";

        public long MpCost => 0;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Melee;
    }

    class StealEffect : ISkillEffect
    {
        private readonly IBattleManager battleManager;
        private readonly PickUpTreasureMenu pickUpTreasureMenu;

        public StealEffect(IBattleManager battleManager, PickUpTreasureMenu pickUpTreasureMenu)
        {
            this.battleManager = battleManager;
            this.pickUpTreasureMenu = pickUpTreasureMenu;
            battleManager.AllowActivePlayerGui = false;
            var treasures = battleManager.Steal();
            pickUpTreasureMenu.GatherTreasures(treasures);
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
