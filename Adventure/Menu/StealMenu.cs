using Adventure.Battle;
using Adventure.Items;
using Adventure.Services;
using Engine.Platform;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

class StealMenu
(
    PickUpTreasureMenu pickUpTreasureMenu
) : IExplorationSubMenu
{
    public void SetTreasure(IEnumerable<ITreasure> treasures, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker)
    {
        pickUpTreasureMenu.GatherTreasures(treasures, TimeSpan.FromSeconds(1), (ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState) =>
        {
            var useTarget = battleManager.GetTargetForStats(user);
            treasure.Use(inventory, inventoryFunctions, battleManager, objectResolver, coroutine, attacker, useTarget, gameState);
        });
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        if (pickUpTreasureMenu.Update(gamepadId, menu, this))
        {
            menu.RequestSubMenu(null, gamepadId);
        }
    }
}
