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
        private static Guid ScrollUp = Guid.NewGuid();
        private static Guid ScrollDown = Guid.NewGuid();

        private List<SharpButton> buttons;

        public ButtonColumn(int numButtons)
        {
            buttons = new List<SharpButton>(numButtons);
            for (var i = 0; i < numButtons; i++)
            {
                buttons.Add(new SharpButton());
            }
        }

        public void Show<T>(ISharpGui sharpGui, IEnumerable<ButtonColumnItem<T>> items, int itemCount, Func<IntSize2, IntRect> GetLayoutPosition)
        {
            var layout =
               new MarginLayout(new IntPad(Margin),
               new MaxWidthLayout(MaxWidth,
               new ColumnLayout(buttons) { Margin = new IntPad(Margin) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(GetLayoutPosition(desiredSize));

            var buttonCount = buttons.Count;
            if (buttonCount > 0)
            {
                var previous = buttonCount - 1;
                var next = buttons.Count > 1 ? 1 : 0;
                int i = 0;

                foreach (var item in items.Skip(CurrentIndex))
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

                    Guid navUpId = buttons[previous].Id;
                    if(i == 0)
                    {
                        navUpId = ScrollUp;
                    }

                    Guid navDownId = buttons[next].Id;
                    var nextIndex = i + 1;
                    var nextButton = nextIndex < buttons.Count ? buttons[nextIndex] : null;
                    if(nextButton == null || nextButton.Rect.Bottom > Bottom)
                    {
                        navDownId = ScrollDown;
                    }

                    button.Text = item.Text;

                    if (sharpGui.Button(button, navUp: navUpId, navDown: navDownId))
                    {

                    }

                    if(sharpGui.FocusedItem == ScrollUp)
                    {
                        sharpGui.StealFocus(button.Id);
                        --CurrentIndex;
                        if(CurrentIndex < 0)
                        {
                            CurrentIndex = 0;
                        }
                    }
                    else if (sharpGui.FocusedItem == ScrollDown)
                    {
                        sharpGui.StealFocus(button.Id);
                        ++CurrentIndex;
                        if(CurrentIndex + i >= itemCount)
                        {
                            CurrentIndex = itemCount - i - 1;
                        }
                    }

                    previous = i;
                    next = (i + 2) % buttonCount;

                    ++i;
                }
            }
        }

        public int Margin { get; set; }

        public int MaxWidth { get; set; }

        public int Bottom { get; set; } = int.MaxValue;

        public int CurrentIndex { get; set; }
    }
}
