using Engine;
using Engine.Platform;

namespace Adventure.Battle.Skills
{
    class Guard : ISkill
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            battleManager.ChangeBlockingStatus(attacker);
            return objectResolver.Resolve<GuardEffect>();
        }

        public virtual string Name => "Guard";

        public bool NeedsTarget => false;

        public long MpCost => 0;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;
    }

    class QuickGuard : Guard
    {
        public override string Name => "Quick Guard";

        public bool QueueFront => true;
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
