using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Adventure.Battle;
using Adventure.Services;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    interface IWorldMapGameState : IGameState
    {
        void Link(IGameState explorationState);
    }

    class WorldMapGameState : IWorldMapGameState
    {
        private readonly ISharpGui sharpGui;
        private readonly RTInstances<IWorldMap> rtInstances;
        private readonly IScreenPositioner screenPositioner;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly Persistence persistence;
        private IGameState nextState;
        private SharpButton restart = new SharpButton() { Text = "Restart" };
        private SharpText gameOver = new SharpText("World Map");
        private ILayoutItem layout;

        public RTInstances Instances => rtInstances;

        public WorldMapGameState
        (
            ISharpGui sharpGui,
            RTInstances<IWorldMap> rtInstances,
            IScreenPositioner screenPositioner,
            ICoroutineRunner coroutineRunner,
            IZoneManager zoneManager,
            Persistence persistence
        )
        {
            this.sharpGui = sharpGui;
            this.rtInstances = rtInstances;
            this.screenPositioner = screenPositioner;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.persistence = persistence;
            layout = new ColumnLayout(gameOver, restart) { Margin = new IntPad(10) };
        }

        public void Link(IGameState nextState)
        {
            this.nextState = nextState;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                //persistence.Current.Zone.CurrentIndex = persistence.Current.Player.RespawnZone ?? 0;
                //persistence.Current.Player.Position = persistence.Current.Player.RespawnPosition;
                persistence.Current.BattleTriggers.ClearData();
            }
        }

        public IGameState Update(Clock clock)
        {
            IGameState nextState = this;

            var size = layout.GetDesiredSize(sharpGui);
            layout.GetDesiredSize(sharpGui);
            var rect = screenPositioner.GetCenterRect(size);
            layout.SetRect(rect);

            sharpGui.Text(gameOver);

            //TODO: Hacky to just use the button 4 times, add a way to process multiple pads
            if (sharpGui.Button(restart, GamepadId.Pad1) || sharpGui.Button(restart, GamepadId.Pad2) || sharpGui.Button(restart, GamepadId.Pad3) || sharpGui.Button(restart, GamepadId.Pad4))
            {
                coroutineRunner.RunTask(zoneManager.Restart());
                nextState = this.nextState;
            }

            return nextState;
        }
    }
}
