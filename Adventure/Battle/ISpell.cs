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
    interface ISpell
    {
        void Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target) { }

        ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target);

        bool DefaultTargetPlayers => false;

        string Name { get; }

        long MpCost { get; }
    }

    interface ISkillEffect
    {
        bool Finished { get; }

        void Update(Clock clock);
    }

    class SpellEffect : ISkillEffect
    {
        public SpellEffect()
        {

        }

        public SpellEffect(bool finished)
        {
            this.Finished = finished;
        }

        public bool Finished { get; set; }

        public void Update(Clock clock)
        {

        }
    }
}
