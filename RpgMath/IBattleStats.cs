using System.Collections.Generic;

namespace RpgMath
{
    public interface IBattleStats
    {
        long Hp { get; }
        long Mp { get; }
        long CurrentHp { get; }
        long CurrentMp { get; }
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
        long CounterPercent { get; }
        float TotalItemUsageBonus { get; }
        float TotalHealingBonus { get; }
        IEnumerable<string> Skills { get; }
        IEnumerable<Element> AttackElements { get; }
        List<CharacterBuff> Buffs { get; }
        bool CanBlock { get; }
        bool CanTriggerAttack { get; }
        bool QueueTurnsFront { get; }
        Dictionary<Element, Resistance> Resistances { get; }
        bool CanSeeEnemyInfo { get; }
        float BlockDamageReduction { get; }
        bool CanDoublecast { get; }
        bool CanCureAll { get; }

        Resistance GetResistance(Element element);
    }
}