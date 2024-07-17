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

        private static Guid currentBottomButton;

        private List<SharpButton> buttons;

        public ButtonColumn(int numButtons, float layer = 0f)
        {
            buttons = new List<SharpButton>(numButtons);
            for (var i = 0; i < numButtons; i++)
            {
                buttons.Add(new SharpButton() { Layer = layer });
            }
        }

        public T Show<T>(ISharpGui sharpGui, IEnumerable<ButtonColumnItem<T>> items, int itemCount, Func<IntSize2, IntRect> GetLayoutPosition, GamepadId gamepad, Guid? navLeft = null, Guid? navRight = null, SharpStyle style = null, Func<ILayoutItem, ILayoutItem> wrapLayout = null, Guid? navUp = null, Guid? navDown = null, Func<IEnumerable<SharpButton>, IEnumerable<ILayoutItem>> wrapItemLayout = null)
        {
            IEnumerable<ILayoutItem> wrappedButtons = buttons;
            if(wrapItemLayout != null)
            {
                wrappedButtons = wrapItemLayout(buttons).ToList();
            }

            ILayoutItem layout =
               new MarginLayout(new IntPad(Margin),
               new MaxWidthLayout(MaxWidth,
               new ColumnLayout(wrappedButtons) { Margin = new IntPad(Margin) }
            ));

            if(wrapLayout != null)
            {
                layout = wrapLayout(layout);
            }

            var desiredSize = layout.GetDesiredSize(sharpGui);
            var layoutRect = GetLayoutPosition(desiredSize);
            if(layoutRect.Bottom > Bottom)
            {
                layoutRect.Height = Bottom - layoutRect.Top;
            }
            layout.SetRect(layoutRect);

            var result = default(T);

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

                    currentBottomButton = button.Id;

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
                        result = item.Item;
                    }

                    if (sharpGui.FocusedItem == ScrollUp)
                    {
                        sharpGui.StealFocus(button.Id);
                        --ListIndex;
                        if (ListIndex < 0)
                        {
                            ListIndex = 0;
                            if(navUp != null)
                            {
                                sharpGui.StealFocus(navUp.Value);
                            }
                        }
                    }
                    else if (sharpGui.FocusedItem == ScrollDown)
                    {
                        sharpGui.StealFocus(button.Id);
                        ++ListIndex;
                        if (ListIndex + i >= itemCount)
                        {
                            ListIndex = itemCount - i - 1;
                            if(navDown != null)
                            {
                                sharpGui.StealFocus(navDown.Value);
                            }
                        }
                    }

                    previous = i;
                    next = (i + 2) % buttonCount;

                    ++i;
                }
            }

            return result;
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

        public Guid BottomButton => currentBottomButton;
    }
}
