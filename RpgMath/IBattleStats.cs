using System.Collections.Generic;

namespace RpgMath
{
    public interface IBattleStats
    {
        long Hp { get; }
        long Mp { get; }
        long Attack { get; }
        long AttackPercent { get; }
        long Defense { get; }
        long DefensePercent { get; }
        long MagicAttack { get; }
        long MagicAttackPercent { get; }
        long MagicDefense { get; }
        long MagicDefensePercent { get; }
        long TotalDexterity { get; }
        long TotalLuck { get; }
        bool AllowLuckyEvade { get; }
        long Level { get; }
        long ExtraCritChance { get; }
        long BlockPercent { get; }
        IEnumerable<string> Skills { get; }
        IEnumerable<Element> AttackElements { get; }
        List<CharacterBuff> Buffs { get; }

        Resistance GetResistance(Element element);
    }
}