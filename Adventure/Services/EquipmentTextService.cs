using Adventure.Items.Actions;
using Adventure.Items;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;

namespace Adventure.Services
{
    class EquipmentTextService
    {
        private bool CompareStat(String label, long currentStat, long newStat, out SharpText sharpText)
        {
            var difference = newStat - currentStat;
            if (difference != 0)
            {
                sharpText = new SharpText();
                if (difference > 0)
                {
                    sharpText.Text = $"{label} +{difference}";
                    sharpText.Color = Color.Green;
                }
                else
                {
                    sharpText.Text = $"{label} {difference}";
                    sharpText.Color = Color.Red;
                }
                return true;
            }
            else
            {
                sharpText = null;
                return false;
            }
        }

        private bool CompareStat(String label, float currentStat, float newStat, out SharpText sharpText)
        {
            var difference = newStat - currentStat;
            if (Math.Abs(difference) > 0.001f)
            {
                sharpText = new SharpText();
                if (difference > 0)
                {
                    sharpText.Text = $"{label} +{difference}";
                    sharpText.Color = Color.Green;
                }
                else
                {
                    sharpText.Text = $"{label} {difference}";
                    sharpText.Color = Color.Red;
                }
                return true;
            }
            else
            {
                sharpText = null;
                return false;
            }
        }

        private IEnumerable<SharpText> CompareEquipment(Equipment currentItem, Equipment newItem)
        {
            bool returnedStat = false;
            SharpText sharpText;
            if (CompareStat("Att:   ", currentItem.Attack, newItem.Attack, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Att%:  ", currentItem.AttackPercent, newItem.AttackPercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MAtt:  ", currentItem.MagicAttack, newItem.MagicAttack, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MAtt%: ", currentItem.MagicAttackPercent, newItem.MagicAttackPercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Def:   ", currentItem.Defense, newItem.Defense, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Def%:  ", currentItem.DefensePercent, newItem.DefensePercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MDef:  ", currentItem.MagicDefense, newItem.MagicDefense, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MDef%: ", currentItem.MagicDefensePercent, newItem.MagicDefensePercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Item%: ", currentItem.ItemUsageBonus, newItem.ItemUsageBonus, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Heal%: ", currentItem.HealingBonus, newItem.HealingBonus, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Blck%: ", currentItem.BlockDamageReduction, newItem.BlockDamageReduction, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }

            if (!returnedStat)
            {
                if (currentItem.Id != null && currentItem.Id == newItem.Id)
                {
                    yield return new SharpText("Unequip") { Color = Color.White };
                }
                else
                {
                    yield return new SharpText("No Change") { Color = Color.White };
                }
            }
        }

        public IEnumerable<SharpText> GetComparisonText(InventoryItem item, Persistence.CharacterData characterData)
        {
            switch (item.Action)
            {
                case nameof(EquipMainHand):
                    return CompareEquipment(characterData.CharacterSheet.MainHand, item.Equipment);
                case nameof(EquipOffHand):
                    return CompareEquipment(characterData.CharacterSheet.OffHand, item.Equipment);
                case nameof(EquipBody):
                    return CompareEquipment(characterData.CharacterSheet.Body, item.Equipment);
                case nameof(EquipAccessory):
                    return CompareEquipment(characterData.CharacterSheet.Accessory, item.Equipment);
            }

            return Enumerable.Empty<SharpText>();
        }

        private bool ShowStat(String label, long stat, out SharpText sharpText)
        {
            if (stat > 0)
            {
                sharpText = new SharpText($"{label} {stat}") { Color = Color.White };
                return true;
            }
            else
            {
                sharpText = null;
                return false;
            }
        }

        private bool ShowStat(String label, float stat, out SharpText sharpText)
        {
            if (stat > 0.0f)
            {
                sharpText = new SharpText($"{label} {stat:n2}") { Color = Color.White };
                return true;
            }
            else
            {
                sharpText = null;
                return false;
            }
        }

        public IEnumerable<SharpText> BuildEquipmentText(InventoryItem item)
        {
            SharpText sharpText;
            switch (item.Action)
            {
                case nameof(EquipMainHand):
                    yield return new SharpText("Main Hand") { Color = Color.White };
                    break;
                case nameof(EquipOffHand):
                    yield return new SharpText("Off Hand") { Color = Color.White };
                    break;
                case nameof(EquipBody):
                    yield return new SharpText("Body") { Color = Color.White };
                    break;
                case nameof(EquipAccessory):
                    yield return new SharpText("Accessory") { Color = Color.White };
                    break;
            }

            if (ShowStat("Att:   ", item.Equipment.Attack, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Att%:  ", item.Equipment.AttackPercent, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("MAtt:  ", item.Equipment.MagicAttack, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("MAtt%: ", item.Equipment.MagicAttackPercent, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Def:   ", item.Equipment.Defense, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Def%:  ", item.Equipment.DefensePercent, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("MDef:  ", item.Equipment.MagicDefense, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("MDef%: ", item.Equipment.MagicDefensePercent, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Item%: ", item.Equipment.ItemUsageBonus * 100.0f, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Heal%: ", item.Equipment.HealingBonus * 100.0f, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Blck%: ", item.Equipment.BlockDamageReduction * 100.0f, out sharpText))
            {
                yield return sharpText;
            }
        }
    }
}
