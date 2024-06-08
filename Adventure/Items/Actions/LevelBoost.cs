using Adventure.Battle;
using Adventure.Services;
using Engine;
using RpgMath;

namespace Adventure.Items.Actions
{
    internal class LevelBoost : IInventoryAction
    {
        private readonly ILevelCalculator levelCalculator;

        public LevelBoost(ILevelCalculator levelCalculator)
        {
            this.levelCalculator = levelCalculator;
        }

        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            LevelUpSheet(item, target);
        }

        public ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            inventory.Items.Remove(item);

            LevelUpSheet(item, target);

            return null;
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item); //If this is used on an enemy it does nothing, but its still lost

            if (target.BattleTargetType == BattleTargetType.Player)
            {
                var characterSheet = target.Stats as CharacterSheet;
                LevelUpSheet(item, characterSheet);
            }
        }

        private void LevelUpSheet(InventoryItem item, CharacterSheet characterSheet)
        {
            for (int i = 0; i < item.Number; ++i)
            {
                characterSheet.LevelUp(levelCalculator);
            }
        }
    }
}
