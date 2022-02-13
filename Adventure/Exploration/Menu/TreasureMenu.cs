using Adventure.Services;
using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    class TreasureMenu : IExplorationSubMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private Stack<ITreasure> currentTreasure;
        SharpButton take = new SharpButton();
        SharpButton store = new SharpButton() { Text = "Store" };
        SharpButton discard = new SharpButton() { Text = "Discard" };
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpText info = new SharpText();
        private int currentSheet;

        public TreasureMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
        }

        public void GatherTreasures(IEnumerable<ITreasure> treasure)
        {
            this.currentTreasure = new Stack<ITreasure>(treasure);
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            if (currentTreasure == null || currentTreasure.Count == 0)
            {
                menu.RequestSubMenu(null);
                return;
            }

            if (currentSheet > persistence.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Party.Members[currentSheet];

            take.Text = $"Take {sheet.CharacterSheet.Name}";
            info.Rect = new IntRect(scaleHelper.Scaled(10), scaleHelper.Scaled(10), scaleHelper.Scaled(500), scaleHelper.Scaled(500));

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(take, store, discard, new RowLayout(previous, next)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));
            var treasure = currentTreasure.Peek();

            info.Text = treasure.InfoText;
            sharpGui.Text(info);

            if (sheet.Inventory.HasRoom() && sharpGui.Button(take))
            {
                currentTreasure.Pop();
                treasure.GiveTo(sheet.Inventory);
            }

            if (persistence.Storage.HasRoom() && sharpGui.Button(store))
            {
                currentTreasure.Pop();
                treasure.GiveTo(persistence.Storage);
            }

            if (sharpGui.Button(discard))
            {
                currentTreasure.Pop();
            }

            if (sharpGui.Button(previous))
            {
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next))
            {
                ++currentSheet;
                if (currentSheet >= persistence.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }
        }
    }
}
