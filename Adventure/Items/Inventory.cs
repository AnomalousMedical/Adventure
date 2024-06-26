﻿using Adventure.Battle;
using Adventure.Services;
using Adventure.Skills;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    class InventoryItem
    {
        public InventoryItem()
        {

        }

        public InventoryItem(Equipment equipment, String action)
        {
            this.Equipment = equipment;
            this.InfoId = equipment.InfoId;
            this.Action = action;
        }

        public String InfoId { get; set; }

        public String Action { get; set; }

        public Equipment Equipment { get; set; }

        public long? Number { get; set; }

        public bool CanUseOnPickup { get; set; }

        public bool Unique { get; set; }
    }

    interface IInventoryAction
    {
        bool AllowTargetChange => true;

        void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target);

        ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer);

        void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target);
    }

    class Inventory
    {
        public List<InventoryItem> Items { get; init; } = new List<InventoryItem>();
    }
}
