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
    private readonly SharpButton togglePresent = new SharpButton();
    private readonly SharpButton toggleRenderApi = new SharpButton();
    private readonly SharpButton back = new SharpButton() { Text = "Back" };

    private const float FSRPercentConversion = 10f;
    private readonly SharpSliderHorizontal fsrPercentSlider = new SharpSliderHorizontal() { Max = (int)(0.9f * FSRPercentConversion) };

    private SharpText restartRequired = new SharpText("Restart Required") { Color = Color.UIWhite };

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        var items = new List<ILayoutItem>() { toggleFullscreen, togglePresent, toggleUpsampling };
        var showFsrSlider = false;
        var showRestartRequired = options.RenderApi != diligentEngineOptions.RenderApi;

        toggleFullscreen.Text = options.Fullscreen ? "Fullscreen" : "Windowed";
        switch (options.UpsamplingMethod)
        {
            case UpsamplingMethod.None:
                toggleUpsampling.Text = "No Upsampling";
                break;
            case UpsamplingMethod.FSR1:
                toggleUpsampling.Text = "FSR 1";
                items.Add(fsrPercentSlider);
                fsrPercentSlider.DesiredSize = scaleHelper.Scaled(new IntSize2(500, 35));
                showFsrSlider = true;
                break;
        }

        togglePresent.Text = "Present " + options.PresentInterval;

        switch (options.RenderApi)
        {
            case GraphicsEngine.RenderApi.D3D12:
                toggleRenderApi.Text = "D3D12";
                break;
            case GraphicsEngine.RenderApi.Vulkan:
                toggleRenderApi.Text = "Vulkan";
                break;
        }
        items.Add(toggleRenderApi);

        if (showRestartRequired)
        {
            items.Add(restartRequired);
        }

        items.Add(back);

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(items) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        if (sharpGui.Button(toggleFullscreen, gamepadId, navUp: back.Id, navDown: togglePresent.Id))
        {
            options.Fullscreen = !options.Fullscreen;
            nativeOSWindow.toggleFullscreen();
            if (!options.Fullscreen)
            {
                nativeOSWindow.Maximized = true;
            }
        }

        if (sharpGui.Button(togglePresent, gamepadId, navUp: toggleFullscreen.Id, navDown: toggleUpsampling.Id))
        {
            options.PresentInterval = (options.PresentInterval + 1) % 5;
        }

        if (sharpGui.Button(toggleUpsampling, gamepadId, navUp: togglePresent.Id, navDown: showFsrSlider ? fsrPercentSlider.Id : toggleRenderApi.Id))
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

        if (showFsrSlider)
        {
            int fsrPercent = (int)((diligentEngineOptions.FSR1RenderPercentage - 0.1f) * FSRPercentConversion);
            if (sharpGui.Slider(fsrPercentSlider, ref fsrPercent, navUp: toggleUpsampling.Id, navDown: toggleRenderApi.Id))
            {
                var fsrPercentFloat = (float)fsrPercent / FSRPercentConversion + 0.1f;
                if (fsrPercentFloat != options.FSR1RenderPercentage)
                {
                    options.FSR1RenderPercentage = fsrPercentFloat;
                    diligentEngineOptions.FSR1RenderPercentage = fsrPercentFloat;
                    imageBlitter.RecreateBuffer();
                }
            }
        }

        if (sharpGui.Button(toggleRenderApi, gamepadId, navUp: showFsrSlider ? fsrPercentSlider.Id : toggleUpsampling.Id, navDown: back.Id))
        {
            switch (options.RenderApi)
            {
                case GraphicsEngine.RenderApi.D3D12:
                    options.RenderApi = GraphicsEngine.RenderApi.Vulkan;
                    break;
                case GraphicsEngine.RenderApi.Vulkan:
                    options.RenderApi = GraphicsEngine.RenderApi.D3D12;
                    break;
            }
        }

        if (showRestartRequired)
        {
            sharpGui.Text(restartRequired);
        }

        if (sharpGui.Button(back, gamepadId, navUp: toggleRenderApi.Id, navDown: toggleFullscreen.Id) || sharpGui.IsStandardBackPressed(gamepadId))
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
