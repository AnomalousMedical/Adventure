using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RpgMath
{
    [JsonConverter(typeof(JsonStringEnumConverter<EquipmentTier>))]
    public enum EquipmentTier
    {
        Tier1,
        Tier2,
        Tier3,
    }

    public class Equipment
    {
        public String InfoId { get; set; }

        public long Attack { get; set; }

        public long AttackPercent { get; set; }

        public long Defense { get; set; }

        public long DefensePercent { get; set; }

        public long MagicAttack { get; set; }

        public long MagicAttackPercent { get; set; }

        public long MagicDefense { get; set; }

        public long MagicDefensePercent { get; set; }

        public long Strength { get; set; }

        public long Vitality { get; set; }

        public long Magic { get; set; }

        public long Spirit { get; set; }

        public long Dexterity { get; set; }

        public long Luck { get; set; }

        public long CritChance { get; set; }

        public long CounterPercent { get; set; }

        public bool TwoHanded { get; set; }

        public int InventorySlots { get; set; }

        public string Sprite { get; set; }

        public bool ShowHand { get; set; } = true;

        public bool AllowActiveBlock { get; set; }

        public bool AllowTriggerAttack { get; set; }

        public bool ShowEnemyInfo { get; set; }

        public float ItemUsageBonus { get; set; }

        public float HealingBonus { get; set; }

        public float BlockDamageReduction { get; set; }

        public bool Doublecast { get; set; }

        public bool CureAll { get; set; }

        public IEnumerable<String> Skills { get; set; }

        public IEnumerable<Element> AttackElements { get; set; }

        public Guid? Id { get; set; }

        public EquipmentTier Tier { get; set; }

        public void EnsureEquipmentId()
        {
            if (Id == null)
            {
                Id = Guid.NewGuid();
            }
        }
    }
}
