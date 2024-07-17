using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

internal class KeybindMenu
(
    KeybindService keybindService,
    ISharpGui sharpGui,
    ILanguageService languageService,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    KeyboardMouseIcons keyboardMouseIcons
) : IExplorationSubMenu
{
    private ButtonColumn itemButtons = new ButtonColumn(25);
    SharpButton close = new SharpButton() { Text = "Close" };

    private List<KeyBindings> keyBindingItems = new List<KeyBindings>();
    private List<ButtonColumnItem<KeyBindings>> currentItems;

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        var descriptionLayout =
          new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
          new ColumnLayout()
          {
              Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
          }
       );

        var layout = new RowLayout(close) { Margin = new IntPad(scaleHelper.Scaled(10)) };
        var backButtonRect = screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui));
        layout.SetRect(backButtonRect);

        itemButtons.Margin = scaleHelper.Scaled(10);
        itemButtons.MaxWidth = scaleHelper.Scaled(900);
        itemButtons.Bottom = backButtonRect.Top;

        if (currentItems == null)
        {
            keyBindingItems = keybindService.GetKeyBindings().ToList();
            currentItems = keyBindingItems.Select(i => new ButtonColumnItem<KeyBindings>(i.ToString(), i)).ToList();
        }
        var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
        var images = new List<SharpImage>();
        itemButtons.Show(sharpGui, currentItems, currentItems.Count, p => screenPositioner.GetTopRightRect(p), gamepadId,
            wrapLayout: l => new RowLayout(descriptionLayout, l) { Margin = new IntPad(scaleHelper.Scaled(10)) },
            wrapItemLayout: i =>
            {
                var topItem = itemButtons.ListIndex;
                var index = topItem;
                return i.Select<SharpButton, ILayoutItem>(j =>
                {
                    if (index < keyBindingItems.Count)
                    {
                        var keyBinding = keyBindingItems[index++];
                        var binding = keybindService.GetKeyboardMouseBinding(keyBinding);

                        if (binding.KeyboardButton != null)
                        {
                            var image = new SharpImage(keyboardMouseIcons.Icons)
                            {
                                UvRect = keyboardMouseIcons.GetButtonRect(binding.KeyboardButton.Value),
                                DesiredWidth = scaleHelper.Scaled(64),
                                DesiredHeight = scaleHelper.Scaled(64)
                            };

                            images.Add(image);
                            return new RowLayout(new KeepHeightLayout(image), j) { Margin = new IntPad(0, 0, scaleHelper.Scaled(15), 0) };
                        }
                        else if (binding.MouseButton != null)
                        {
                            var image = new SharpImage(keyboardMouseIcons.Icons)
                            {
                                UvRect = keyboardMouseIcons.GetButtonRect(binding.MouseButton.Value),
                                DesiredWidth = scaleHelper.Scaled(64),
                                DesiredHeight = scaleHelper.Scaled(64)
                            };

                            images.Add(image);
                            return new RowLayout(new KeepHeightLayout(image), j) { Margin = new IntPad(0, 0, scaleHelper.Scaled(15), 0) };
                        }
                    }
                    return j;
                });
            },
            navUp: close.Id, navDown: close.Id);

        foreach (var image in images)
        {
            if(image.Rect.Bottom > itemButtons.Bottom)
            {
                break;
            }
            sharpGui.Image(image);
        }

        if (sharpGui.Button(close, gamepadId, navUp: itemButtons.BottomButton, navDown: itemButtons.TopButton) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            currentItems = null;
            menu.RequestSubMenu(PreviousMenu, gamepadId);
        }
    }
}
