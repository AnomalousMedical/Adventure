﻿using Adventure.Items;
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
) : IExplorationSubMenu, IDisposable
{
    private IObjectResolver objectResolver = objectResolverFactory.Create();
    private ISkillEffect currentEffect;

    public void Dispose()
    {
        objectResolver.Dispose();
    }

    public void GatherTreasures(IEnumerable<ITreasure> treasure, Action<ITreasure> treasureGivenCallback = null)
    {
        pickUpTreasureMenu.GatherTreasures(treasure, TimeSpan.FromMilliseconds(500),
        (ITreasure treasure, Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState) =>
        {
            currentEffect = treasure.Use(inventory, user, inventoryFunctions, gameState, characterMenuPositionService, objectResolver, coroutine, cameraMover, soundEffectPlayer);
        },
        cd =>
        {
            MoveCamera(cd.CharacterSheet);
        }, treasureGivenCallback: treasureGivenCallback);
    }

    public void Update(IExplorationMenu menu, GamepadId gamepad)
    {
        if (currentEffect != null)
        {
            currentEffect.Update(clockService.Clock);
            if (currentEffect.Finished)
            {
                currentEffect = null;
                if (pickUpTreasureMenu.HasMoreTreasure)
                {
                    pickUpTreasureMenu.RecenterCamera();
                }
            }
            return;
        }

        if (pickUpTreasureMenu.Update(gamepad, menu, this))
        {
            if (currentEffect == null)
            {
                menu.RequestSubMenu(null, gamepad);
                return;
            }
        }
    }

    private void MoveCamera(CharacterSheet characterSheet)
    {
        if (characterMenuPositionService.TryGetEntry(characterSheet, out var characterMenuPosition))
        {
            cameraMover.SetInterpolatedGoalPosition(characterMenuPosition.CameraPosition, characterMenuPosition.CameraRotation);
            characterMenuPosition.FaceCamera();
        }
    }
}
