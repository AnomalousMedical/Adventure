using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle.Skills
{
    class Attack : ISkill
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            var attack = objectResolver.Resolve<AttackEffect>();
            attack.Link(attacker, target);
            return attack;
        }

        public string Name => "Attack";

        public long MpCost => 0;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Melee;
    }

    class AttackEffect : ISkillEffect
    {
        private readonly IBattleManager battleManager;
        private IBattleTarget attacker;
        private IBattleTarget target;

        public AttackEffect(IBattleManager battleManager)
        {
            this.battleManager = battleManager;
        }

        public void Link(IBattleTarget attacker, IBattleTarget target)
        {
            this.attacker = attacker;
            this.target = target;
        }

        public bool Finished { get; private set; }

        public void Update(Clock clock)
        {
            battleManager.Attack(attacker, target);
            Finished = true;
        }
    }
}
