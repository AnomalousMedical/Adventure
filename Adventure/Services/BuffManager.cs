using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class BuffManager
    {
        private readonly Persistence persistence;
        private readonly IDamageCalculator damageCalculator;

        public BuffManager(Persistence persistence, IDamageCalculator damageCalculator)
        {
            this.persistence = persistence;
            this.damageCalculator = damageCalculator;
        }

        public void Update(Clock clock)
        {
            foreach(var member in persistence.Current.Party.Members)
            {
                var removedBuff = false;
                var buffs = member.CharacterSheet.Buffs;
                for (var i = 0; i < buffs.Count;)
                {
                    var buff = buffs[i];
                    buff.TimeRemaining -= clock.DeltaTimeMicro;
                    if (buff.TimeRemaining <= 0)
                    {
                        buffs.RemoveAt(i);
                        removedBuff = true;
                    }
                    else
                    {
                        ++i;
                    }
                }
                if (removedBuff)
                {
                    member.CharacterSheet.BuffRemoved();
                }

                var effects = member.CharacterSheet.Effects;
                for (var i = 0; i < effects.Count;)
                {
                    var effect = effects[i];
                    effect.TimeRemaining -= clock.DeltaTimeMicro;

                    switch (effect.StatusEffect)
                    {
                        case StatusEffects.Poison:
                            TickPoison(member.CharacterSheet, effect);
                            break;
                        case StatusEffects.DeathTimer:
                            TickDeath(member.CharacterSheet, effect);
                            break;
                    }

                    if (effect.TimeRemaining <= 0)
                    {
                        effects.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }

        private void TickPoison(CharacterSheet stats, CharacterEffect effect)
        {
            if (effect.TimeRemaining < effect.NextEffectTime)
            {
                effect.NextEffectTime -= 1 * Clock.SecondsToMicro;
                var damage = damageCalculator.Magical(effect.AttackerMagicLevelSum, stats, effect.Power);
                damage = damageCalculator.RandomVariation(damage);
                stats.CurrentHp = damageCalculator.ApplyDamage(damage, stats.CurrentHp, stats.Hp);
            }
        }

        private void TickDeath(CharacterSheet stats, CharacterEffect effect)
        {
            if (effect.TimeRemaining < effect.NextEffectTime)
            {
                stats.CurrentHp = 0;
            }
        }
    }
}
