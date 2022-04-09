using Engine;
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
        }

        public bool Update()
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

            var hasInventoryRoom = sheet.HasRoom;

            if (hasInventoryRoom && sharpGui.Button(take, navUp: previous.Id, navDown: discard.Id))
            {
                currentTreasure.Pop();
                treasure.GiveTo(sheet.Inventory);
            }

            if (sharpGui.Button(discard, navUp: hasInventoryRoom ? take.Id : previous.Id, navDown: previous.Id))
            {
                currentTreasure.Pop();
            }

            var bottomNavDown = hasInventoryRoom ? take.Id : discard.Id;
            if (sharpGui.Button(previous, navUp: discard.Id, navDown: bottomNavDown, navLeft: next.Id, navRight: next.Id) || sharpGui.IsStandardPreviousPressed())
            {
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Current.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next, navUp: discard.Id, navDown: bottomNavDown, navLeft: previous.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed())
            {
                ++currentSheet;
                if (currentSheet >= persistence.Current.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }

            return false;
        }
    }
}
