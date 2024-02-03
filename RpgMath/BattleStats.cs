using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpgMath
{
    public class BattleStats : IBattleStats
    {
        public long Hp { get; set; }

        public long Mp { get; set; }

        public long CurrentHp { get; set; }

        public long CurrentMp { get; set; }

        public long Attack { get; set; }

        public long AttackPercent { get; set; }

        public long Defense { get; set; }

        public long DefensePercent { get; set; }

        public long MagicAttack { get; set; }

        public long MagicAttackPercent { get; set; }

        public long MagicDefense { get; set; }

        public long MagicDefensePercent { get; set; }

        public long TotalDexterity { get; set; }

        public long TotalLuck { get; set; }

        public bool AllowLuckyEvade { get; set; }

        public long Level { get; set; }

        public long ExtraCritChance { get; set; }

        public long CounterPercent { get; set; }

        public float TotalItemUsageBonus { get; set; }

        public float TotalHealingBonus { get; set; }

        public IEnumerable<Element> AttackElements { get; set; } = Enumerable.Empty<Element>();

        public Dictionary<Element, Resistance> Resistances { get; set; }

        public IEnumerable<string> Skills => Enumerable.Empty<string>();

        public List<CharacterBuff> Buffs { get; set; } = new List<CharacterBuff>(); //These are not hooked up to the stats

        public bool QueueTurnsFront => Buffs.Any(i => i.QueueTurnsFront);

        public bool CanBlock { get; set; }

        public bool CanTriggerAttack { get; set; }

        public bool CanSeeEnemyInfo { get; set; }

        public float BlockDamageReduction { get; set; }

        public bool CanDoublecast { get; set; }
        
        public bool CanCureAll { get; set; }

        public Resistance GetResistance(Element element)
        {
            if (Resistances != null && Resistances.TryGetValue(element, out var resistance))
            {
                return resistance;
            }
            return Resistance.Normal;
        }

        public void UpdateBuffs(CharacterBuff buff)
        {
            //Not hooked up
        }
    }
}
