using System;
using System.Collections.Generic;
using System.Text;

namespace RpgMath
{
    public class Equipment
    {
        public String Name { get; set; }

        public long Attack { get; set; }

        public long AttackPercent { get; set; }

        public long Defense { get; set; }

        public long DefensePercent { get; set; }

        public long MagicAttack { get; set; }

        public long MagicAttackPercent { get; set; }

        public long MagicDefense { get; set; }

        public long MagicDefensePercent { get; set; }

        public long Hp { get; set; }

        public long Mp { get; set; }

        public long Strength { get; set; }

        public long Vitality { get; set; }

        public long Magic { get; set; }

        public long Spirit { get; set; }

        public long Dexterity { get; set; }

        public long Luck { get; set; }

        public long CritChance { get; set; }

        public bool TwoHanded { get; set; }

        public int InventorySlots { get; set; }

        public string Sprite { get; set; }

        public int PalletSeed { get; set; }

        public IEnumerable<String> Skills { get; set; }

        public IEnumerable<Element> AttackElements { get; set; }

        public Guid? Id { get; set; }

        public void EnsureEquipmentId()
        {
            if (Id == null)
            {
                Id = Guid.NewGuid();
            }
        }
    }
}
