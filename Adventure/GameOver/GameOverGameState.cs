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
        private IGameState nextState;
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

        public void Link(IGameState nextState)
        {
            this.nextState = nextState;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                persistence.Current.Zone.CurrentIndex = persistence.Current.Player.RespawnZone ?? 0;
                persistence.Current.Player.Position = persistence.Current.Player.RespawnPosition;
                persistence.Current.BattleTriggers.ClearData();
                persistence.Current.Party.Undefeated = false;
                if (persistence.Current.Party.Gold > 0)
                {
                    persistence.Current.Player.LootDropPosition = zoneManager.GetPlayerLoc();
                    persistence.Current.Player.LootDropZone = zoneManager.Current?.Index ?? 0;
                    persistence.Current.Player.LootDropGold = persistence.Current.Party.Gold;
                    persistence.Current.Party.Gold = 0;
                }
                else
                {
                    persistence.Current.Player.LootDropPosition = null;
                    persistence.Current.Player.LootDropZone = null;
                    persistence.Current.Player.LootDropGold = 0;
                }

                foreach (var character in persistence.Current.Party.Members)
                {
                    character.CharacterSheet.Rest();
                }
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
