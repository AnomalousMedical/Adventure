using Adventure.Items;
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
    record struct ButtonColumnItem<T>(String Text, T Item) { }

    class ButtonColumn
    {
        private List<SharpButton> buttons;

        public ButtonColumn(int numButtons)
        {
            buttons = new List<SharpButton>(numButtons);
            for(var i = 0; i < numButtons; i++)
            {
                buttons.Add(new SharpButton());
            }
        }

        public void Show<T>(ISharpGui sharpGui, IEnumerable<ButtonColumnItem<T>> items, Func<IntSize2, IntRect> GetLayoutPosition)
        {
            var layout =
               new MarginLayout(new IntPad(Margin),
               new MaxWidthLayout(MaxWidth,
               new ColumnLayout(buttons) { Margin = new IntPad(Margin) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(GetLayoutPosition(desiredSize));

            var firstButton = buttons[0];
            SharpButton lastButton;

            var buttonCount = buttons.Count;
            if (buttonCount > 0)
            {
                var previous = buttonCount - 1;
                var next = buttons.Count > 1 ? 1 : 0;
                int i = 0;

                foreach(var item in items)
                {
                    if(i >= buttonCount)
                    {
                        break;
                    }

                    var button = buttons[i];
                    button.Text = item.Text;

                    if (sharpGui.Button(button, navUp: buttons[previous].Id, navDown: buttons[next].Id))
                    {
                        
                    }

                    lastButton = buttons[i];

                    previous = i;
                    next = (i + 2) % buttonCount;

                    ++i;
                }
            }
        }

        public int Margin { get; set; }

        public int MaxWidth { get; set; }
    }

    class ItemMenu : IExplorationSubMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private ButtonColumn itemButtons = new ButtonColumn(5);
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
            var characterData = persistence.Party.Members[currentSheet];

            info.Rect = new IntRect(scaleHelper.Scaled(10), scaleHelper.Scaled(10), scaleHelper.Scaled(500), scaleHelper.Scaled(500));

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new RowLayout(previous, next), back) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            sharpGui.Text(info);
            info.Text = characterData.CharacterSheet.Name;

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Show(sharpGui, characterData.Inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(i.Name, i)), p => screenPositioner.GetCenterRect(p));

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
            if (sharpGui.Button(back) || sharpGui.IsStandardBackPressed())
            {
                menu.RequestSubMenu(menu.RootMenu);
            }
        }
    }
}
