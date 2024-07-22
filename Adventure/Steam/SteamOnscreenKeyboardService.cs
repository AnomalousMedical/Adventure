using Adventure.Services;
using Engine.Platform;
using SharpGui;
using Steamworks;

namespace Adventure.Steam;

internal class SteamOnscreenKeyboardService : IOnscreenKeyboardService
{
    public void ShowKeyboard(RequestedOnscreenKeyboardMode mode, int x, int y, int width, int height)
    {
        EFloatingGamepadTextInputMode steamMode;
        switch (mode)
        {
            default:
            case RequestedOnscreenKeyboardMode.SingleLine:
                steamMode = EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine;
                break;
            case RequestedOnscreenKeyboardMode.MultipleLines:
                steamMode = EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeMultipleLines;
                break;
            case RequestedOnscreenKeyboardMode.Numeric:
                steamMode = EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeNumeric;
                break;
            case RequestedOnscreenKeyboardMode.Email:
                steamMode = EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeEmail;
                break;
        }
        
        //This just forces the keyboard to the bottom of the screen. Steam puts it at the top if you pass the coords.
        //There is only 1 position for these in the game, so this should be ok.
        SteamUtils.ShowFloatingGamepadTextInput(steamMode, 0, 0, 1, 1);
    }
}
