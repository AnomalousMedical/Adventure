﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace RpgMath
{
    public class CharacterSheet : IBattleStats
    {
        private static Equipment Unarmed = new Equipment
        {
            InfoId = "Unarmed",
            AttackPercent = 95
        };

        public static CharacterSheet CreateStartingFighter(FIRandom random)
        {
            var s = new CharacterSheet();

            s.HpRank = 1;
            s.MpRank = 1;
            s.StrengthRank = 0;
            s.VitalityRank = 2;
            s.MagicRank = 23;
            s.SpiritRank = 14;

            s.DexterityRank = 26;
            s.LuckRank = 0;

            s.Hp = 300;
            s.Mp = 8;
            s.BaseStrength = 18;
            s.BaseMagic = 9;
            s.BaseVitality = 14;
            s.BaseSpirit = 9;
            s.BaseBlockDamageReduction = 0.05f;

            MixStats(random, s);
            s.BaseDexterity = random.Next(5, 9);
            s.BaseLuck = random.Next(12, 15);

            s.CurrentHp = s.Hp;
            s.CurrentMp = s.Mp;
            return s;
        }

        public static CharacterSheet CreateStartingMage(FIRandom random)
        {
            var s = new CharacterSheet();

            s.HpRank = 7;
            s.MpRank = 3;
            s.StrengthRank = 23;
            s.VitalityRank = 20;
            s.MagicRank = 0;
            s.SpiritRank = 1;

            s.DexterityRank = 26;
            s.LuckRank = 0;

            s.Hp = 180;
            s.Mp = 25;
            s.BaseStrength = 9;
            s.BaseMagic = 18;
            s.BaseVitality = 11;
            s.BaseSpirit = 14;

            MixStats(random, s);
            s.BaseDexterity = random.Next(5, 9);
            s.BaseLuck = random.Next(12, 15);

            s.CurrentHp = s.Hp;
            s.CurrentMp = s.Mp;
            return s;
        }

        public static CharacterSheet CreateStartingThief(FIRandom random)
        {
            var s = new CharacterSheet();

            s.HpRank = 8;
            s.MpRank = 8;
            s.StrengthRank = 11;
            s.VitalityRank = 7;
            s.MagicRank = 17;
            s.SpiritRank = 15;

            s.DexterityRank = 26;
            s.LuckRank = 8;

            s.Hp = 300;
            s.Mp = 8;
            s.BaseStrength = 15;
            s.BaseMagic = 11;
            s.BaseVitality = 11;
            s.BaseSpirit = 14;
            s.BaseItemUsageBonus = 0.5f;

            MixStats(random, s);
            s.BaseDexterity = random.Next(5, 9);
            s.BaseLuck = random.Next(12, 15);

            s.CurrentHp = s.Hp;
            s.CurrentMp = s.Mp;
            return s;
        }

        public static CharacterSheet CreateStartingSage(FIRandom random)
        {
            var s = new CharacterSheet();

            s.HpRank = 3;
            s.MpRank = 7;
            s.StrengthRank = 21;
            s.VitalityRank = 4;
            s.MagicRank = 6;
            s.SpiritRank = 4;

            s.DexterityRank = 26;
            s.LuckRank = 7;

            s.Hp = 180;
            s.Mp = 25;
            s.BaseStrength = 9;
            s.BaseMagic = 10;
            s.BaseVitality = 11;
            s.BaseSpirit = 11;
            s.BaseHealingBonus = 0.2f;

            MixStats(random, s);
            s.BaseDexterity = random.Next(5, 9);
            s.BaseLuck = random.Next(12, 15);

            s.CurrentHp = s.Hp;
            s.CurrentMp = s.Mp;
            return s;
        }

        private static void MixStats(FIRandom random, CharacterSheet s)
        {
            s.Hp = random.Next((int)s.Hp - 30, (int)s.Hp + 50);
            s.Mp = random.Next((int)s.Mp - 1, (int)s.Mp + 3);
            s.BaseStrength = random.Next((int)s.BaseStrength - 1, (int)s.BaseStrength + 2);
            s.BaseMagic = random.Next((int)s.BaseMagic - 2, (int)s.BaseMagic + 1);
            s.BaseVitality = random.Next((int)s.BaseVitality - 1, (int)s.BaseVitality + 2);
            s.BaseSpirit = random.Next((int)s.BaseSpirit - 1, (int)s.BaseSpirit + 2);
        }

        public const int MaxLevel = 99;

        public String Name { get; set; }

        public long Hp { get; set; }

        public long Mp { get; set; }

        public long BaseStrength { get; set; }

        public long BonusStrength { get; set; }

        public long BaseVitality { get; set; }

        public long BonusVitality { get; set; }

        public long BaseMagic { get; set; }

        public long BonusMagic { get; set; }

        public long BaseSpirit { get; set; }

        public long BonusSpirit { get; set; }

        /// <summary>
        /// This is used to determine base battle timing. Raise it with the dex equation but don't ever modify it by anything.
        /// </summary>
        public long BaseDexterity { get; set; }

        public long BonusDexterity { get; set; }

        public long BaseLuck { get; set; }

        public long BonusLuck { get; set; }

        public float BaseItemUsageBonus { get; set; }

        public float BaseBlockDamageReduction { get; set; }

        [JsonIgnore]
        public float TotalItemUsageBonus => BaseItemUsageBonus + EquippedItems().Sum(i => i.ItemUsageBonus);

        public float BaseHealingBonus { get; set; }

        [JsonIgnore]
        public float TotalHealingBonus => BaseHealingBonus + EquippedItems().Sum(i => i.HealingBonus);

        [JsonIgnore]
        public IEnumerable<Element> AttackElements => AttackElementsFunc();

        IEnumerable<Element> AttackElementsFunc()
        {
            if (MainHand?.AttackElements != null)
            {
                foreach (var i in MainHand.AttackElements)
                {
                    yield return i;
                }
            }
            if (Buffs != null)
            {
                foreach (var i in Buffs)
                {
                    if (i.AttackElements != null)
                    {
                        foreach (var element in i.AttackElements)
                        {
                            yield return element;
                        }
                    }
                }
            }
        }


        private Equipment mainHand;
        public Equipment MainHand
        {
            get
            {
                return mainHand;
            }
            set
            {
                if (mainHand == value) { return; }

                mainHand = value;
                if (mainHand?.TwoHanded == true)
                {
                    OffHand = null;
                }

                OnMainHandModified?.Invoke(this);
            }
        }

        public event Action<CharacterSheet> OnMainHandModified;

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
                if (offHand != null && mainHand?.TwoHanded == true)
                {
                    MainHand = null;
                }

                OnOffHandModified?.Invoke(this);
            }
        }

        public event Action<CharacterSheet> OnOffHandModified;

        public event Action<CharacterSheet, CharacterEffect> OnEffectApplied;

        private Equipment body;
        public Equipment Body
        {
            get
            {
                return body;
            }
            set
            {
                body = value;
                OnBodyModified?.Invoke(this);
            }
        }
        public event Action<CharacterSheet> OnBodyModified;

        public Equipment Accessory { get; set; }

        public IEnumerable<Equipment> EquippedItems()
        {
            if (MainHand != null)
            {
                yield return MainHand;
            }
            else
            {
                yield return Unarmed;
            }
            if (OffHand != null)
            {
                yield return OffHand;
            }
            if (Body != null)
            {
                yield return Body;
            }
            if (Accessory != null)
            {
                yield return Accessory;
            }
        }

        public void RemoveEquipment(Guid id)
        {
            if (MainHand?.Id == id)
            {
                MainHand = null;
            }
            if (Body?.Id == id)
            {
                Body = null;
            }
            if (OffHand?.Id == id)
            {
                OffHand = null;
            }
            if (Accessory?.Id == id)
            {
                Accessory = null;
            }
        }

        [JsonIgnore]
        public EquipmentTier EquipmentTier => body?.Tier ?? EquipmentTier.Tier1;

        [JsonIgnore]
        public bool CanBlock => OffHand?.AllowActiveBlock == true || MainHand?.AllowActiveBlock == true || Accessory?.AllowActiveBlock == true || Body?.AllowActiveBlock == true;

        [JsonIgnore]
        public bool CanTriggerAttack => OffHand?.AllowTriggerAttack == true || MainHand?.AllowTriggerAttack == true || Accessory?.AllowTriggerAttack == true || Body?.AllowTriggerAttack == true;

        [JsonIgnore]
        public bool QueueTurnsFront => Buffs.Any(i => i.QueueTurnsFront);

        public long CurrentHp { get; set; }

        public long CurrentMp { get; set; }

        [JsonIgnore]
        public long TotalStrength => BaseStrength + BonusStrength + EquippedItems().Sum(i => i.Strength) + Buffs.Sum(i => i.Strength);

        [JsonIgnore]
        public long Attack => BaseStrength + BonusStrength + EquippedItems().Sum(i => i.Strength + i.Attack) + Buffs.Sum(i => i.Strength + i.Attack);

        [JsonIgnore]
        public long AttackPercent => EquippedItems().Sum(i => i.AttackPercent) + Buffs.Sum(i => i.AttackPercent);

        [JsonIgnore]
        public long TotalVitality => BaseVitality + BonusVitality + EquippedItems().Sum(i => i.Vitality) + Buffs.Sum(i => i.Vitality);

        [JsonIgnore]
        public long Defense => BaseVitality + BonusVitality + EquippedItems().Sum(i => i.Vitality + i.Defense) + Buffs.Sum(i => i.Vitality + i.Defense);

        [JsonIgnore]
        public long DefensePercent => EquippedItems().Sum(i => i.DefensePercent) + Buffs.Sum(i => i.DefensePercent);

        [JsonIgnore]
        public long TotalMagic => BaseMagic + BonusMagic + EquippedItems().Sum(i => i.Magic) + Buffs.Sum(i => i.Magic);

        [JsonIgnore]
        public long MagicAttack => BaseMagic + BonusMagic + EquippedItems().Sum(i => i.Magic + i.MagicAttack) + Buffs.Sum(i => i.Magic + i.MagicAttack);

        [JsonIgnore]
        public long MagicAttackPercent => EquippedItems().Sum(i => i.MagicAttackPercent) + Buffs.Sum(i => i.MagicAttackPercent);

        [JsonIgnore]
        public long TotalSpirit => BaseSpirit + BonusSpirit + EquippedItems().Sum(i => i.Spirit) + Buffs.Sum(i => i.Spirit);

        [JsonIgnore]
        public long MagicDefense => BaseSpirit + BonusSpirit + EquippedItems().Sum(i => i.Spirit + i.MagicDefense) + Buffs.Sum(i => i.Spirit + i.MagicDefense);

        [JsonIgnore]
        public long MagicDefensePercent => EquippedItems().Sum(i => i.MagicDefensePercent) + Buffs.Sum(i => i.MagicDefensePercent);

        [JsonIgnore]
        public long TotalDexterity => BaseDexterity + BonusDexterity + EquippedItems().Sum(i => i.Dexterity) + Buffs.Sum(i => i.Dexterity);

        [JsonIgnore]
        public long TotalLuck => BaseLuck + BonusLuck + EquippedItems().Sum(i => i.Luck) + Buffs.Sum(i => i.Luck);

        [JsonIgnore]
        public int InventorySize => 6 + EquippedItems().Sum(i => i.InventorySlots);

        [JsonIgnore]
        public long CounterPercent => EquippedItems().Sum(i => i.CounterPercent);

        [JsonIgnore]
        public bool AllowLuckyEvade => true;

        public long Level { get; set; } = 1;

        [JsonIgnore]
        public long ExtraCritChance => EquippedItems().Sum(i => i.CritChance) + Buffs.Sum(i => i.CritChance);

        public long PowerGauge { get; set; }

        [JsonIgnore]
        public float PowerGaugePct => Math.Min(PowerGauge / 255f, 1.0f);

        [JsonIgnore]
        public bool AtPowerMax => PowerGauge >= 255;

        [JsonIgnore]
        public bool CanSeeEnemyInfo => EquippedItems().Any(i => i.ShowEnemyInfo);

        [JsonIgnore]
        public bool CanDoublecast => EquippedItems().Any(i => i.Doublecast);

        [JsonIgnore]
        public bool CanCureAll => EquippedItems().Any(i => i.CureAll);

        [JsonIgnore]
        public float BlockDamageReduction => BaseBlockDamageReduction + EquippedItems().Sum(i => i.BlockDamageReduction);

        public List<CharacterBuff> Buffs { get; set; } = new List<CharacterBuff>();

        public List<CharacterEffect> Effects { get; set; } = new List<CharacterEffect>();

        public event Action<CharacterSheet> OnBuffsModified;
        public void UpdateBuffs(CharacterBuff buff)
        {
            var count = Buffs.Count;
            for (var i = 0; i < count; i++)
            {
                var activeBuff = Buffs[i];
                if (activeBuff.BuffTypeId == buff.BuffTypeId)
                {
                    Buffs[i] = buff;
                    return;
                }
            }

            //If nothing was added above, add the new buff
            Buffs.Add(buff);
            OnBuffsModified?.Invoke(this);
        }

        public void BuffRemoved()
        {
            OnBuffsModified?.Invoke(this);
        }

        [JsonIgnore]
        public IEnumerable<String> Skills
        {
            get
            {
                foreach (var item in EquippedItems())
                {
                    if (item.Skills != null)
                    {
                        foreach (var skill in item.Skills)
                        {
                            yield return skill;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Level up, this will always increase stats even if the max level
        /// has been reached, so something else must prevent that if desired.
        /// </summary>
        /// <param name="levelCalculator"></param>
        public void LevelUp(ILevelCalculator levelCalculator)
        {
            ++Level;
            if (Level > MaxLevel)
            {
                Level = MaxLevel;
            }

            Hp += levelCalculator.ComputeHpGain(Level, HpRank, Hp);
            Mp += levelCalculator.ComputeMpGain(Level, MpRank, Mp);
            BaseStrength += levelCalculator.ComputePrimaryStatGain(Level, StrengthRank, BaseStrength);
            BaseVitality += levelCalculator.ComputePrimaryStatGain(Level, VitalityRank, BaseVitality);
            BaseMagic += levelCalculator.ComputePrimaryStatGain(Level, MagicRank, BaseMagic);
            BaseSpirit += levelCalculator.ComputePrimaryStatGain(Level, SpiritRank, BaseSpirit);

            BaseDexterity += levelCalculator.ComputePrimaryStatGain(Level, DexterityRank, BaseDexterity);
            BaseLuck += levelCalculator.ComputeLuckGain(Level, LuckRank, BaseLuck);

            CurrentHp = Hp;
            CurrentMp = Mp;
        }

        public int HpRank { get; set; }

        public int MpRank { get; set; }

        public int StrengthRank { get; set; }

        public int VitalityRank { get; set; }

        public int MagicRank { get; set; }

        public int SpiritRank { get; set; }

        public int DexterityRank { get; set; }

        public int LuckRank { get; set; }

        public Resistance GetResistance(Element element)
        {
            return Resistance.Normal;
        }

        public Dictionary<Element, Resistance> Resistances { get; } = new Dictionary<Element, Resistance>();

        public void Rest()
        {
            this.CurrentHp = Hp;
            this.CurrentMp = Mp;
            this.Buffs.Clear();
            OnBuffsModified?.Invoke(this);
        }
    }
}
