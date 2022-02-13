using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    public record struct ButtonColumnItem<T>(String Text, T Item) { }

    public class ButtonColumn
    {
        private List<SharpButton> buttons;

        public ButtonColumn(int numButtons)
        {
            buttons = new List<SharpButton>(numButtons);
            for (var i = 0; i < numButtons; i++)
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

                foreach (var item in items)
                {
                    if (i >= buttonCount)
                    {
                        break;
                    }

                    var button = buttons[i];

                    if (button.Rect.Bottom > Bottom)
                    {
                        break;
                    }

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

        public int Bottom { get; set; } = int.MaxValue;
    }
}
