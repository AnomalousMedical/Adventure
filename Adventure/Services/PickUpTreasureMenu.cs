using Adventure.Items;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class PickUpTreasureMenu
    {
        public const float ReplaceButtonsLayer = 0.15f;
        private static readonly InventoryItem CancelInventoryItem = new InventoryItem();

        private enum SaveBlocker { Treasure }

        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly IPersistenceWriter persistenceWriter;
        private Stack<ITreasure> currentTreasure;
        SharpButton take = new SharpButton() { Text = "Take" };
        SharpButton discard = new SharpButton() { Text = "Discard" };
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpText itemInfo = new SharpText() { Color = Color.White };
        SharpText currentCharacter = new SharpText() { Color = Color.White };
        SharpText inventoryInfo = new SharpText() { Color = Color.White };
        private int currentSheet;
        private bool replacingItem = false;

        private ButtonColumn replaceButtons = new ButtonColumn(25, ReplaceButtonsLayer);

        public PickUpTreasureMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            IPersistenceWriter persistenceWriter
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.persistenceWriter = persistenceWriter;
        }

        public void GatherTreasures(IEnumerable<ITreasure> treasure)
        {
            this.currentTreasure = new Stack<ITreasure>(treasure);
            persistenceWriter.AddSaveBlock(SaveBlocker.Treasure);
            replacingItem = false;
        }

        public bool Update(GamepadId gamepadId)
        {
            if (currentTreasure == null || currentTreasure.Count == 0)
            {
                persistenceWriter.RemoveSaveBlock(SaveBlocker.Treasure);
                persistenceWriter.Save();
                return true;
            }

            if (currentSheet > persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Current.Party.Members[currentSheet];
            var hasInventoryRoom = sheet.HasRoom;

            take.Text = hasInventoryRoom ? "Take" : "Replace";

            currentCharacter.Text = sheet.CharacterSheet.Name;
            inventoryInfo.Text = $"Items: {sheet.Inventory.Items.Count} / {sheet.CharacterSheet.InventorySize}";

            ILayoutItem layout;

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), next);
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), previous);
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            var treasure = currentTreasure.Peek();

            itemInfo.Text = treasure.InfoText;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(
                   new CenterHorizontalLayout(currentCharacter), 
                   new CenterHorizontalLayout(inventoryInfo),
                   new CenterHorizontalLayout(itemInfo),
                   new CenterHorizontalLayout(take), 
                   new CenterHorizontalLayout(discard)) 
               { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            layout.SetRect(screenPositioner.GetCenterTopRect(layout.GetDesiredSize(sharpGui)));

            sharpGui.Text(currentCharacter);
            sharpGui.Text(inventoryInfo);
            sharpGui.Text(itemInfo);

            if (sharpGui.Button(take, gamepadId, navUp: discard.Id, navDown: discard.Id, navLeft: previous.Id, navRight: next.Id))
            {
                if (hasInventoryRoom)
                {
                    currentTreasure.Pop();
                    treasure.GiveTo(sheet.Inventory);
                }
                else
                {
                    replacingItem = true;
                    sharpGui.StealFocus(replaceButtons.TopButton);
                }
            }

            if (sharpGui.Button(discard, gamepadId, navUp: take.Id, navDown: take.Id, navLeft: previous.Id, navRight: next.Id))
            {
                currentTreasure.Pop();
            }

            if (sharpGui.Button(previous, gamepadId, navLeft: next.Id, navRight: replacingItem ? replaceButtons.TopButton : take.Id) || sharpGui.IsStandardPreviousPressed(gamepadId))
            {
                replacingItem = false;
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Current.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next, gamepadId, navLeft: replacingItem ? replaceButtons.TopButton : take.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed(gamepadId))
            {
                replacingItem = false;
                ++currentSheet;
                if (currentSheet >= persistence.Current.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }

            replaceButtons.Margin = scaleHelper.Scaled(10);
            replaceButtons.MaxWidth = scaleHelper.Scaled(900);
            replaceButtons.Bottom = screenPositioner.ScreenSize.Height;

            if (replacingItem)
            {
                var removeItem = replaceButtons.Show(sharpGui, sheet.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(i.Name, i)).Append(new ButtonColumnItem<InventoryItem>("Cancel", CancelInventoryItem)), sheet.Inventory.Items.Count + 1, p => screenPositioner.GetCenterTopRect(p), gamepadId, navLeft: previous.Id, navRight: next.Id);
                if (removeItem != null)
                {
                    replacingItem = false;
                    if (removeItem != CancelInventoryItem)
                    {
                        sheet.RemoveItem(removeItem);
                        currentTreasure.Pop();
                        treasure.GiveTo(sheet.Inventory);
                    }
                }

                if (sharpGui.IsStandardBackPressed(gamepadId))
                {
                    replacingItem = false;
                }
            }

            return false;
        }
    }
}
