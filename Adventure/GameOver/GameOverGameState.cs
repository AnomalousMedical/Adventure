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

namespace Adventure.GameOver
{
    interface IGameOverGameState : IGameState
    {
        void Link(IGameState explorationState);
    }

    class GameOverGameState : IGameOverGameState
    {
        private readonly ISharpGui sharpGui;
        private readonly RTInstances<IBattleManager> rtInstances;
        private readonly IScreenPositioner screenPositioner;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly Persistence persistence;
        private IGameState explorationState;
        private SharpButton restart = new SharpButton() { Text = "Restart" };
        private SharpText gameOver = new SharpText("Game Over");
        private ILayoutItem layout;

        public RTInstances Instances => rtInstances;

        public GameOverGameState
        (
            ISharpGui sharpGui,
            RTInstances<IBattleManager> rtInstances,
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

        public void Link(IGameState explorationState)
        {
            this.explorationState = explorationState;
        }

        public void SetActive(bool active)
        {
            persistence.Zone.CurrentIndex = persistence.Player.RespawnLevel ?? 0;
            persistence.Player.Position = persistence.Player.RespawnPosition;
            persistence.BattleTriggers.ClearData();

            foreach (var character in persistence.Party.Members)
            {
                character.CharacterSheet.Rest();
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
            if (sharpGui.Button(restart))
            {

                coroutineRunner.RunTask(zoneManager.Restart());
                nextState = explorationState;
            }

            return nextState;
        }
    }
}
