using Adventure.Battle;
using Adventure.Items;
using Adventure.Services;
using Adventure.Skills;
using Engine;
using RpgMath;

namespace Adventure
{
    enum TreasureType
    {
        Weapon = 0,
        OffHand = 1,
        Accessory = 2,
        Armor = 3,
        PlotItem = 4,
        StatBoost = 5,
        Potion = 6,
    }

    interface ITreasure
    {
        string InfoId { get; }

        bool CanUseOnPickup { get; }

        bool CanEquipOnPickup { get; }

        bool IsPlotItem { get; }

        void GiveTo(Inventory inventory, Persistence.GameState gameState);

        void Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState);

        ISkillEffect Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer);

        void Use(Inventory inventory, IInventoryFunctions inventoryFunctions, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, Persistence.GameState gameState);

        int? Id { get; }

        TreasureFortuneType FortuneText { get; }

        InventoryItem Item { get; }

        TreasureType TreasureType { get; }
    }

    class Treasure : ITreasure
    {
        private readonly InventoryItem inventoryItem;

        public string InfoId => inventoryItem.InfoId;

        public bool CanUseOnPickup => inventoryItem.CanUseOnPickup;

        public bool CanEquipOnPickup => inventoryItem.Equipment != null;

        public bool IsPlotItem => false;

        public int? Id { get; init; }

        public TreasureFortuneType FortuneText { get; init; }

        public InventoryItem Item => inventoryItem;

        public TreasureType TreasureType { get; init; }

        public Treasure(InventoryItem inventoryItem, TreasureType treasureType)
        {
            this.inventoryItem = inventoryItem;
            this.TreasureType = treasureType;
        }

        public void GiveTo(Inventory inventory, Persistence.GameState gameState)
        {
            inventory.Items.Insert(0, inventoryItem);
        }

        public void Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState)
        {
            inventoryFunctions.Use(this.inventoryItem, inventory, user, user);
        }

        public ISkillEffect Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            return inventoryFunctions.Use(this.inventoryItem, inventory, user, user, characterMenuPositionService, objectResolver, coroutine, cameraMover, soundEffectPlayer);
        }

        public void Use(Inventory inventory, IInventoryFunctions inventoryFunctions, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, Persistence.GameState gameState)
        {
            inventoryFunctions.Use(this.inventoryItem, inventory, battleManager, objectResolver, coroutine, attacker, target);
        }
    }

    class PlotItemTreasure : ITreasure
    {
        private readonly PlotItems plotItem;

        public string InfoId { get; init; }

        public bool CanUseOnPickup => false;

        public bool CanEquipOnPickup => false;

        public bool IsPlotItem => true;

        public int? Id { get; init; }

        public TreasureFortuneType FortuneText { get; init; }

        public InventoryItem Item => null;

        public TreasureType TreasureType { get; init; }

        public PlotItemTreasure(PlotItems plotItem, string infoId)
        {
            this.InfoId = infoId;
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

        public ISkillEffect Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions, Persistence.GameState gameState, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            gameState.PlotItems.Add(plotItem);
            return null;
        }

        public void Use(Inventory inventory, IInventoryFunctions inventoryFunctions, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, Persistence.GameState gameState)
        {
            gameState.PlotItems.Add(plotItem);
        }
    }
}
