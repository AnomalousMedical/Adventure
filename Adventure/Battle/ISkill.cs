using Engine;
using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    public enum SkillAttackStyle
    {
        Melee,
        Cast
    }

    interface ISkill
    {
        void Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target) { }

        ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target);

        bool DefaultTargetPlayers => false;

        /// <summary>
        /// If this is false a skill will auto target the current player.
        /// </summary>
        bool NeedsTarget => true;

        bool QueueFront => false;

        string Name { get; }

        long MpCost { get; }

        SkillAttackStyle AttackStyle { get; }
    }

    interface ISkillEffect
    {
        bool Finished { get; }

        void Update(Clock clock) { }
    }

    class SkillEffect : ISkillEffect
    {
        public SkillEffect(bool finished = false)
        {
            this.Finished = finished;
        }

        public bool Finished { get; set; }

        public void Update(Clock clock)
        {

        }
    }
}
