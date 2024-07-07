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
using Adventure.Menu;

namespace Adventure.GameOver
{
    interface IGameOverGameState : IGameState
    {
        void SaveDeathStatus();
    }

    class GameOverGameState : IGameOverGameState
    {
        private readonly ISharpGui sharpGui;
        private readonly RTInstances rtInstances;
        private readonly IScreenPositioner screenPositioner;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly Persistence persistence;
        private readonly IPersistenceWriter persistenceWriter;
        private readonly ISetupGameState setupGameState;
        private readonly ConfirmMenu confirmMenu;
        private readonly IExplorationMenu explorationMenu;
        private readonly ISetupRespawnGameState setupRespawnGameState;

        private SharpButton retry = new SharpButton() { Text = "Retry Battle" };
        private SharpButton restart = new SharpButton() { Text = "Restart Zone" };
        private SharpText gameOver = new SharpText("Game Over") { Color = Color.White };
        private readonly object saveBlock = new object();
        private IGameState nextState;

        public RTInstances Instances => rtInstances;

        public GameOverGameState
        (
            ISharpGui sharpGui,
            RTInstances<EmptyScene> rtInstances,
            IScreenPositioner screenPositioner,
            ICoroutineRunner coroutineRunner,
            IZoneManager zoneManager,
            Persistence persistence,
            IPersistenceWriter persistenceWriter,
            ISetupGameState setupGameState,
            FontLoader fontLoader,
            ConfirmMenu confirmMenu,
            IExplorationMenu explorationMenu,
            ISetupRespawnGameState setupRespawnGameState
        )
        {
            gameOver.Font = fontLoader.TitleFont;

            this.sharpGui = sharpGui;
            this.rtInstances = rtInstances;
            this.screenPositioner = screenPositioner;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.persistence = persistence;
            this.persistenceWriter = persistenceWriter;
            this.setupGameState = setupGameState;
            this.confirmMenu = confirmMenu;
            this.explorationMenu = explorationMenu;
            this.setupRespawnGameState = setupRespawnGameState;
        }

        public void SaveDeathStatus()
        {
            if (persistence.Current.Party.Undefeated)
            {
                persistenceWriter.SaveDefeated();
                persistence.Current.Party.Undefeated = false;
            }
            persistence.Current.Party.GameOver = true;
            persistenceWriter.SaveGameOver(persistence.Current.Party.GameOver);
        }

        public void SetActive(bool active)
        {
            nextState = this;
            if (active)
            {
                persistenceWriter.AddSaveBlock(saveBlock);
            }
            else
            {
                persistenceWriter.RemoveSaveBlock(saveBlock);
                persistence.Current.Party.GameOver = false;
                persistenceWriter.SaveGameOver(persistence.Current.Party.GameOver);
            }
        }

        public IGameState Update(Clock clock)
        {
            if (nextState == this)
            {
                if (!explorationMenu.Update())
                {
                    var layout = new ColumnLayout(new KeepWidthCenterLayout(gameOver), retry, restart) { Margin = new IntPad(10) };

                    var size = layout.GetDesiredSize(sharpGui);
                    layout.GetDesiredSize(sharpGui);
                    var rect = screenPositioner.GetCenterRect(size);
                    layout.SetRect(rect);

                    sharpGui.Text(gameOver);

                    if (sharpGui.Button(retry, GamepadId.Pad1, navUp: restart.Id, navDown: restart.Id))
                    {
                        coroutineRunner.RunTask(async () =>
                        {
                            if (!persistence.Current.Party.OldSchool || await confirmMenu.ShowAndWait("Restart from current battle?", null, GamepadId.Pad1))
                            {
                                if (persistence.Current.Party.OldSchool)
                                {
                                    persistenceWriter.SaveNewSchool();
                                    persistence.Current.Party.OldSchool = false;
                                }

                                nextState = setupGameState;
                            }
                        });
                    }

                    if (sharpGui.Button(restart, GamepadId.Pad1, navUp: retry.Id, navDown: retry.Id))
                    {
                        persistence.Current.Zone.CurrentIndex = persistence.Current.Player.RespawnZone ?? 0;
                        persistence.Current.Player.Position = persistence.Current.Player.RespawnPosition;
                        persistence.Current.BattleTriggers.ClearData();
                        persistence.Current.Player.InBattle = false;

                        foreach (var character in persistence.Current.Party.Members)
                        {
                            character.CharacterSheet.Rest();
                        }

                        persistenceWriter.Save();

                        coroutineRunner.RunTask(zoneManager.Restart(() => Task.CompletedTask));
                        nextState = setupRespawnGameState;
                    }
                }
            }

            return nextState;
        }
    }
}
