using Engine;
using Engine.Platform;

namespace Adventure.Battle.Skills
{
    class Guard : ISkill
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            battleManager.ChangeBlockingStatus(attacker);
            return objectResolver.Resolve<GuardEffect>();
        }

        public virtual string Name => "Guard";

        public bool NeedsTarget => false;

        public long GetMpCost(bool triggered, bool triggerSpammed) => 0;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;

        public virtual bool QueueFront => false;
    }

    class QuickGuard : Guard
    {
        public override string Name => "Quick Guard";

        public override bool QueueFront => true;
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
