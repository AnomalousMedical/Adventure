using Adventure.Items;
using Adventure.Services;
using Adventure.Skills;
using Engine;
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
    CameraMover cameraMover,
    IObjectResolverFactory objectResolverFactory,
    IScopedCoroutine coroutine,
    ISoundEffectPlayer soundEffectPlayer,
    IClockService clockService
) : IExplorationSubMenu
{
    private IObjectResolver objectResolver = objectResolverFactory.Create();
    private ISkillEffect currentEffect;

    public void GatherTreasures(IEnumerable<ITreasure> treasure)
    {
        pickUpTreasureMenu.GatherTreasures(treasure, TimeSpan.FromMilliseconds(500),
        (ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState) =>
        {
            currentEffect = treasure.Use(inventory, user, inventoryFunctions, gameState, characterMenuPositionService, objectResolver, coroutine, cameraMover, soundEffectPlayer);
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

    public void Update(IExplorationMenu menu, GamepadId gamepad)
    {
        if (currentEffect != null)
        {
            currentEffect.Update(clockService.Clock);
            if (currentEffect.Finished)
            {
                currentEffect = null;
            }
            return;
        }

        if (pickUpTreasureMenu.Update(gamepad))
        {
            if (currentEffect == null)
            {
                menu.RequestSubMenu(null, gamepad);
                return;
            }
        }
    }
}
