﻿using Engine;
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
        protected bool isCounter = false;
        protected bool isPower = false;

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            var attack = objectResolver.Resolve<AttackEffect>();
            attack.Link(attacker, target, isCounter, isPower);
            return attack;
        }

        public string Name => "Attack";

        public long MpCost => 0;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Melee;
    }

    class CounterAttack : Attack
    {
        public CounterAttack()
        {
            isCounter = true;
        }
    }

    class PowerAttack : Attack
    {
        public PowerAttack()
        {
            isCounter = true; //This prevents countering these attacks
            isPower = true;
        }
    }

    class AttackEffect : ISkillEffect
    {
        private readonly IBattleManager battleManager;
        private IBattleTarget attacker;
        private IBattleTarget target;
        private bool isCounter;
        private bool isPower;

        public AttackEffect(IBattleManager battleManager)
        {
            this.battleManager = battleManager;
        }

        public void Link(IBattleTarget attacker, IBattleTarget target, bool isCounter, bool isPower)
        {
            this.attacker = attacker;
            this.target = target;
            this.isCounter = isCounter;
            this.isPower = isPower;
        }

        public bool Finished { get; private set; }

        public void Update(Clock clock)
        {
            battleManager.Attack(attacker, target, isCounter, target.TryBlock(), isPower);
            Finished = true;
        }
    }
}
