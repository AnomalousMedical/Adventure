using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu;

internal class KeybindMenu : IExplorationSubMenu
{
    private ButtonColumn itemButtons = new ButtonColumn(25);
    SharpButton close = new SharpButton() { Text = "Back" };
    SharpButton reset = new SharpButton() { Text = "Reset" };

    SharpText keyboard = new SharpText() { Text = "Keyboard", Color = Color.White };
    SharpText pad1 = new SharpText() { Text = "Pad 1", Color = Color.White };
    SharpText pad2 = new SharpText() { Text = "Pad 2", Color = Color.White };
    SharpText pad3 = new SharpText() { Text = "Pad 3", Color = Color.White };
    SharpText pad4 = new SharpText() { Text = "Pad 4", Color = Color.White };
    SharpText action = new SharpText() { Text = "Action", Color = Color.White };

    ILayoutItem headerLayout;

    private List<KeyBindings> keyBindingItems = new List<KeyBindings>();
    private List<ButtonColumnItem<KeyBindings?>> currentItems;

    public IExplorationSubMenu PreviousMenu { get; set; }

    private KeyBindings? selectedBinding;
    private SharpText rebindKeyText = new SharpText() { Color = Color.White };
    private SharpPanel panel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };
    private readonly KeybindService keybindService;
    private readonly ISharpGui sharpGui;
    private readonly ILanguageService languageService;
    private readonly IScaleHelper scaleHelper;
    private readonly IScreenPositioner screenPositioner;
    private readonly KeyboardMouseIcons keyboardMouseIcons;
    private readonly GamepadIcons gamepadIcons;
    private readonly ICoroutineRunner coroutine;
    private readonly ConfirmMenu confirmMenu;

    const int UnscaledColumnWidth = 150;
    const int UnscaledActionColumnWidth = 250;

    public KeybindMenu
    (
        KeybindService keybindService,
        ISharpGui sharpGui,
        ILanguageService languageService,
        IScaleHelper scaleHelper,
        IScreenPositioner screenPositioner,
        KeyboardMouseIcons keyboardMouseIcons,
        GamepadIcons gamepadIcons,
        ICoroutineRunner coroutine,
        ConfirmMenu confirmMenu
    )
    {
        this.keybindService = keybindService;
        this.sharpGui = sharpGui;
        this.languageService = languageService;
        this.scaleHelper = scaleHelper;
        this.screenPositioner = screenPositioner;
        this.keyboardMouseIcons = keyboardMouseIcons;
        this.gamepadIcons = gamepadIcons;
        this.coroutine = coroutine;
        this.confirmMenu = confirmMenu;

        headerLayout = new RowLayout(
            new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(keyboard)),
            new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad1)),
            new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad2)),
            new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad3)),
            new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad4)),
            new MinWidthLayout(scaleHelper.Scaled(UnscaledActionColumnWidth), new KeepWidthCenterLayout(action)));
    }

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

        var layout = new RowLayout(reset, close) { Margin = new IntPad(scaleHelper.Scaled(10)) };
        var backButtonRect = screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui));
        layout.SetRect(backButtonRect);

        itemButtons.MaxWidth = scaleHelper.Scaled(900);
        itemButtons.Bottom = backButtonRect.Top;

        if (currentItems == null)
        {
            keyBindingItems = keybindService.GetKeyBindings().ToList();
            currentItems = keyBindingItems.Select(i => new ButtonColumnItem<KeyBindings?>(languageService.Current.KeybindMenu.GetText(i), i)).ToList();
        }

        sharpGui.Panel(panel, panelStyle);

        var images = new List<SharpImage>();
        selectedBinding = itemButtons.Show(sharpGui, currentItems, currentItems.Count, p => screenPositioner.GetTopRightRect(p), gamepadId,
            wrapLayout: l => new PanelLayout(panel, new ColumnLayout(headerLayout, l) { Margin = new IntPad(0, 0, 0, scaleHelper.Scaled(10)) }),
            wrapItemLayout: i => CreateBindingRow(i, images),
            navUp: close.Id, navDown: close.Id);

        sharpGui.Text(keyboard);
        sharpGui.Text(pad1);
        sharpGui.Text(pad2);
        sharpGui.Text(pad3);
        sharpGui.Text(pad4);
        sharpGui.Text(action);

        foreach (var image in images)
        {
            if (image.Rect.Bottom > itemButtons.Bottom)
            {
                break;
            }
            sharpGui.Image(image);
        }

        if (sharpGui.Button(reset, gamepadId, navUp: itemButtons.BottomButton, navDown: itemButtons.TopButton, navLeft: close.Id, navRight: close.Id) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            coroutine.RunTask(async () =>
            {
                if(await confirmMenu.ShowAndWait("Are you sure you want to reset all bindings?", this, gamepadId))
                {
                    keybindService.Clear();
                }
            });
        }

        if (sharpGui.Button(close, gamepadId, navUp: itemButtons.BottomButton, navDown: itemButtons.TopButton, navLeft: reset.Id, navRight: reset.Id) || sharpGui.IsStandardBackPressed(gamepadId))
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
                { Margin = new IntPad(0, 0, 0, scaleHelper.Scaled(15)) };

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
                    layout.Add(new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(keyboardImage)));
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

                layout.Add(new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad1Image)));
                layout.Add(new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad2Image)));
                layout.Add(new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad3Image)));
                layout.Add(new MinWidthLayout(scaleHelper.Scaled(UnscaledColumnWidth), new KeepWidthCenterLayout(pad4Image)));

                if (images.Count > 0)
                {
                    layout.Add(new MinWidthLayout(scaleHelper.Scaled(UnscaledActionColumnWidth), j));
                    return layout;
                }
            }
            return j;
        });
    }

    public record Text
    (
        string Confirm,
        string Cancel,
        string ActiveAbility,
        string SwitchCharacter,
        string OpenMenu,
        string Previous,
        string Next,
        string Up,
        string Down,
        string Left,
        string Right,
        string MoveUp,
        string MoveDown,
        string MoveLeft,
        string MoveRight
    )
    {
        internal string GetText(KeyBindings i)
        {
            switch (i)
            {
                case KeyBindings.Confirm:
                    return Confirm;
                case KeyBindings.Cancel:
                    return Cancel;
                case KeyBindings.Previous:
                    return Previous;
                case KeyBindings.Next:
                    return Next;
                case KeyBindings.SwitchCharacter:
                    return SwitchCharacter;
                case KeyBindings.ActiveAbility:
                    return ActiveAbility;
                case KeyBindings.Up:
                    return Up;
                case KeyBindings.Down:
                    return Down;
                case KeyBindings.Left:
                    return Left;
                case KeyBindings.Right:
                    return Right;
                case KeyBindings.MoveUp:
                    return MoveUp;
                case KeyBindings.MoveDown:
                    return MoveDown;
                case KeyBindings.MoveLeft:
                    return MoveLeft;
                case KeyBindings.MoveRight:
                    return MoveRight;
                case KeyBindings.OpenMenu:
                    return OpenMenu;
            }
            return "MissingID_" + i;
        }
    }
}
