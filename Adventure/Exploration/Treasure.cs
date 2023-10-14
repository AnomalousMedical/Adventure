using Adventure.Items;
using RpgMath;

namespace Adventure
{
    interface ITreasure
    {
        string InfoText { get; }

        bool CanUseOnPickup { get; }

        bool CanEquipOnPickup { get; }

        void GiveTo(Inventory inventory);

        void Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions);
    }

    class Treasure : ITreasure
    {
        private readonly InventoryItem inventoryItem;

        public string InfoText => inventoryItem.Name;

        public bool CanUseOnPickup => inventoryItem.CanUseOnPickup;

        public bool CanEquipOnPickup => inventoryItem.Equipment != null;

        public Treasure(InventoryItem inventoryItem)
        {
            this.inventoryItem = inventoryItem;
        }

        public void GiveTo(Inventory inventory)
        {
            inventory.Items.Insert(0, inventoryItem);
        }

        public void Use(Inventory inventory, CharacterSheet user, IInventoryFunctions inventoryFunctions)
        {
            inventoryFunctions.Use(this.inventoryItem, inventory, user, user);
        }
    }
}
