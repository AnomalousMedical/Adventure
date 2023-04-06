using Adventure.Services;
using Microsoft.Extensions.DependencyInjection;
using RpgMath;
using System;
using System.Xml.Linq;

namespace Adventure.Items;

interface IInventoryFunctions
{
    IInventoryAction CreateAction(InventoryItem item, Inventory inventory);
    void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target);
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

    public IInventoryAction CreateAction(InventoryItem item, Inventory inventory)
    {
        if (item.Action == null)
        {
            inventory.Items.Remove(item);
            return null;
        }

        var type = Type.GetType($"Adventure.Items.Actions.{item.Action}");
        var action = serviceProvider.GetRequiredService(type) as IInventoryAction;

        return action;
    }
}
