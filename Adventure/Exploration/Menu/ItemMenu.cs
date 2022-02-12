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
    class ItemMenu : IExplorationSubMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText();
        private int currentSheet;

        public ItemMenu
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

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            if (currentSheet > persistence.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Party.Members[currentSheet];

            info.Rect = new IntRect(scaleHelper.Scaled(10), scaleHelper.Scaled(10), scaleHelper.Scaled(500), scaleHelper.Scaled(500));

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new RowLayout(previous, next), back) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            sharpGui.Text(info);
            info.Text = sheet.CharacterSheet.Name;
            
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
            if(sharpGui.Button(back) || sharpGui.IsStandardBackPressed())
            {
                menu.RequestSubMenu(menu.RootMenu);
            }
        }
    }
}
