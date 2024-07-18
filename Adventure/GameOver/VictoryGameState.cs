using Adventure.Menu;
using Adventure.Services;
using Anomalous.OSPlatform;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;

namespace Adventure.GameOver;

interface IVictoryGameState : IGameState
{
}

class VictoryGameState
(
    ISharpGui sharpGui,
    RTInstances<EmptyScene> rtInstances,
    IScreenPositioner screenPositioner,
    Persistence persistence,
    IPersistenceWriter persistenceWriter,
    FontLoader fontLoader,
    FileMenu fileMenu,
    IExplorationMenu explorationMenu,
    App app,
    CreditsMenu creditsMenu
) : IVictoryGameState
{
    private SharpButton credits = new SharpButton() { Text = "Credits" };
    private SharpButton file = new SharpButton() { Text = "File" };
    private SharpButton exit = new SharpButton() { Text = "Exit" };
    private SharpText youWin = new SharpText("The End") { Color = Color.UIWhite, Font = fontLoader.TitleFont };
    private SharpText worldSeedText = new SharpText() { Color = Color.UIWhite };
    private SharpText clearTimeText = new SharpText() { Color = Color.UIWhite };
    private SharpText undefeatedText = new SharpText("Undefeated") { Color = Color.UIWhite };
    private SharpText oldSchoolText = new SharpText("Old School") { Color = Color.UIWhite };

    public RTInstances Instances => rtInstances;

    public void SetActive(bool active)
    {
        if (active)
        {
            if(persistence.Current.Time.ClearTime == null)
            {
                persistence.Current.Time.ClearTime = persistence.Current.Time.Total;
            }
            var time = TimeSpan.FromMilliseconds(persistence.Current.Time.ClearTime.Value * Clock.MicroToMilliseconds);
            worldSeedText.Text = "World Seed: " + persistence.Current.World.Seed;
            clearTimeText.Text = $"Cleared in: {(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";
            persistence.Current.Player.InWorld = true;
            persistenceWriter.Save();
        }
    }

    private IEnumerable<ILayoutItem> GetMenuItems()
    {
        yield return new KeepWidthCenterLayout(youWin);
        yield return new KeepWidthCenterLayout(worldSeedText);
        yield return new KeepWidthCenterLayout(clearTimeText);
        if (persistence.Current.Party.Undefeated)
        {
            yield return new KeepWidthCenterLayout(undefeatedText);
        }
        if (persistence.Current.Party.OldSchool)
        {
            yield return new KeepWidthCenterLayout(oldSchoolText);
        }
        yield return new KeepWidthCenterLayout(credits);
        yield return new KeepWidthCenterLayout(file);
        yield return new KeepWidthCenterLayout(exit);
    }

    public IGameState Update(Clock clock)
    {
        IGameState nextState = this;

        if (!explorationMenu.Update())
        {
            var layout = new ColumnLayout(GetMenuItems()) { Margin = new IntPad(10) };

            var size = layout.GetDesiredSize(sharpGui);
            layout.GetDesiredSize(sharpGui);
            var rect = screenPositioner.GetCenterRect(size);
            layout.SetRect(rect);

            if (persistence.Current.Party.Undefeated)
            {
                sharpGui.Text(undefeatedText);
            }
            if (persistence.Current.Party.OldSchool)
            {
                sharpGui.Text(oldSchoolText);
            }

            sharpGui.Text(youWin);
            sharpGui.Text(worldSeedText);
            sharpGui.Text(clearTimeText);

            if (sharpGui.Button(credits, GamepadId.Pad1, navUp: exit.Id, navDown: file.Id))
            {
                creditsMenu.Previous = null;
                explorationMenu.RequestSubMenu(creditsMenu, GamepadId.Pad1);
            }
            else if (sharpGui.Button(file, GamepadId.Pad1, navUp: credits.Id, navDown: exit.Id))
            {
                fileMenu.PreviousMenu = null;
                explorationMenu.RequestSubMenu(fileMenu, GamepadId.Pad1);
            }
            else if (sharpGui.Button(exit, GamepadId.Pad1, navUp: file.Id, navDown: credits.Id))
            {
                app.Exit();
            }
        }

        return nextState;
    }
}
