﻿using Adventure.Items.Actions;
using Adventure.Items;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Adventure.Skills;

namespace Adventure.Services
{
    class EquipmentTextService(ISkillFactory skillFactory)
    {
        private bool CompareStat(String label, long? currentStat, long? newStat, out SharpText sharpText)
        {
            if (currentStat == null && newStat != null)
            {
                if (newStat > 0)
                {
                    sharpText = new SharpText($"{label} +{newStat}") { Color = Color.UIGreen };
                    return true;
                }
                sharpText = null;
                return false;
            }

            if (currentStat != null && newStat == null)
            {
                if (currentStat > 0)
                {
                    sharpText = new SharpText($"{label} -{currentStat}") { Color = Color.UIRed };
                    return true;
                }
                sharpText = null;
                return false;
            }

            if (currentStat == null && newStat == null)
            {
                sharpText = null;
                return false;
            }

            var difference = newStat - currentStat;
            if (difference != 0)
            {
                sharpText = new SharpText();
                if (difference > 0)
                {
                    sharpText.Text = $"{label} +{difference}";
                    sharpText.Color = Color.UIGreen;
                }
                else
                {
                    sharpText.Text = $"{label} {difference}";
                    sharpText.Color = Color.UIRed;
                }
                return true;
            }
            else
            {
                sharpText = null;
                return false;
            }
        }

        private bool CompareStat(String label, float? currentStat, float? newStat, out SharpText sharpText)
        {
            if (currentStat == null && newStat != null)
            {
                if (newStat > 0)
                {
                    sharpText = new SharpText($"{label} +{newStat}") { Color = Color.UIGreen };
                    return true;
                }
                sharpText = null;
                return false;
            }

            if (currentStat != null && newStat == null)
            {
                if (currentStat > 0)
                {
                    sharpText = new SharpText($"{label} -{currentStat}") { Color = Color.UIRed };
                    return true;
                }
                sharpText = null;
                return false;
            }

            if (currentStat == null && newStat == null)
            {
                sharpText = null;
                return false;
            }

            var difference = newStat - currentStat;
            if (MathF.Abs(difference.Value) > 0.001f)
            {
                sharpText = new SharpText();
                if (difference > 0)
                {
                    sharpText.Text = $"{label} +{difference}";
                    sharpText.Color = Color.UIGreen;
                }
                else
                {
                    sharpText.Text = $"{label} {difference}";
                    sharpText.Color = Color.UIRed;
                }
                return true;
            }
            else
            {
                sharpText = null;
                return false;
            }
        }

        private bool CompareStat(String label, bool? currentStat, bool? newStat, out SharpText sharpText)
        {
            if (currentStat == null && newStat != null)
            {
                if (newStat.Value)
                {
                    sharpText = new SharpText($"+{label}") { Color = Color.UIGreen };
                    return true;
                }
                sharpText = null;
                return false;
            }

            if (currentStat != null && newStat == null)
            {
                if (currentStat.Value)
                {
                    sharpText = new SharpText($"-{label}") { Color = Color.UIRed };
                    return true;
                }
                sharpText = null;
                return false;
            }

            if (currentStat == null && newStat == null)
            {
                sharpText = null;
                return false;
            }

            if (!currentStat.Value && newStat.Value)
            {
                sharpText = new SharpText($"+{label}") { Color = Color.UIGreen };
                return true;
            }

            if (currentStat.Value && !newStat.Value)
            {
                sharpText = new SharpText($"-{label}") { Color = Color.UIRed };
                return true;
            }

            sharpText = null;
            return false;
        }

        private IEnumerable<SharpText> CompareEquipment(Equipment currentItem, Equipment newItem)
        {
            if (currentItem?.Id != null && currentItem.Id == newItem?.Id)
            {
                yield return new SharpText("Unequip") { Color = Color.UIRed };
                newItem = null; //Same item, effect will be to unequip
            }

            bool returnedStat = false;
            SharpText sharpText;
            if (CompareStat("Att:   ", currentItem?.Attack, newItem?.Attack, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Att%:  ", currentItem?.AttackPercent, newItem?.AttackPercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MAtt:  ", currentItem?.MagicAttack, newItem?.MagicAttack, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MAtt%: ", currentItem?.MagicAttackPercent, newItem?.MagicAttackPercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Def:   ", currentItem?.Defense, newItem?.Defense, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Def%:  ", currentItem?.DefensePercent, newItem?.DefensePercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MDef:  ", currentItem?.MagicDefense, newItem?.MagicDefense, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("MDef%: ", currentItem?.MagicDefensePercent, newItem?.MagicDefensePercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Crit%: ", currentItem?.CritChance, newItem?.CritChance, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Item%: ", currentItem?.ItemUsageBonus, newItem?.ItemUsageBonus, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Heal%: ", currentItem?.HealingBonus, newItem?.HealingBonus, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Cntr%: ", currentItem?.CounterPercent, newItem?.CounterPercent, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Slots: ", currentItem?.InventorySlots, newItem?.InventorySlots, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Str: ", currentItem?.Strength, newItem?.Strength, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Mag: ", currentItem?.Magic, newItem?.Magic, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Vit: ", currentItem?.Vitality, newItem?.Vitality, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Spr: ", currentItem?.Spirit, newItem?.Spirit, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Dex: ", currentItem?.Dexterity, newItem?.Dexterity, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Lck: ", currentItem?.Luck, newItem?.Luck, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Active Attack", currentItem?.AllowTriggerAttack, newItem?.AllowTriggerAttack, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Active Block", currentItem?.AllowActiveBlock, newItem?.AllowActiveBlock, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Blck%: ", currentItem?.BlockDamageReduction, newItem?.BlockDamageReduction, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Cure All", currentItem?.CureAll, newItem?.CureAll, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Doublecast", currentItem?.Doublecast, newItem?.Doublecast, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }
            if (CompareStat("Enemy Scope", currentItem?.ShowEnemyInfo, newItem?.ShowEnemyInfo, out sharpText))
            {
                returnedStat = true;
                yield return sharpText;
            }

            if (currentItem?.Skills != null && newItem?.Skills == null)
            {
                returnedStat = true;
                foreach (var skill in currentItem.Skills)
                {
                    var skillInstance = skillFactory.CreateSkill(skill);
                    yield return new SharpText("-" + skillInstance.Name) { Color = Color.UIRed };
                }
            }

            if (currentItem?.Skills == null && newItem?.Skills != null)
            {
                returnedStat = true;
                foreach (var skill in newItem.Skills)
                {
                    var skillInstance = skillFactory.CreateSkill(skill);
                    yield return new SharpText("+" + skillInstance.Name) { Color = Color.UIGreen };
                }
            }

            if (currentItem?.Skills != null && newItem?.Skills != null)
            {
                returnedStat = true;
                foreach (var skill in newItem.Skills.Where(i => !currentItem.Skills.Contains(i)))
                {
                    var skillInstance = skillFactory.CreateSkill(skill);
                    yield return new SharpText("+" + skillInstance.Name) { Color = Color.UIGreen };
                }

                foreach (var skill in currentItem.Skills.Where(i => !newItem.Skills.Contains(i)))
                {
                    var skillInstance = skillFactory.CreateSkill(skill);
                    yield return new SharpText("-" + skillInstance.Name) { Color = Color.UIRed };
                }
            }

            if (!returnedStat)
            {
                yield return new SharpText("No Change") { Color = Color.UIWhite };
            }
        }

        public IEnumerable<SharpText> GetComparisonText(InventoryItem item, Persistence.CharacterData characterData)
        {
            IEnumerable<SharpText> results;
            switch (item.Action)
            {
                case nameof(EquipMainHand):
                    results = CompareEquipment(characterData.CharacterSheet.MainHand, item.Equipment);
                    if (item.Equipment.TwoHanded && characterData.CharacterSheet.OffHand != null)
                    {
                        results = results.Concat(new[] { 
                            new SharpText("Off Hand") { Color = Color.UIWhite },
                            new SharpText("Unequip") { Color = Color.UIRed }
                        });
                        results = results.Concat(CompareEquipment(characterData.CharacterSheet.OffHand, null));
                    }
                    return results;
                case nameof(EquipOffHand):
                    results = CompareEquipment(characterData.CharacterSheet.OffHand, item.Equipment);
                    if (characterData.CharacterSheet.MainHand?.TwoHanded == true)
                    {
                        results = results.Concat(new[] {
                            new SharpText("Main Hand") { Color = Color.UIWhite },
                            new SharpText("Unequip") { Color = Color.UIRed }
                        });
                        results = results.Concat(CompareEquipment(characterData.CharacterSheet.MainHand, null));
                    }
                    return results;
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
                sharpText = new SharpText($"{label} {stat}") { Color = Color.UIWhite };
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
                sharpText = new SharpText($"{label} {stat:n2}") { Color = Color.UIWhite };
                return true;
            }
            else
            {
                sharpText = null;
                return false;
            }
        }

        private bool ShowStat(String label, bool stat, out SharpText sharpText)
        {
            if (stat)
            {
                sharpText = new SharpText(label) { Color = Color.UIWhite };
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
                    yield return new SharpText("Main Hand") { Color = Color.UIWhite };
                    break;
                case nameof(EquipOffHand):
                    yield return new SharpText("Off Hand") { Color = Color.UIWhite };
                    break;
                case nameof(EquipBody):
                    yield return new SharpText("Body") { Color = Color.UIWhite };
                    break;
                case nameof(EquipAccessory):
                    yield return new SharpText("Accessory") { Color = Color.UIWhite };
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
            if (ShowStat("Crit%: ", item.Equipment.CritChance, out sharpText))
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
            if (ShowStat("Cntr%: ", item.Equipment.CounterPercent, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Slots: ", item.Equipment.InventorySlots, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Str: ", item.Equipment.Strength, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Mag: ", item.Equipment.Magic, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Vit: ", item.Equipment.Vitality, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Spr: ", item.Equipment.Spirit, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Dex: ", item.Equipment.Dexterity, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Lck: ", item.Equipment.Luck, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Active Attack", item.Equipment.AllowTriggerAttack, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Active Block", item.Equipment.AllowActiveBlock, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Blck%: ", item.Equipment.BlockDamageReduction * 100.0f, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Cure All", item.Equipment.CureAll, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Doublecast", item.Equipment.Doublecast, out sharpText))
            {
                yield return sharpText;
            }
            if (ShowStat("Enemy Scope", item.Equipment.ShowEnemyInfo, out sharpText))
            {
                yield return sharpText;
            }

            if (item.Equipment.Skills != null)
            {
                foreach (var skill in item.Equipment.Skills)
                {
                    var skillInstance = skillFactory.CreateSkill(skill);
                    yield return new SharpText(skillInstance.Name) { Color = Color.UIWhite };
                }
            }
        }
    }
}
