using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using SoundPlugin;
using System;

namespace Adventure.Menu;

class SoundOptionsMenu
(
    IScaleHelper scaleHelper,
    GameOptions options,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    SoundState soundState
) : IExplorationSubMenu
{
    private const float SoundPercentConversion = 20f;

    private readonly SharpText masterVolumeText = new SharpText("Master Volume") { Color = Color.White };
    private readonly SharpSliderHorizontal masterVolumeSlider = new SharpSliderHorizontal() { Max = (int)(1f * SoundPercentConversion) };
    private readonly SharpButton back = new SharpButton() { Text = "Back" };

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
    {
        masterVolumeSlider.DesiredSize = scaleHelper.Scaled(new IntSize2(500, 35));

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(masterVolumeText, masterVolumeSlider, back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        sharpGui.Text(masterVolumeText);

        int volumePercent = (int)(options.MasterVolume * SoundPercentConversion);
        if (sharpGui.Slider(masterVolumeSlider, ref volumePercent, navUp: back.Id, navDown: back.Id))
        {
            var volumeFloat = (float)volumePercent / SoundPercentConversion;
            Console.WriteLine(volumeFloat);
            if(volumeFloat != options.MasterVolume)
            {
                options.MasterVolume = volumeFloat;
                soundState.MasterVolume = volumeFloat;
            }
        }

        if (sharpGui.Button(back, gamepadId, navUp: masterVolumeSlider.Id, navDown: masterVolumeSlider.Id) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            menu.RequestSubMenu(PreviousMenu, gamepadId);
            PreviousMenu = null;
        }
    }
}
