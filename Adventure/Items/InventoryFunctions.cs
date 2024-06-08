using Adventure.Battle;
using Adventure.Items.Actions;
using Adventure.Services;
using Engine;
using Microsoft.Extensions.DependencyInjection;
using RpgMath;
using System;
using System.Xml.Linq;

namespace Adventure.Items;

interface IInventoryFunctions
{
    IInventoryAction CreateAction(InventoryItem item, Inventory inventory);

    void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target);

    ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer);

    void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target);
}

class InventoryFunctions : IInventoryFunctions
{
    private readonly IServiceProvider serviceProvider;

    public InventoryFunctions(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
    {
        var action = CreateAction(item, inventory);
        action.Use(item, inventory, attacker, target);
    }

    public ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
    {
        var action = CreateAction(item, inventory);
        return action.Use(item, inventory, attacker, target, characterMenuPositionService, objectResolver, coroutine, cameraMover, soundEffectPlayer);
    }

    public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
    {
        var action = CreateAction(item, inventory);
        action.Use(item, inventory, battleManager, objectResolver, coroutine, attacker, target);
    }

    public IInventoryAction CreateAction(InventoryItem item, Inventory inventory)
    {
        if (item.Action == null)
        {
            inventory.Items.Remove(item);
            return null;
        }

        Type type;

        switch (item.Action)
        {
            case nameof(EquipMainHand):
                type = typeof(EquipMainHand);
                break;
            case nameof(EquipOffHand):
                type = typeof(EquipOffHand);
                break;
            case nameof(EquipAccessory):
                type = typeof(EquipAccessory);
                break;
            case nameof(EquipBody):
                type = typeof(EquipBody);
                break;
            case nameof(LevelBoost):
                type = typeof(LevelBoost);
                break;
            case nameof(StrengthBoost):
                type = typeof(StrengthBoost);
                break;
            case nameof(MagicBoost):
                type = typeof(MagicBoost);
                break;
            case nameof(SpiritBoost):
                type = typeof(SpiritBoost);
                break;
            case nameof(VitalityBoost):
                type = typeof(VitalityBoost);
                break;
            case nameof(DexterityBoost):
                type = typeof(DexterityBoost);
                break;
            case nameof(LuckBoost):
                type = typeof(LuckBoost);
                break;

            case nameof(RestoreHp):
                type = typeof(RestoreHp);
                break;

            case nameof(RestoreMp):
                type = typeof(RestoreMp);
                break;

            case nameof(Revive):
                type = typeof(Revive);
                break;

            default: 
                throw new NotImplementedException(item.Action);
        }

        var action = serviceProvider.GetRequiredService(type) as IInventoryAction;

        return action;
    }
}
