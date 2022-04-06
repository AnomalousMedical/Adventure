﻿using Engine;
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

        ISpellEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target);

        bool DefaultTargetPlayers => false;

        string Name { get; }

        long MpCost { get; }
    }

    interface ISpellEffect
    {
        bool Finished { get; set; }
    }

    class SpellEffect : ISpellEffect
    {
        public SpellEffect()
        {

        }

        public SpellEffect(bool finished)
        {
            this.Finished = finished;
        }

        public bool Finished { get; set; }
    }
}
