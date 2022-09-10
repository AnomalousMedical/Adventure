﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RpgMath
{
    public class ClassicCharacterSheet : IBattleStats
    {
        public const int MaxLevel = 99;

        public String Name { get; set; }

        public Archetype Archetype { get; set; }

        private Equipment mainHand;
        public Equipment MainHand
        {
            get
            {
                return mainHand;
            }
            set
            {
                mainHand = value;
                if (mainHand.TwoHanded)
                {
                    OffHand = null;
                }
            }
        }

        private Equipment offHand;
        public Equipment OffHand
        {
            get
            {
                return offHand;
            }
            set
            {
                offHand = value;
                if (mainHand.TwoHanded)
                {
                    mainHand = null;
                }
            }
        }

        public Equipment Body { get; set; }

        public Equipment Accessory { get; set; }

        private IEnumerable<Equipment> EquippedItems()
        {
            if (MainHand != null)
            {
                yield return MainHand;
            }
            if (Body != null)
            {
                yield return Body;
            }
            if (OffHand != null)
            {
                yield return OffHand;
            }
            if (Accessory != null)
            {
                yield return Accessory;
            }
        }

        public long Hp => Archetype.BaseHp + Archetype.BonusHp;

        public long Mp => Archetype.BaseMp + Archetype.BonusMp;

        public long CurrentHp { get; set; }

        public long CurrentMp { get; set; }

        public long Attack => Archetype.BaseStrength + Archetype.BonusStrength + EquippedItems().Sum(i => i.Strength + i.Attack);

        public long AttackPercent => EquippedItems().Sum(i => i.AttackPercent);

        public long Defense => Archetype.BaseVitality + Archetype.BonusVitality + EquippedItems().Sum(i => i.Vitality + i.Defense);

        public long DefensePercent => EquippedItems().Sum(i => i.DefensePercent);

        public long MagicAttack => Archetype.BaseMagic + Archetype.BonusMagic + EquippedItems().Sum(i => i.Magic + i.MagicAttack);

        public long MagicAttackPercent => EquippedItems().Sum(i => i.MagicAttackPercent);

        public long MagicDefense => Archetype.BaseSpirit + Archetype.BonusSpirit + EquippedItems().Sum(i => i.Spirit + i.MagicDefense);

        public long MagicDefensePercent => EquippedItems().Sum(i => i.MagicDefensePercent);

        public long BaseDexterity => Archetype.BaseDexterity;

        public long TotalDexterity => Archetype.BaseDexterity + Archetype.BonusDexterity + EquippedItems().Sum(i => i.Dexterity);

        public long TotalLuck => Archetype.BaseLuck + Archetype.BonusLuck + EquippedItems().Sum(i => i.Luck);

        public long GuardPercent => EquippedItems().Sum(i => i.GuardPercent);

        public bool AllowLuckyEvade => true;

        public long CurrentXp { get; set; }

        public long Level { get; set; } = 1;

        public long ExtraCritChance => EquippedItems().Sum(i => i.CritChance);

        public IEnumerable<Element> AttackElements => MainHand?.AttackElements ?? Enumerable.Empty<Element>();

        public IEnumerable<string> Skills => Enumerable.Empty<string>();

        public List<CharacterBuff> Buffs { get; set; } = new List<CharacterBuff>(); //Not hooked up

        public void LevelUp(ILevelCalculator levelCalculator)
        {
            ++Level;
            if(Level > MaxLevel)
            {
                Level = MaxLevel;
            }
            var hpGain = levelCalculator.ComputeHpGain(Level, Archetype.HpGrade, Archetype.BaseHp);
            var mpGain = levelCalculator.ComputeMpGain(Level, Archetype.MpGrade, Archetype.BaseMp); ;
            Archetype.BaseHp += hpGain;
            CurrentHp += hpGain;
            Archetype.BaseMp += mpGain;
            CurrentMp += mpGain;
            Archetype.BaseStrength  += levelCalculator.ComputePrimaryStatGain(Level, Archetype.StrengthGrade, Archetype.BaseStrength);
            Archetype.BaseVitality  += levelCalculator.ComputePrimaryStatGain(Level, Archetype.VitalityGrade, Archetype.BaseVitality);
            Archetype.BaseMagic     += levelCalculator.ComputePrimaryStatGain(Level, Archetype.MagicGrade, Archetype.BaseMagic);
            Archetype.BaseSpirit    += levelCalculator.ComputePrimaryStatGain(Level, Archetype.SpiritGrade, Archetype.BaseSpirit);
            Archetype.BaseDexterity += levelCalculator.ComputePrimaryStatGain(Level, Archetype.DexterityGrade, Archetype.BaseDexterity);
            Archetype.BaseLuck      += levelCalculator.ComputeLuckGain(Level, Archetype.LuckGrade, Archetype.BaseLuck);
        }

        public Resistance GetResistance(Element element)
        {
            return Resistance.Normal;
        }

        public void Rest()
        {
            this.CurrentHp = Hp;
            this.CurrentMp = Mp;
        }
    }
}
