using Adventure.Menu;
using Adventure.Services;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using SharpGui;

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
    private SharpButton file = new SharpButton() { Text = "File" };
    private SharpText youWin = new SharpText("You Win") { Color = Color.White };
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
        IExplorationMenu explorationMenu
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
        layout = new ColumnLayout(new KeepWidthCenterLayout(youWin), file) { Margin = new IntPad(10) };
    }

    public void SetActive(bool active)
    {
        if (active)
        {
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

            if (sharpGui.Button(file, GamepadId.Pad1))
            {
                explorationMenu.RequestSubMenu(fileMenu, GamepadId.Pad1);
            }
        }

        return nextState;
    }
}
