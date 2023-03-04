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
        private readonly RTInstances<BattleScene> rtInstances;
        private readonly IScreenPositioner screenPositioner;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly Persistence persistence;
        private readonly IPersistenceWriter persistenceWriter;
        private readonly ISetupGameState setupGameState;
        private IGameState nextState;
        private SharpButton load = new SharpButton() { Text = "Load" };
        private SharpButton restart = new SharpButton() { Text = "Restart" };
        private SharpText gameOver = new SharpText("Game Over");
        private ILayoutItem layout;

        public RTInstances Instances => rtInstances;

        public GameOverGameState
        (
            ISharpGui sharpGui,
            RTInstances<BattleScene> rtInstances,
            IScreenPositioner screenPositioner,
            ICoroutineRunner coroutineRunner,
            IZoneManager zoneManager,
            Persistence persistence,
            IPersistenceWriter persistenceWriter,
            ISetupGameState setupGameState
        )
        {
            this.sharpGui = sharpGui;
            this.rtInstances = rtInstances;
            this.screenPositioner = screenPositioner;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.persistence = persistence;
            this.persistenceWriter = persistenceWriter;
            this.setupGameState = setupGameState;
            layout = new ColumnLayout(gameOver, load, restart) { Margin = new IntPad(10) };
        }

        public void Link(IGameState nextState)
        {
            this.nextState = nextState;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                if (persistence.Current.Party.Undefeated)
                {
                    persistenceWriter.SaveDefeated();
                    persistence.Current.Party.Undefeated = false;
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
            if (sharpGui.Button(load, GamepadId.Pad1) || sharpGui.Button(load, GamepadId.Pad2) || sharpGui.Button(load, GamepadId.Pad3) || sharpGui.Button(load, GamepadId.Pad4))
            {
                nextState = setupGameState;
            }

            if (sharpGui.Button(restart, GamepadId.Pad1) || sharpGui.Button(restart, GamepadId.Pad2) || sharpGui.Button(restart, GamepadId.Pad3) || sharpGui.Button(restart, GamepadId.Pad4))
            {
                persistence.Current.Zone.CurrentIndex = persistence.Current.Player.RespawnZone ?? 0;
                persistence.Current.Player.Position = persistence.Current.Player.RespawnPosition;
                persistence.Current.BattleTriggers.ClearData();
                if (persistence.Current.Party.Gold > 0)
                {
                    var takeGold = (long)(persistence.Current.Party.Gold * 0.75f);
                    persistence.Current.Player.LootDropPosition = zoneManager.GetPlayerLoc();
                    persistence.Current.Player.LootDropZone = zoneManager.Current?.Index ?? 0;
                    persistence.Current.Player.LootDropGold = takeGold;
                    persistence.Current.Party.Gold -= takeGold;
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

                persistenceWriter.Save();

                coroutineRunner.RunTask(zoneManager.Restart());
                nextState = this.nextState;
            }

            return nextState;
        }
    }
}
