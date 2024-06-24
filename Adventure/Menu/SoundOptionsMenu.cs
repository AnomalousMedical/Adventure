using Adventure.Assets.SoundEffects;
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
    SoundState soundState,
    IBackgroundMusicPlayer backgroundMusicPlayer,
    ISoundEffectPlayer soundEffectPlayer
) : IExplorationSubMenu
{
    private const float SoundPercentConversion = 20f;

    private readonly SharpText masterVolumeText = new SharpText("Master Volume") { Color = Color.White };
    private readonly SharpSliderHorizontal masterVolumeSlider = new SharpSliderHorizontal() { Max = (int)(1f * SoundPercentConversion) };

    private readonly SharpText musicVolumeText = new SharpText("Music Volume") { Color = Color.White };
    private readonly SharpSliderHorizontal musicVolumeSlider = new SharpSliderHorizontal() { Max = (int)(1f * SoundPercentConversion) };

    private readonly SharpText sfxVolumeText = new SharpText("Sound Effect Volume") { Color = Color.White };
    private readonly SharpSliderHorizontal sfxVolumeSlider = new SharpSliderHorizontal() { Max = (int)(1f * SoundPercentConversion) };

    private readonly SharpButton testSound = new SharpButton() { Text = "Test Sound Effect" };
    private readonly SharpButton back = new SharpButton() { Text = "Back" };

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        masterVolumeSlider.DesiredSize = scaleHelper.Scaled(new IntSize2(500, 35));
        musicVolumeSlider.DesiredSize = scaleHelper.Scaled(new IntSize2(500, 35));
        sfxVolumeSlider.DesiredSize = scaleHelper.Scaled(new IntSize2(500, 35));

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(masterVolumeText, masterVolumeSlider, musicVolumeText, musicVolumeSlider, sfxVolumeText, sfxVolumeSlider, testSound, back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        sharpGui.Text(masterVolumeText);

        int volumePercent = (int)(options.MasterVolume * SoundPercentConversion);
        if (sharpGui.Slider(masterVolumeSlider, ref volumePercent, navUp: back.Id, navDown: musicVolumeSlider.Id))
        {
            var volumeFloat = (float)volumePercent / SoundPercentConversion;
            if(volumeFloat != options.MasterVolume)
            {
                options.MasterVolume = volumeFloat;
                soundState.MasterVolume = volumeFloat;
            }
        }

        sharpGui.Text(musicVolumeText);

        volumePercent = (int)(options.MusicVolume * SoundPercentConversion);
        if (sharpGui.Slider(musicVolumeSlider, ref volumePercent, navUp: masterVolumeSlider.Id, navDown: sfxVolumeSlider.Id))
        {
            var volumeFloat = (float)volumePercent / SoundPercentConversion;
            if (options.MusicVolume != volumeFloat)
            {
                options.MusicVolume = volumeFloat;
                backgroundMusicPlayer.UpdateCurrentVolume(options.MusicVolume);
            }
        }

        sharpGui.Text(sfxVolumeText);

        volumePercent = (int)(options.SfxVolume * SoundPercentConversion);
        if (sharpGui.Slider(sfxVolumeSlider, ref volumePercent, navUp: musicVolumeSlider.Id, navDown: testSound.Id))
        {
            var volumeFloat = (float)volumePercent / SoundPercentConversion;
            options.SfxVolume = volumeFloat;
        }

        if (sharpGui.Button(testSound, gamepadId, navUp: sfxVolumeSlider.Id, navDown: back.Id))
        {
            soundEffectPlayer.PlaySound(HeavyHammerSoundEffect.Instance);
        }

        if (sharpGui.Button(back, gamepadId, navUp: testSound.Id, navDown: masterVolumeSlider.Id) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            menu.RequestSubMenu(PreviousMenu, gamepadId);
            PreviousMenu = null;
        }
    }
}
