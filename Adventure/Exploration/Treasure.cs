using Adventure.Items;
using RpgMath;

namespace Adventure
{
    interface ITreasure
    {
        string InfoText { get; }

        bool CanUseOnPickup { get; }

        void GiveTo(Inventory inventory);

        void Use(Inventory inventory, CharacterSheet user);
    }

    class Treasure : ITreasure
    {
        private readonly InventoryItem inventoryItem;

        public string InfoText => inventoryItem.Name;

        public bool CanUseOnPickup => inventoryItem.CanUseOnPickup;

        public Treasure(InventoryItem inventoryItem)
        {
            this.inventoryItem = inventoryItem;
        }

        public void GiveTo(Inventory inventory)
        {
            inventory.Items.Add(inventoryItem);
        }

        public void Use(Inventory inventory, CharacterSheet user)
        {
            inventory.Use(this.inventoryItem, user, user);
        }
    }
}
