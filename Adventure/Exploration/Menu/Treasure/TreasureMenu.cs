using Adventure.Services;
using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu.Treasure
{
    class TreasureMenu : IExplorationSubMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private Stack<ITreasure> currentTreasure;
        SharpButton take = new SharpButton() { Text = "Take" };
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Rect = new IntRect(10, 10, 500, 500) };
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
            if (currentTreasure.Count == 0)
            {
                menu.RequestSubMenu(null);
                return;
            }

            if (currentSheet > persistence.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Party.Members[currentSheet];

            take.Text = $"{sheet.CharacterSheet.Name} Take";

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(take, new RowLayout(previous, next), back) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));
            var treasure = currentTreasure.Peek();

            info.Text = treasure.InfoText;
            sharpGui.Text(info);

            if (sharpGui.Button(take))
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
