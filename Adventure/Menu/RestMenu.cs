using Adventure.Assets.SoundEffects;
using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Adventure.Services.Persistence;

namespace Adventure.Menu;

internal class RestMenu
(
    IScaleHelper scaleHelper,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    RestManager restManager,
    IExplorationMenu explorationMenu
): IExplorationSubMenu
{
    SharpText prompt = new SharpText() { Text = "Rest Until", Color = Color.White, Layer = BuyMenu.UseItemMenuLayer };
    SharpButton dawn = new SharpButton() { Text = "Dawn", Layer = BuyMenu.UseItemMenuLayer };
    SharpButton noon = new SharpButton() { Text = "Noon", Layer = BuyMenu.UseItemMenuLayer };
    SharpButton dusk = new SharpButton() { Text = "Dusk", Layer = BuyMenu.UseItemMenuLayer };
    SharpButton midnight = new SharpButton() { Text = "Midnight", Layer = BuyMenu.UseItemMenuLayer };
    SharpButton cancel = new SharpButton() { Text = "Cancel", Layer = BuyMenu.UseItemMenuLayer };
    private SharpPanel promptPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

    public void Show(GamepadId gamepadId)
    {
        explorationMenu.RequestSubMenu(this, gamepadId);
    }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new ColumnLayout(new KeepWidthCenterLayout(new PanelLayout(promptPanel, prompt)),
                   new KeepWidthCenterLayout(new ColumnLayout(dawn, noon, dusk, midnight, cancel) { Margin = new IntPad(scaleHelper.Scaled(10)) })
            ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetCenterTopRect(desiredSize));
        sharpGui.Panel(promptPanel, panelStyle);
        sharpGui.Text(prompt);

        if (sharpGui.Button(dawn, gamepadId, navUp: cancel.Id, navDown: noon.Id))
        {
            restManager.Rest(RestManager.RestTarget.Dawn);
            FireClosed(menu, gamepadId);
        }
        if (sharpGui.Button(noon, gamepadId, navUp: dawn.Id, navDown: dusk.Id))
        {
            restManager.Rest(RestManager.RestTarget.Noon);
            FireClosed(menu, gamepadId);
        }
        if (sharpGui.Button(dusk, gamepadId, navUp: noon.Id, navDown: midnight.Id))
        {
            restManager.Rest(RestManager.RestTarget.Dusk);
            FireClosed(menu, gamepadId);
        }
        if (sharpGui.Button(midnight, gamepadId, navUp: dusk.Id, navDown: cancel.Id))
        {
            restManager.Rest(RestManager.RestTarget.Midnight);
            FireClosed(menu, gamepadId);
        }
        if (sharpGui.Button(cancel, gamepadId, navUp: midnight.Id, navDown: dawn.Id) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            FireClosed(menu, gamepadId);
        }
    }

    private void FireClosed(IExplorationMenu menu, GamepadId gamepadId)
    {
        menu.RequestSubMenu(null, gamepadId);
    }
}
