using Adventure.Items;
using Adventure.Services;
using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

class TreasureMenu
(
    PickUpTreasureMenu pickUpTreasureMenu,
    CharacterMenuPositionService characterMenuPositionService,
    CameraMover cameraMover
) : IExplorationSubMenu
{
    public void GatherTreasures(IEnumerable<ITreasure> treasure)
    {
        pickUpTreasureMenu.GatherTreasures(treasure, TimeSpan.FromMilliseconds(500),
        (ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState) =>
        {
            treasure.Use(inventory, user, inventoryFunctions, gameState);
        },
        cd =>
        {
            if(characterMenuPositionService.TryGetEntry(cd.CharacterSheet, out var characterMenuPosition))
            {
                cameraMover.SetInterpolatedGoalPosition(characterMenuPosition.CameraPosition, characterMenuPosition.CameraRotation);
                characterMenuPosition.FaceCamera();
            }
        });
    }

    public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepad)
    {
        if (pickUpTreasureMenu.Update(gamepad))
        {
            menu.RequestSubMenu(null, gamepad);
            return;
        }
    }
}
