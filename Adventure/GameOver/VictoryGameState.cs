using Adventure.Menu;
using Adventure.Services;
using Anomalous.OSPlatform;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using SharpGui;
using System;

namespace Adventure.GameOver;

interface IVictoryGameState : IGameState
{
}

class VictoryGameState : IVictoryGameState
{
    private readonly ISharpGui sharpGui;
    private readonly RTInstances rtInstances;
    private readonly IScreenPositioner screenPositioner;
    private readonly Persistence persistence;
    private readonly IPersistenceWriter persistenceWriter;
    private readonly FileMenu fileMenu;
    private readonly IExplorationMenu explorationMenu;
    private readonly App app;
    private SharpButton file = new SharpButton() { Text = "File" };
    private SharpButton exit = new SharpButton() { Text = "Exit" };
    private SharpText youWin = new SharpText("You Win") { Color = Color.White };
    private SharpText clearTimeText = new SharpText() { Color = Color.White };
    private ILayoutItem layout;

    public RTInstances Instances => rtInstances;

    public VictoryGameState
    (
        ISharpGui sharpGui,
        RTInstances<EmptyScene> rtInstances,
        IScreenPositioner screenPositioner,
        Persistence persistence,
        IPersistenceWriter persistenceWriter,
        FontLoader fontLoader,
        FileMenu fileMenu,
        IExplorationMenu explorationMenu,
        App app
    )
    {
        youWin.Font = fontLoader.TitleFont;

        this.sharpGui = sharpGui;
        this.rtInstances = rtInstances;
        this.screenPositioner = screenPositioner;
        this.persistence = persistence;
        this.persistenceWriter = persistenceWriter;
        this.fileMenu = fileMenu;
        this.explorationMenu = explorationMenu;
        this.app = app;
        layout = new ColumnLayout(new KeepWidthCenterLayout(youWin), new KeepWidthCenterLayout(clearTimeText), file, exit) { Margin = new IntPad(10) };
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            if(persistence.Current.Time.ClearTime == null)
            {
                persistence.Current.Time.ClearTime = persistence.Current.Time.Total;
            }
            var time = TimeSpan.FromMilliseconds(persistence.Current.Time.ClearTime.Value * Clock.MicroToMilliseconds);
            clearTimeText.Text = $"Cleared in: {(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";
            persistence.Current.Player.InWorld = true;
            persistenceWriter.Save();
        }
    }

    public IGameState Update(Clock clock)
    {
        IGameState nextState = this;

        if (!explorationMenu.Update())
        {
            var size = layout.GetDesiredSize(sharpGui);
            layout.GetDesiredSize(sharpGui);
            var rect = screenPositioner.GetCenterRect(size);
            layout.SetRect(rect);

            sharpGui.Text(youWin);
            sharpGui.Text(clearTimeText);

            if (sharpGui.Button(file, GamepadId.Pad1, navUp: exit.Id, navDown: exit.Id))
            {
                fileMenu.PreviousMenu = null;
                explorationMenu.RequestSubMenu(fileMenu, GamepadId.Pad1);
            }
            else if (sharpGui.Button(exit, GamepadId.Pad1, navUp: file.Id, navDown: file.Id))
            {
                app.Exit();
            }
        }

        return nextState;
    }
}
