using Engine;
using Engine.Platform;
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

        public ButtonColumn(int numButtons, float layer = 0f)
        {
            buttons = new List<SharpButton>(numButtons);
            for (var i = 0; i < numButtons; i++)
            {
                buttons.Add(new SharpButton() { Layer = layer });
            }
        }

        public T Show<T>(ISharpGui sharpGui, IEnumerable<ButtonColumnItem<T>> items, int itemCount, Func<IntSize2, IntRect> GetLayoutPosition, GamepadId gamepad, Guid? navLeft = null, Guid? navRight = null, SharpStyle style = null)
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

                foreach (var item in items.Skip(ListIndex))
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
                    if (i == 0)
                    {
                        navUpId = ScrollUp;
                    }

                    Guid navDownId = buttons[next].Id;
                    var nextIndex = i + 1;
                    var nextButton = nextIndex < buttons.Count ? buttons[nextIndex] : null;
                    if (nextButton == null || nextButton.Rect.Bottom > Bottom || nextIndex + ListIndex >= itemCount)
                    {
                        navDownId = ScrollDown;
                    }

                    button.Text = item.Text;

                    if (sharpGui.Button(button, gamepad, navUp: navUpId, navDown: navDownId, navLeft: navLeft, navRight: navRight, style: style))
                    {
                        return item.Item;
                    }

                    if (sharpGui.FocusedItem == ScrollUp)
                    {
                        sharpGui.StealFocus(button.Id);
                        --ListIndex;
                        if (ListIndex < 0)
                        {
                            ListIndex = 0;
                        }
                    }
                    else if (sharpGui.FocusedItem == ScrollDown)
                    {
                        sharpGui.StealFocus(button.Id);
                        ++ListIndex;
                        if (ListIndex + i >= itemCount)
                        {
                            ListIndex = itemCount - i - 1;
                        }
                    }

                    previous = i;
                    next = (i + 2) % buttonCount;

                    ++i;
                }
            }

            return default(T);
        }

        public void StealFocus(ISharpGui sharpGui)
        {
            if (!HasFocus(sharpGui))
            {
                FocusTop(sharpGui);
            }
        }

        public void FocusTop(ISharpGui sharpGui)
        {
            sharpGui.StealFocus(buttons[0].Id);
        }

        public bool HasFocus(ISharpGui sharpGui)
        {
            return buttons.Any(i => i.Id == sharpGui.FocusedItem);
        }

        public int Margin { get; set; }

        public int MaxWidth { get; set; }

        public int Bottom { get; set; } = int.MaxValue;

        public int ListIndex { get; set; }

        public int FocusedIndex(ISharpGui sharpGui)
        {
            return ListIndex + Math.Max(buttons.FindIndex(i => i.Id == sharpGui.FocusedItem), 0);
        }

        public int HoverIndex(ISharpGui sharpGui)
        {
            return ListIndex + Math.Max(buttons.FindIndex(i => i.Id == sharpGui.HoverItem), 0);
        }

        public Guid TopButton => buttons[0].Id;
    }
}
