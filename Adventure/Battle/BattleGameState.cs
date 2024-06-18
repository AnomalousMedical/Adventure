using DiligentEngine.RT;
using Engine.Platform;
using Adventure.GameOver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adventure.Items.Creators;
using Adventure.Services;
using Adventure.Menu;
using Engine;
using Adventure.Assets.Music;

namespace Adventure.Battle
{
    interface IBattleGameState : IGameState
    {
        /// <summary>
        /// This is a circular link, so it must be set by the ExplorationGameState itself, which injects this class.
        /// </summary>
        void Link(IGameState returnState, IGameOverGameState gameOver);
        void SetBattleTrigger(BattleTrigger battleTrigger);
        bool ShowExplorationMenu { get; set; }
    }

    class BattleGameState
    (
        IBattleManager battleManager,
        RTInstances<BattleScene> rtInstances,
        Party party,
        PotionCreator potionCreator,
        EventManager eventManager,
        IPersistenceWriter persistenceWriter,
        Persistence persistence,
        TypedLightManager<BattleScene> typedLightManager,
        CharacterMenuPositionService characterMenuPositionService,
        IExplorationMenu explorationMenu,
        FadeScreenMenu fadeScreenMenu,
        IScopedCoroutine coroutine,
        IBackgroundMusicPlayer backgroundMusicPlayer
    ) : IBattleGameState
    {
        private readonly object saveBlock = new object();
        private IGameOverGameState gameOverState;
        private IGameState returnState;
        private BattleTrigger battleTrigger;
        private Random noTriggerRandom = new Random();
        private bool saveOnExit = false;

        public bool ShowExplorationMenu { get; set; } = false;

        private IGameState nextState;

        public RTInstances Instances => rtInstances;

        public void Link(IGameState returnState, IGameOverGameState gameOver)
        {
            this.returnState = returnState;
            this.gameOverState = gameOver;
        }

        public void SetBattleTrigger(BattleTrigger battleTrigger)
        {
            this.battleTrigger = battleTrigger;
            if (battleTrigger != null)
            {
                persistence.Current.Player.LastBattleIndex = battleTrigger.Index;
                persistence.Current.Player.LastBattleIsBoss = battleTrigger.IsBoss;
            }
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                nextState = this;

                characterMenuPositionService.SetTrackerActive(typeof(BattleScene));
                persistence.Current.Player.InBattle = true;
                persistenceWriter.Save();
                persistenceWriter.AddSaveBlock(saveBlock);
                eventManager[EventLayers.Battle].makeFocusLayer();
                int battleSeed;
                int level;
                bool boss = false;
                Func<IEnumerable<ITreasure>> stealCb;
                BiomeEnemy triggerEnemy = null;
                if (battleTrigger == null) //This is the test battle setup
                {
                    level = party.GetAverageLevel() * 4 / 5;
                    if (level < 1)
                    {
                        level = 1;
                    }
                    battleSeed = noTriggerRandom.Next(int.MinValue, int.MaxValue);
                    boss = true;
                    var hasTreasure = true;
                    stealCb = () =>
                    {
                        if (hasTreasure)
                        {
                            hasTreasure = false;
                            return new[] { new Treasure(potionCreator.CreateManaPotion(level), TreasureType.Potion) };
                        }
                        else
                        {
                            return Enumerable.Empty<Treasure>();
                        }
                    };
                }
                else
                {
                    level = battleTrigger.EnemyLevel;
                    battleSeed = battleTrigger.BattleSeed;
                    boss = battleTrigger.IsBoss;
                    stealCb = battleTrigger.StealTreasure;
                    triggerEnemy = battleTrigger.TriggerEnemy;
                }

                battleManager.SetupBattle(battleSeed, level, boss, stealCb, triggerEnemy);
            }
            else
            {
                persistenceWriter.RemoveSaveBlock(saveBlock);
                if (saveOnExit)
                {
                    persistenceWriter.Save();
                    saveOnExit = false;
                }
            }
            typedLightManager.SetActive(active);
            battleManager.SetActive(active);
        }

        public IGameState Update(Clock clock)
        {
            if (ShowExplorationMenu)
            {
                explorationMenu.Update();
            }
            else if(nextState == this)
            {
                var result = battleManager.Update(clock);
                switch (result)
                {
                    case IBattleManager.Result.GameOver:
                        coroutine.RunTask(async () =>
                        {
                            backgroundMusicPlayer.SetBackgroundSong(GameOverMusic.File);

                            //Allow the game over state to save the defeated status.
                            //With the time delay this needs to happen right when you die.
                            persistenceWriter.RemoveSaveBlock(saveBlock);
                            gameOverState.SaveDeathStatus();
                            persistenceWriter.AddSaveBlock(saveBlock);

                            ShowExplorationMenu = true;
                            await fadeScreenMenu.ShowAndWaitAndClose(0.0f, 1.0f, 2.0f, GamepadId.Pad1);
                            ShowExplorationMenu = false;
                            nextState = gameOverState;
                        });
                        break;
                    case IBattleManager.Result.ReturnToExploration:
                        coroutine.RunTask(async () =>
                        {
                            ShowExplorationMenu = true;
                            await fadeScreenMenu.ShowAndWait(0.0f, 1.0f, 0.6f, GamepadId.Pad1);
                            ShowExplorationMenu = false;

                            nextState = returnState;
                            battleTrigger?.BattleWon();
                            persistence.Current.Player.InBattle = false;
                            saveOnExit = true;

                            await fadeScreenMenu.ShowAndWait(1.0f, 0.0f, 0.6f, GamepadId.Pad1);
                            fadeScreenMenu.Close();
                        });
                        break;
                }
            }

            if (nextState != this)
            {
                battleTrigger = null;
            }

            return nextState;
        }
    }
}
