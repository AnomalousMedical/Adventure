﻿using Adventure.Services;
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
    private List<ButtonColumnItem<KeyBindings?>> currentItems;

    public IExplorationSubMenu PreviousMenu { get; set; }

    private KeyBindings? selectedBinding;
    private SharpText rebindKeyText = new SharpText() { Color = Color.White };
    private SharpPanel panel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        if (selectedBinding != null)
        {
            rebindKeyText.Text = "Press a new button to assign to " + selectedBinding.ToString() + ".";

            var bindLayout =
                    new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                    new KeepWidthCenterLayout(new PanelLayout(panel, rebindKeyText)));

            bindLayout.SetRect(screenPositioner.GetCenterRect(bindLayout.GetDesiredSize(sharpGui)));

            sharpGui.Panel(panel, panelStyle);
            sharpGui.Text(rebindKeyText);

            if (sharpGui.KeyEntered != KeyboardButtonCode.KC_UNASSIGNED)
            {
                keybindService.SetBinding(selectedBinding.Value, new KeyboardMouseBinding(sharpGui.KeyEntered));
                selectedBinding = null;
            }
            return;
        }

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
            currentItems = keyBindingItems.Select(i => new ButtonColumnItem<KeyBindings?>(i.ToString(), i)).ToList();
        }
        
        var images = new List<SharpImage>();
        selectedBinding = itemButtons.Show(sharpGui, currentItems, currentItems.Count, p => screenPositioner.GetTopRightRect(p), gamepadId,
            wrapLayout: l => new RowLayout(descriptionLayout, l) { Margin = new IntPad(scaleHelper.Scaled(10)) },
            wrapItemLayout: i => CreateBindingRow(i, images),
            navUp: close.Id, navDown: close.Id);

        foreach (var image in images)
        {
            if (image.Rect.Bottom > itemButtons.Bottom)
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

    private IEnumerable<ILayoutItem> CreateBindingRow(IEnumerable<SharpButton> i, List<SharpImage> images)
    {
        var topItem = itemButtons.ListIndex;
        var index = topItem;
        return i.Select<SharpButton, ILayoutItem>(j =>
        {
            if (index < keyBindingItems.Count)
            {
                var keyBinding = keyBindingItems[index++];
                var binding = keybindService.GetKeyboardMouseBinding(keyBinding);

                SharpImage image = null;
                if (binding.KeyboardButton != null)
                {
                    image = new SharpImage(keyboardMouseIcons.Icons)
                    {
                        UvRect = keyboardMouseIcons.GetButtonRect(binding.KeyboardButton.Value),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                }
                else if (binding.MouseButton != null)
                {
                    image = new SharpImage(keyboardMouseIcons.Icons)
                    {
                        UvRect = keyboardMouseIcons.GetButtonRect(binding.MouseButton.Value),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                }

                if (image != null)
                {
                    images.Add(image);
                    return new RowLayout(new KeepHeightLayout(image), j) { Margin = new IntPad(0, 0, scaleHelper.Scaled(15), 0) };
                }
            }
            return j;
        });
    }
}
