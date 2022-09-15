﻿using System;

namespace RpgMath
{
    public class DamageCalculator : IDamageCalculator
    {
        private Random random = new Random();

        public long Physical(IBattleStats attacker, IBattleStats target, long power)
        {
            long baseDamage = attacker.Attack + ((attacker.Attack + attacker.Level) / 32L) * ((attacker.Attack * attacker.Level) / 32L);
            return ((power * (512L - target.Defense) * baseDamage) / (16L * 512L));
        }

        public bool PhysicalHit(IBattleStats attacker, IBattleStats target)
        {
            long hitPct = ((attacker.TotalDexterity / 4L) + attacker.AttackPercent) + attacker.DefensePercent - target.DefensePercent;
            long luckRoll = (long)random.Next(100);
            //Lucky hit
            if (luckRoll < attacker.TotalLuck / 4)
            {
                hitPct = 255;
            }
            else if (target.AllowLuckyEvade)
            {
                long evadeChance = target.TotalLuck / 4 - attacker.TotalLuck / 4;
                if (luckRoll < evadeChance)
                {
                    hitPct = 0;
                }
            }

            var rand = (long)random.Next(65536) * 99 / 65535 + 1;
            return rand < hitPct;
        }

        public bool CriticalHit(IBattleStats attacker, IBattleStats target)
        {
            long critPct = (attacker.TotalLuck + attacker.Level - target.Level) / 4 + attacker.ExtraCritChance;
            var rand = (long)random.Next(65536) * 99 / 65535 + 1;
            return rand < critPct;
        }

        /// <summary>
        /// Magical damage formula.
        /// </summary>
        /// <param name="mat">Magic attack</param>
        /// <param name="level">Level of attacker</param>
        /// <param name="power">The power of the attack. 16 is the base power. Above that is extra, below less. Usually 1 unless special effects.</param>
        /// <param name="mdef">Magic defense of target</param>
        /// <returns></returns>
        public long Magical(IBattleStats attacker, IBattleStats target, long power)
        {
            long baseDamage = 6L * (attacker.MagicAttack + attacker.Level);
            return ((power * (512L - target.MagicDefense) * baseDamage) / (16L * 512L));
        }

        public bool MagicalHit(IBattleStats attacker, IBattleStats target, Resistance resistance, long magicAttackPercent)
        {
            long dodgeRoll = (long)random.Next(100);
            if (dodgeRoll < target.MagicDefensePercent)
            {
                return false;
            }

            var hitPercent = magicAttackPercent + attacker.Level - target.Level / 2 - 1;
            switch (resistance)
            {
                case Resistance.Death:
                case Resistance.AutoHit:
                case Resistance.Immune:
                case Resistance.Absorb:
                    hitPercent = 255L;
                    break;
            }

            long rand = (long)random.Next(100);
            return rand < hitPercent;
        }

        /// <summary>
        /// Cure formula.
        /// </summary>
        /// <param name="mat">Magic attack</param>
        /// <param name="level">Level of attacker</param>
        /// <param name="power">The power of the cure. This is 22 * Power.</param>
        /// <returns></returns>
        public long Cure(IBattleStats caster, long power)
        {
            long baseDamage = 6L * (caster.MagicAttack + caster.Level);
            long damage = baseDamage + 22L * power;
            return damage + (long)(damage * caster.TotalHealingBonus);
        }

        /// <summary>
        /// Item formula.
        /// </summary>
        /// <param name="power">The power of the item. This is 16 * Power.</param>
        /// <param name="def">Def or mdef of target.</param>
        /// <returns></returns>
        public long Item(IBattleStats target, long power)
        {
            long baseDamage = 16L * power;
            return baseDamage * (512L - target.Defense) / 512L;
        }

        /// <summary>
        /// Random variation formula.
        /// </summary>
        /// <param name="damage">The calculated damage to randomize.</param>
        /// <returns>A randomized damage value.</returns>
        public long RandomVariation(long damage)
        {
            return damage * (3841L + (long)random.Next(256)) / 4096L;
        }

        public long ApplyResistance(long damage, Resistance resistance)
        {
            switch (resistance)
            {
                case Resistance.Absorb:
                    return -damage;
                case Resistance.Death:
                    return long.MaxValue;
                case Resistance.Immune:
                    return 0;
                case Resistance.Normal:
                    return damage;
                case Resistance.Resist:
                    return damage / 2;
                case Resistance.Weak:
                    return damage * 2;
                case Resistance.Recovery:
                    return long.MinValue;
            }

            return damage;
        }

        /// <summary>
        /// Apply damage. This is subtraction so positive numbers mean take damage, negative means healing.
        /// The return value is capped at 0 and maxHp.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="currentHp"></param>
        /// <param name="maxHp"></param>
        /// <returns></returns>
        public long ApplyDamage(long damage, long currentHp, long maxHp)
        {
            long newHp = currentHp - damage;

            if (damage < 0)
            {
                //damage < 0 means healing, if we end up with less hp it overflowed, max out hp by returning maxHp
                //Also cap newHp at maxHp
                if (newHp < currentHp || newHp > maxHp)
                {
                    newHp = maxHp;
                }
            }
            else
            {
                //damage > 0 means damage, if we end up with more hp the target is dead
                //Also cap new hp at 0
                if (newHp > currentHp || newHp < 0)
                {
                    newHp = 0;
                }
            }

            return newHp;
        }

        public bool Guard(IBattleStats attacker, IBattleStats blocker)
        {
            long hitPct = blocker.GuardPercent;

            var rand = (long)random.Next(65536) * 99 / 65535 + 1;
            return rand < hitPct;
        }

        public long PowerGaugeGain(IBattleStats target, long damage)
        {
            if(damage < 1)
            {
                return 0;
            }

            long statusFactor = 1;
            long lNum = 202; //This can be per character, this is about 67% damage taken to get a full bar

            var unitsGained = ((300 * damage / target.Hp) * 256 * statusFactor / lNum);

            return unitsGained;
        }
    }
}
