using Adventure.Assets.Equipment;
using Adventure.Items.Actions;
using RpgMath;
using System;
using System.Linq;

namespace Adventure.Items.Creators
{
    class BookCreator
    {
        private readonly IEquipmentCurve equipmentCurve;

        public BookCreator(IEquipmentCurve equipmentCurve)
        {
            this.equipmentCurve = equipmentCurve;
        }

        public InventoryItem CreateRestoration(int level, string adjective, params String[] spells)
        {
            return CreateRestoration(nameof(FancyBook), level, adjective, spells);
        }

        public InventoryItem CreateRestoration(String sprite, int level, string adjective, params String[] spells)
        {
            var book = new Equipment
            {
                Name = $"{adjective} Book of Restoration",
                MagicAttack = equipmentCurve.GetAttack(level),
                Sprite = sprite,
                Skills = spells.ToArray(),
                ShowHand = false,
            };

            return CreateInventoryItem(book);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand));
        }
    }
}
