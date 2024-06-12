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

        public InventoryItem CreateRestoration(String sprite, int level, string infoId, params String[] spells)
        {
            var book = new Equipment
            {
                InfoId = infoId,
                MagicAttack = equipmentCurve.GetAttack(level),
                Sprite = sprite,
                Skills = spells.ToArray(),
                ShowHand = false,
            };

            return CreateInventoryItem(book);
        }

        private InventoryItem CreateInventoryItem(Equipment equipment)
        {
            return new InventoryItem(equipment, nameof(EquipOffHand))
            {
                Unique = true
            };
        }
    }
}
