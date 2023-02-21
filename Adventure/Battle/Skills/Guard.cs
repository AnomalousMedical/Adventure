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
    class Guard : ISkill
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            battleManager.ChangeBlockingStatus(attacker);
            return objectResolver.Resolve<GuardEffect>();
        }

        public string Name => "Guard";

        public bool NeedsTarget => false;

        public bool QueueFront => true;

        public long MpCost => 0;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;
    }

    class GuardEffect : ISkillEffect
    {
        public GuardEffect()
        {
        }

        public bool Finished => true;

        public void Update(Clock clock)
        {
            
        }
    }
}
