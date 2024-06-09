using Adventure.Services;
using Anomalous.OSPlatform;
using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Microsoft.Extensions.Options;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Menu;

internal class GraphicsOptionsMenu
(
    IScaleHelper scaleHelper,
    GameOptions options,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    NativeOSWindow nativeOSWindow,
    RTImageBlitter imageBlitter,
    DiligentEngineOptions diligentEngineOptions
) : IExplorationSubMenu
{
    private readonly SharpButton toggleFullscreen = new SharpButton();
    private readonly SharpButton toggleUpsampling = new SharpButton();
    private readonly SharpButton back = new SharpButton() { Text = "Back" };

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
    {
        toggleFullscreen.Text = options.Fullscreen ? "Fullscreen" : "Windowed";
        switch (options.UpsamplingMethod)
        {
            case UpsamplingMethod.None:
                toggleUpsampling.Text = "No Upsampling";
                break;
            case UpsamplingMethod.FSR1:
                toggleUpsampling.Text = "FSR 1";
                break;
        }
        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(toggleFullscreen, toggleUpsampling, back) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        if (sharpGui.Button(toggleFullscreen, gamepadId, navUp: back.Id, navDown: toggleUpsampling.Id))
        {
            options.Fullscreen = !options.Fullscreen;
            nativeOSWindow.toggleFullscreen();
            if (!options.Fullscreen)
            {
                nativeOSWindow.Maximized = true;
            }
        }

        if (sharpGui.Button(toggleUpsampling, gamepadId, navUp: toggleFullscreen.Id, navDown: back.Id))
        {
            switch (options.UpsamplingMethod)
            {
                case UpsamplingMethod.None:
                    options.UpsamplingMethod = UpsamplingMethod.FSR1;
                    break;
                case UpsamplingMethod.FSR1:
                    options.UpsamplingMethod = UpsamplingMethod.None;
                    break;
            }
            diligentEngineOptions.UpsamplingMethod = options.UpsamplingMethod;
            imageBlitter.RecreateBuffer();
        }

        if (sharpGui.Button(back, gamepadId, navUp: toggleUpsampling.Id, navDown: toggleFullscreen.Id) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            Close(menu, gamepadId);
        }
    }

    private void Close(IExplorationMenu menu, GamepadId gamepadId)
    {
        menu.RequestSubMenu(PreviousMenu, gamepadId);
        PreviousMenu = null;
    }
}
