using SharpGui;
using System;
using System.Collections.Generic;
using Engine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RpgMath;

namespace Adventure.Services
{
    internal class CharacterStatsTextService(ILanguageService languageService)
    {
        public IEnumerable<SharpText> GetInventorySpace(IEnumerable<Persistence.CharacterData> members)
        {
            var text = "";
            foreach (var character in members)
            {
                text += $@"{character.CharacterSheet.Name}
Items:  {character.Inventory.Items.Count} / {character.CharacterSheet.InventorySize}
  
";
            }

            yield return new SharpText(text) { Color = Color.White };
        }

        private Color GetHpColor(CharacterSheet characterSheet)
        {
            return characterSheet.CurrentHp > 0 && (float)characterSheet.CurrentHp / characterSheet.Hp < 0.35f ? Color.Yellow : Color.White;
        }

        private Color GetMpColor(CharacterSheet characterSheet)
        {
            return characterSheet.CurrentHp > 0 && characterSheet.CurrentMp > 0 && (float)characterSheet.CurrentMp / characterSheet.Mp < 0.35f ? Color.Yellow : Color.White;
        }

        public IEnumerable<SharpText> GetVitalStats(IEnumerable<Persistence.CharacterData> members)
        {
            foreach (var character in members)
            {
                yield return new SharpText(character.CharacterSheet.Name) { Color = Color.White };
                yield return new SharpText($"HP:  {character.CharacterSheet.CurrentHp} / {character.CharacterSheet.Hp}") 
                {
                    Color = GetHpColor(character.CharacterSheet)
                };
                yield return new SharpText($"MP:  {character.CharacterSheet.CurrentMp} / {character.CharacterSheet.Mp}\n ")
                {
                    Color = GetMpColor(character.CharacterSheet)
                };
            }
        }

        public IEnumerable<SharpText> GetFullStats(Persistence.CharacterData characterData)
        {
            var characterSheetDisplay = characterData;

            var text =
    $@"{characterSheetDisplay.CharacterSheet.Name}
{languageService.Current.Levels.GetText(characterSheetDisplay.CharacterSheet.Level)}
 ";
            yield return new SharpText(text) { Color = Color.White };

            yield return new SharpText($"HP:  {characterSheetDisplay.CharacterSheet.CurrentHp} / {characterSheetDisplay.CharacterSheet.Hp}")
            {
                Color = GetHpColor(characterSheetDisplay.CharacterSheet)
            };
            yield return new SharpText($"MP:  {characterSheetDisplay.CharacterSheet.CurrentMp} / {characterSheetDisplay.CharacterSheet.Mp}\n ")
            {
                Color = GetMpColor(characterSheetDisplay.CharacterSheet)
            };

            yield return new SharpText($"Items:  {characterSheetDisplay.Inventory.Items.Count} / {characterSheetDisplay.CharacterSheet.InventorySize}\n ") { Color = Color.White };

            text = 
$@"Att:   {characterSheetDisplay.CharacterSheet.Attack}
Att%:  {characterSheetDisplay.CharacterSheet.AttackPercent}
MAtt:  {characterSheetDisplay.CharacterSheet.MagicAttack}
MAtt%: {characterSheetDisplay.CharacterSheet.MagicAttackPercent}
Def:   {characterSheetDisplay.CharacterSheet.Defense}
Def%:  {characterSheetDisplay.CharacterSheet.DefensePercent}
MDef:  {characterSheetDisplay.CharacterSheet.MagicDefense}
MDef%: {characterSheetDisplay.CharacterSheet.MagicDefensePercent}
Item%: {characterSheetDisplay.CharacterSheet.TotalItemUsageBonus * 100f + 100f}
Heal%: {characterSheetDisplay.CharacterSheet.TotalHealingBonus * 100f + 100f}
 
Str: {characterSheetDisplay.CharacterSheet.TotalStrength}
Mag: {characterSheetDisplay.CharacterSheet.TotalMagic}
Vit: {characterSheetDisplay.CharacterSheet.TotalVitality}
Spr: {characterSheetDisplay.CharacterSheet.TotalSpirit}
Dex: {characterSheetDisplay.CharacterSheet.TotalDexterity}
Lck: {characterSheetDisplay.CharacterSheet.TotalLuck}
 ";

            foreach (var item in characterSheetDisplay.CharacterSheet.EquippedItems())
            {
                text += $@"
{languageService.Current.Items.GetText(item.InfoId)}";
            }

            foreach (var item in characterSheetDisplay.CharacterSheet.Buffs)
            {
                text += $@"
{item.Name}";
            }

            foreach (var item in characterSheetDisplay.CharacterSheet.Effects)
            {
                text += $@"
{item.StatusEffect}";
            }

            yield return new SharpText(text) { Color = Color.White };
        }
    }
}
