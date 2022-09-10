using System;
using Xunit;
using Engine;
using Xunit.Abstractions;

namespace RpgMath.Tests
{
    public class Characters
    {
        public static readonly IBattleStats level10 = new BattleStats()
        {
            Attack = 50,
            AttackPercent = 100,
            Defense = 42,
            DefensePercent = 3,
            MagicAttack = 25,
            MagicDefense = 22,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 13,
            TotalLuck = 19,
            Level = 10,
            GuardPercent = 60,
            Hp = 200,
            Mp = 20
        };

        public static readonly IBattleStats level20 = new BattleStats()
        {
            Attack = 76,
            AttackPercent = 100,
            Defense = 68,
            DefensePercent = 5,
            MagicAttack = 37,
            MagicDefense = 34,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 22,
            TotalLuck = 20,
            Level = 20,
            GuardPercent = 60,
            Hp = 400,
            Mp = 40
        };

        public static readonly IBattleStats level30 = new BattleStats()
        {
            Attack = 100,
            AttackPercent = 100,
            Defense = 102,
            DefensePercent = 7,
            MagicAttack = 51,
            MagicDefense = 47,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 30,
            TotalLuck = 21,
            Level = 30,
            GuardPercent = 60,
            Hp = 800,
            Mp = 80
        };

        public static readonly IBattleStats level40 = new BattleStats()
        {
            Attack = 130,
            AttackPercent = 100,
            Defense = 122,
            DefensePercent = 9,
            MagicAttack = 63,
            MagicDefense = 60,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 38,
            TotalLuck = 22,
            Level = 40,
            GuardPercent = 80,
            Hp = 2000,
            Mp = 200
        };

        public static readonly IBattleStats level50 = new BattleStats()
        {
            Attack = 154,
            AttackPercent = 100,
            Defense = 138,
            DefensePercent = 11,
            MagicAttack = 76,
            MagicDefense = 70,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 46,
            TotalLuck = 23,
            Level = 50,
            GuardPercent = 80,
            Hp = 4000,
            Mp = 300
        };

        public static readonly IBattleStats level60 = new BattleStats()
        {
            Attack = 176,
            AttackPercent = 100,
            Defense = 154,
            DefensePercent = 12,
            MagicAttack = 84,
            MagicDefense = 78,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 51,
            TotalLuck = 23,
            Level = 60,
            GuardPercent = 80,
            Hp = 5000,
            Mp = 400
        };

        public static readonly IBattleStats level70 = new BattleStats()
        {
            Attack = 186,
            AttackPercent = 100,
            Defense = 164,
            DefensePercent = 13,
            MagicAttack = 90,
            MagicDefense = 84,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 53,
            TotalLuck = 24,
            Level = 70,
            GuardPercent = 100,
            Hp = 6000,
            Mp = 600
        };

        public static readonly IBattleStats level80 = new BattleStats()
        {
            Attack = 198,
            AttackPercent = 100,
            Defense = 174,
            DefensePercent = 13,
            MagicAttack = 95,
            MagicDefense = 90,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 55,
            TotalLuck = 25,
            Level = 80,
            GuardPercent = 100,
            Hp = 7000,
            Mp = 700
        };

        public static readonly IBattleStats level90 = new BattleStats()
        {
            Attack = 200,
            AttackPercent = 100,
            Defense = 180,
            DefensePercent = 14,
            MagicAttack = 99,
            MagicDefense = 94,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 57,
            TotalLuck = 26,
            Level = 90,
            GuardPercent = 100,
            Hp = 8000,
            Mp = 800
        };

        public static readonly IBattleStats level99 = new BattleStats()
        {
            Attack = 200,
            AttackPercent = 100,
            Defense = 186,
            DefensePercent = 14,
            MagicAttack = 100,
            MagicDefense = 97,
            MagicDefensePercent = 3,
            AllowLuckyEvade = true,
            TotalDexterity = 59,
            TotalLuck = 26,
            Level = 99,
            GuardPercent = 100,
            Hp = 9000,
            Mp = 900
        };
    }
}
