using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RpgMath
{
    public class CharacterSheet : IBattleStats
    {
        public static CharacterSheet CreateStartingFighter(Random random)
        {
            var s = new CharacterSheet();
            s.Hp = random.Next((int)s.FighterHp - 30, (int)s.FighterHp + 50);
            s.Mp = random.Next((int)s.FighterMp - 1, (int)s.FighterMp + 3);
            s.BaseStrength = random.Next((int)s.FighterStrength - 1, (int)s.FighterStrength + 2);
            s.BaseMagic = random.Next((int)s.FighterMagic - 2, (int)s.FighterMagic + 1);
            s.BaseVitality = random.Next((int)s.FighterVitality - 1, (int)s.FighterVitality + 2);
            s.BaseSpirit = random.Next((int)s.FighterSpirit - 1, (int)s.FighterSpirit + 2);
            s.BaseDexterity = random.Next(5, 9);
            s.BaseLuck = random.Next(12, 15);
            s.CurrentHp = s.Hp;
            s.CurrentMp = s.Mp;
            return s;
        }

        public static CharacterSheet CreateStartingMage(Random random)
        {
            var s = new CharacterSheet();
            s.Hp = random.Next((int)s.MageHp - 10, (int)s.MageHp + 30);
            s.Mp = random.Next((int)s.MageMp - 1, (int)s.MageMp + 7);
            s.BaseStrength = random.Next((int)s.MageStrength - 2, (int)s.MageStrength + 1);
            s.BaseMagic = random.Next((int)s.MageMagic - 1, (int)s.MageMagic + 3);
            s.BaseVitality = random.Next((int)s.MageVitality - 1, (int)s.MageVitality + 2);
            s.BaseSpirit = random.Next((int)s.MageSpirit - 1, (int)s.MageSpirit + 2);
            s.BaseDexterity = random.Next(5, 9);
            s.BaseLuck = random.Next(12, 15);
            s.CurrentHp = s.Hp;
            s.CurrentMp = s.Mp;
            return s;
        }

        public const int MaxLevel = 99;

        public String Name { get; set; }

        public long Hp { get; set; }

        public long FighterHp { get; set; } = 300;

        public long MageHp { get; set; } = 180;

        public long Mp { get; set; }

        public long FighterMp { get; set; } = 15;

        public long MageMp { get; set; } = 25;

        public long BaseStrength { get; set; }

        public long FighterStrength { get; set; } = 18;

        public long MageStrength { get; set; } = 9;

        public long BaseVitality { get; set; }

        public long FighterVitality { get; set; } = 14;

        public long MageVitality { get; set; } = 11;

        public long BaseMagic { get; set; }

        public long FighterMagic { get; set; } = 9;

        public long MageMagic { get; set; } = 18;

        public long BaseSpirit { get; set; }

        public long FighterSpirit { get; set; } = 9;

        public long MageSpirit { get; set; } = 14;

        /// <summary>
        /// This is used to determine base battle timing. Raise it with the dex equation but don't ever modify it by anything.
        /// </summary>
        public long BaseDexterity { get; set; }

        public long BonusDexterity { get; set; }

        public long BaseLuck { get; set; }

        public long BonusLuck { get; set; }


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

        public long CurrentHp { get; set; }

        public long CurrentMp { get; set; }

        public long Attack => BaseStrength + EquippedItems().Sum(i => i.Strength + i.Attack);

        public long AttackPercent => EquippedItems().Sum(i => i.AttackPercent);

        public long Defense => BaseVitality + EquippedItems().Sum(i => i.Vitality + i.Defense);

        public long DefensePercent => EquippedItems().Sum(i => i.DefensePercent);

        public long MagicAttack => BaseMagic + EquippedItems().Sum(i => i.Magic + i.MagicAttack);

        public long MagicAttackPercent => EquippedItems().Sum(i => i.MagicAttackPercent);

        public long MagicDefense => BaseSpirit + EquippedItems().Sum(i => i.Spirit + i.MagicDefense);

        public long MagicDefensePercent => EquippedItems().Sum(i => i.MagicDefensePercent);

        public long Dexterity => BaseDexterity + BonusDexterity + EquippedItems().Sum(i => i.Dexterity);

        public long Luck => BaseLuck + BonusLuck + EquippedItems().Sum(i => i.Luck);

        public bool AllowLuckyEvade => true;

        public long Level { get; set; } = 1;

        public long ExtraCritChance => EquippedItems().Sum(i => i.CritChance);

        public void LevelUpMage(ILevelCalculator levelCalculator)
        {
            var hp = MageHp;
            var mp = MageMp;
            var strength = MageStrength;
            var magic = MageMagic;
            var vitality = MageVitality;
            var spirit = MageSpirit;

            LevelStats(levelCalculator);

            hp = MageHp - hp;
            mp = MageMp - mp;
            strength = MageStrength - strength;
            magic = MageMagic - magic;
            vitality = MageVitality - vitality;
            spirit = MageSpirit - spirit;

            Hp += hp;
            Mp += mp;
            CurrentHp += hp;
            CurrentMp += mp;
            BaseStrength += strength;
            BaseMagic += magic;
            BaseVitality += vitality;
            BaseSpirit += spirit;
        }

        public void LevelUpFighter(ILevelCalculator levelCalculator)
        {
            var hp = FighterHp;
            var mp = FighterMp;
            var strength = FighterStrength;
            var magic = FighterMagic;
            var vitality = FighterVitality;
            var spirit = FighterSpirit;

            LevelStats(levelCalculator);

            hp = FighterHp - hp;
            mp = FighterMp - mp;
            strength = FighterStrength - strength;
            magic = FighterMagic - magic;
            vitality = FighterVitality - vitality;
            spirit = FighterSpirit - spirit;

            Hp += hp;
            Mp += mp;
            CurrentHp += hp;
            CurrentMp += mp;
            BaseStrength += strength;
            BaseMagic += magic;
            BaseVitality += vitality;
            BaseSpirit += spirit;
        }

        private void LevelStats(ILevelCalculator levelCalculator)
        {
            ++Level;
            if (Level > MaxLevel)
            {
                Level = MaxLevel;
            }

            MageHp += levelCalculator.ComputeHpGain(Level, 3, MageHp);
            MageMp += levelCalculator.ComputeMpGain(Level, 3, MageMp);
            MageStrength += levelCalculator.ComputePrimaryStatGain(Level, 23, MageStrength);
            MageVitality += levelCalculator.ComputePrimaryStatGain(Level, 20, MageVitality);
            MageMagic += levelCalculator.ComputePrimaryStatGain(Level, 0, MageMagic);
            MageSpirit += levelCalculator.ComputePrimaryStatGain(Level, 1, MageSpirit);

            FighterHp += levelCalculator.ComputeHpGain(Level, 1, FighterHp);
            FighterMp += levelCalculator.ComputeMpGain(Level, 1, FighterMp);
            FighterStrength += levelCalculator.ComputePrimaryStatGain(Level, 0, FighterStrength);
            FighterVitality += levelCalculator.ComputePrimaryStatGain(Level, 2, FighterVitality);
            FighterMagic += levelCalculator.ComputePrimaryStatGain(Level, 23, FighterMagic);
            FighterSpirit += levelCalculator.ComputePrimaryStatGain(Level, 14, FighterSpirit);

            BaseDexterity += levelCalculator.ComputePrimaryStatGain(Level, 26, BaseDexterity);
            BaseLuck += levelCalculator.ComputeLuckGain(Level, 0, BaseLuck);
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
