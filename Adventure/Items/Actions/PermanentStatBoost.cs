using Adventure.Battle;
using Adventure.Services;
using Adventure.Skills;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Actions
{
    abstract class PermanentStatBoost : IInventoryAction
    {
        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);
            RaiseStat(item, target);
        }

        public ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            inventory.Items.Remove(item);
            RaiseStat(item, target);

            return null;
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item); //If this is used on an enemy it does nothing, but its still lost

            if (target.BattleTargetType == BattleTargetType.Player)
            {
                var characterSheet = target.Stats as CharacterSheet;
                RaiseStat(item, characterSheet);
            }
        }

        protected abstract void RaiseStat(InventoryItem item, CharacterSheet characterSheet);
    }

    class StrengthBoost : PermanentStatBoost
    {
        protected override void RaiseStat(InventoryItem item, CharacterSheet characterSheet)
        {
            characterSheet.BonusStrength += item.Number.Value;
        }
    }

    class MagicBoost : PermanentStatBoost
    {
        protected override void RaiseStat(InventoryItem item, CharacterSheet characterSheet)
        {
            characterSheet.BonusMagic += item.Number.Value;
        }
    }

    class SpiritBoost : PermanentStatBoost
    {
        protected override void RaiseStat(InventoryItem item, CharacterSheet characterSheet)
        {
            characterSheet.BonusSpirit += item.Number.Value;
        }
    }

    class VitalityBoost : PermanentStatBoost
    {
        protected override void RaiseStat(InventoryItem item, CharacterSheet characterSheet)
        {
            characterSheet.BonusVitality += item.Number.Value;
        }
    }

    class DexterityBoost : PermanentStatBoost
    {
        protected override void RaiseStat(InventoryItem item, CharacterSheet characterSheet)
        {
            characterSheet.BonusDexterity += item.Number.Value;
        }
    }

    class LuckBoost : PermanentStatBoost
    {
        protected override void RaiseStat(InventoryItem item, CharacterSheet characterSheet)
        {
            characterSheet.BonusLuck += item.Number.Value;
        }
    }
}
