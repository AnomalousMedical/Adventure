using Adventure.Battle;
using Adventure.Items;
using Adventure.Services;
using Engine;
using RpgMath;

namespace Adventure
{
    interface ITreasure
    {
        string InfoText { get; }

        bool CanUseOnPickup { get; }

        bool CanEquipOnPickup { get; }

        bool IsPlotItem { get; }

        void GiveTo(Inventory inventory, Persistence.GameState gameState);

        void Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState);

        void Use(Inventory inventory, IInventoryFunctions inventoryFunctions, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, Persistence.GameState gameState);

        int? Id { get; }

        string FortuneText { get; }
    }

    class Treasure : ITreasure
    {
        private readonly InventoryItem inventoryItem;

        public string InfoText => inventoryItem.Name;

        public bool CanUseOnPickup => inventoryItem.CanUseOnPickup;

        public bool CanEquipOnPickup => inventoryItem.Equipment != null;

        public bool IsPlotItem => false;

        public int? Id { get; init; }

        public string FortuneText { get; init; }

        public Treasure(InventoryItem inventoryItem)
        {
            this.inventoryItem = inventoryItem;
        }

        public void GiveTo(Inventory inventory, Persistence.GameState gameState)
        {
            inventory.Items.Insert(0, inventoryItem);
        }

        public void Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState)
        {
            inventoryFunctions.Use(this.inventoryItem, inventory, user, user);
        }

        public void Use(Inventory inventory, IInventoryFunctions inventoryFunctions, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, Persistence.GameState gameState)
        {
            inventoryFunctions.Use(this.inventoryItem, inventory, battleManager, objectResolver, coroutine, attacker, target);
        }
    }

    class PlotItemTreasure : ITreasure
    {
        private readonly PlotItems plotItem;

        public string InfoText { get; init; }

        public bool CanUseOnPickup => false;

        public bool CanEquipOnPickup => false;

        public bool IsPlotItem => true;

        public int? Id { get; init; }

        public string FortuneText { get; init; }

        public PlotItemTreasure(PlotItems plotItem, string infoText)
        {
            this.InfoText = infoText;
            this.plotItem = plotItem;
        }

        public void GiveTo(Inventory inventory, Persistence.GameState gameState)
        {
            gameState.PlotItems.Add(plotItem);
        }

        public void Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState)
        {
            gameState.PlotItems.Add(plotItem);
        }

        public void Use(Inventory inventory, IInventoryFunctions inventoryFunctions, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, Persistence.GameState gameState)
        {
            gameState.PlotItems.Add(plotItem);
        }
    }
}
