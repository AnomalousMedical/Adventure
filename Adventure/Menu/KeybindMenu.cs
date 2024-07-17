using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu;

internal class KeybindMenu
(
    KeybindService keybindService,
    ISharpGui sharpGui,
    ILanguageService languageService,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    KeyboardMouseIcons keyboardMouseIcons,
    GamepadIcons gamepadIcons
) : IExplorationSubMenu
{
    private ButtonColumn itemButtons = new ButtonColumn(25);
    SharpButton close = new SharpButton() { Text = "Back" };

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
            var isMovement = 
                 selectedBinding == KeyBindings.MoveUp
              || selectedBinding == KeyBindings.MoveDown
              || selectedBinding == KeyBindings.MoveLeft
              || selectedBinding == KeyBindings.MoveRight;

            rebindKeyText.Text = "Press a new button to assign to " + selectedBinding.ToString();

            if(isMovement)
            {
                rebindKeyText.Text += " (Keyboard Only)";
            }

            rebindKeyText.Text += ".";

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
            else if (!isMovement)
            {
                if(sharpGui.GamepadButtonEntered[0] != GamepadButtonCode.NUM_BUTTONS)
                {
                    keybindService.SetBinding(selectedBinding.Value, GamepadId.Pad1, sharpGui.GamepadButtonEntered[0]);
                    selectedBinding = null;
                }
                else if (sharpGui.GamepadButtonEntered[1] != GamepadButtonCode.NUM_BUTTONS)
                {
                    keybindService.SetBinding(selectedBinding.Value, GamepadId.Pad2, sharpGui.GamepadButtonEntered[1]);
                    selectedBinding = null;
                }
                else if (sharpGui.GamepadButtonEntered[2] != GamepadButtonCode.NUM_BUTTONS)
                {
                    keybindService.SetBinding(selectedBinding.Value, GamepadId.Pad3, sharpGui.GamepadButtonEntered[2]);
                    selectedBinding = null;
                }
                else if (sharpGui.GamepadButtonEntered[3] != GamepadButtonCode.NUM_BUTTONS)
                {
                    keybindService.SetBinding(selectedBinding.Value, GamepadId.Pad4, sharpGui.GamepadButtonEntered[3]);
                    selectedBinding = null;
                }
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
                var layout = new RowLayout()
                { Margin = new IntPad(0, 0, scaleHelper.Scaled(15), 0) };

                var keyBinding = keyBindingItems[index++];
                var binding = keybindService.GetKeyboardMouseBinding(keyBinding);

                SharpImage keyboardImage = null;
                if (binding.KeyboardButton != null)
                {
                    keyboardImage = new SharpImage(keyboardMouseIcons.Icons)
                    {
                        UvRect = keyboardMouseIcons.GetButtonRect(binding.KeyboardButton.Value),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                }
                else if (binding.MouseButton != null)
                {
                    keyboardImage = new SharpImage(keyboardMouseIcons.Icons)
                    {
                        UvRect = keyboardMouseIcons.GetButtonRect(binding.MouseButton.Value),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                }

                if (keyboardImage != null)
                {
                    images.Add(keyboardImage);
                    layout.Add(keyboardImage);
                }

                SharpImage pad1Image;
                SharpImage pad2Image;
                SharpImage pad3Image;
                SharpImage pad4Image;

                if (keyBinding == KeyBindings.MoveUp
                 || keyBinding == KeyBindings.MoveDown)
                {
                    pad1Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickYAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad2Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickYAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad3Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickYAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad4Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickYAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                }
                else if(keyBinding == KeyBindings.MoveLeft
                 || keyBinding == KeyBindings.MoveRight)
                {
                    pad1Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickXAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad2Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickXAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad3Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickXAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad4Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.LeftStickXAxis,
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                }
                else
                {
                    pad1Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.GetButtonRect(keybindService.GetGamepadBinding(keyBinding, GamepadId.Pad1)),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad2Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.GetButtonRect(keybindService.GetGamepadBinding(keyBinding, GamepadId.Pad2)),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad3Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.GetButtonRect(keybindService.GetGamepadBinding(keyBinding, GamepadId.Pad3)),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                    pad4Image = new SharpImage(gamepadIcons.Icons)
                    {
                        UvRect = gamepadIcons.GetButtonRect(keybindService.GetGamepadBinding(keyBinding, GamepadId.Pad4)),
                        DesiredWidth = scaleHelper.Scaled(64),
                        DesiredHeight = scaleHelper.Scaled(64)
                    };
                }

                images.Add(pad1Image);
                images.Add(pad2Image);
                images.Add(pad3Image);
                images.Add(pad4Image);

                layout.Add(pad1Image);
                layout.Add(pad2Image);
                layout.Add(pad3Image);
                layout.Add(pad4Image);

                if (images.Count > 0)
                {
                    layout.Add(j);
                    return layout;
                }
            }
            return j;
        });
    }
}
