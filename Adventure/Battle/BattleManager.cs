using Anomalous.OSPlatform;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface IBattleManager
    {
        public enum Result
        {
            ContinueBattle,
            ReturnToExploration,
            GameOver
        }

        bool Active { get; }

        void AddToActivePlayers(BattlePlayer player);
        void Attack(IBattleTarget attacker, IBattleTarget target);
        void ChangeBlockingStatus(IBattleTarget blocker);
        Task<IBattleTarget> GetTarget(bool targetPlayers);

        /// <summary>
        /// Called by players before they queue their turn to remove them from the active player list.
        /// </summary>
        void DeactivateCurrentPlayer();

        /// <summary>
        /// Called by everything to start the turn.
        /// </summary>
        /// <param name="turn"></param>
        void QueueTurn(Func<Clock, bool> turn);

        /// <summary>
        /// Set battle mode active / inactive.
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);
        void SetupBattle(int battleSeed, int level, bool boss, Func<IEnumerable<ITreasure>> Steal, BiomeEnemy triggerEnemy);
        Result Update(Clock clock);
        IBattleTarget ValidateTarget(IBattleTarget attacker, IBattleTarget target);
        IBattleTarget GetRandomPlayer();
        void PlayerDead(BattlePlayer battlePlayer);

        IDamageCalculator DamageCalculator { get; }
        bool AllowActivePlayerGui { get; set; }

        void HandleDeath(IBattleTarget target);

        public void AddDamageNumber(IBattleTarget target, long damage);

        public void AddDamageNumber(IBattleTarget target, String damage, Color color);

        void SwitchPlayer();

        /// <summary>
        /// Make sure the targets exist with no other logic.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        bool IsStillValidTarget(IBattleTarget target);

        public IEnumerable<ITreasure> Steal();
        IEnumerable<IBattleTarget> GetTargetsInGroup(IBattleTarget target);
        BattlePlayer GetActivePlayer();
        IBattleTarget GetBlocker(IBattleTarget attacker, IBattleTarget target);
    }

    class BattleManager : IDisposable, IBattleManager
    {
        const long NumberDisplayTime = (long)(0.9f * Clock.SecondsToMicro);

        private readonly EventManager eventManager;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly CameraMover cameraMover;
        private readonly IZoneManager zoneManager;
        private readonly Party party;
        private readonly IDamageCalculator damageCalculator;
        private readonly IBackgroundMusicPlayer backgroundMusicPlayer;
        private readonly IScreenPositioner screenPositioner;
        private readonly ICameraProjector cameraProjector;
        private readonly ITurnTimer turnTimer;
        private readonly IBattleScreenLayout battleScreenLayout;
        private readonly IBattleBuilder battleBuilder;
        private readonly IObjectResolver objectResolver;
        private BattleArena battleArena;

        private SharpButton endBattle = new SharpButton() { Text = "End Battle" };
        private SharpText goldRewardText = new SharpText() { Color = Color.White };

        private List<Enemy> enemies = new List<Enemy>(20);
        private List<Enemy> killedEnemies = new List<Enemy>(20);
        private List<BattlePlayer> players = new List<BattlePlayer>(4);
        private List<DamageNumber> numbers = new List<DamageNumber>(10);
        private Queue<BattlePlayer> activePlayers = new Queue<BattlePlayer>(4);
        private Queue<Func<Clock, bool>> turnQueue = new Queue<Func<Clock, bool>>(30);

        private TargetCursor cursor;

        private Random targetRandom = new Random();
        private String backgroundMusic;
        private Func<IEnumerable<ITreasure>> stealCb;

        private readonly List<IBattleTarget> blockingPlayers = new List<IBattleTarget>();

        public bool AllowActivePlayerGui { get; set; } = true;

        public BattleManager(EventManager eventManager,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IObjectResolverFactory objectResolverFactory,
            CameraMover cameraMover,
            IZoneManager zoneManager,
            Party party,
            IDamageCalculator damageCalculator,
            IBackgroundMusicPlayer backgroundMusicPlayer,
            IScreenPositioner screenPositioner,
            ICameraProjector cameraProjector,
            ITurnTimer turnTimer,
            IBattleScreenLayout battleScreenLayout,
            IBattleBuilder battleBuilder)
        {
            this.eventManager = eventManager;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.cameraMover = cameraMover;
            this.zoneManager = zoneManager;
            this.party = party;
            this.damageCalculator = damageCalculator;
            this.backgroundMusicPlayer = backgroundMusicPlayer;
            this.screenPositioner = screenPositioner;
            this.cameraProjector = cameraProjector;
            this.turnTimer = turnTimer;
            this.battleScreenLayout = battleScreenLayout;
            this.battleBuilder = battleBuilder;
            this.objectResolver = objectResolverFactory.Create();

            cursor = this.objectResolver.Resolve<TargetCursor>();

            zoneManager.ZoneChanged += ZoneManager_ZoneChanged;
        }

        public void Dispose()
        {
            zoneManager.ZoneChanged -= ZoneManager_ZoneChanged;
            objectResolver.Dispose();
        }

        public void SetupBattle(int battleSeed, int level, bool boss, Func<IEnumerable<ITreasure>> stealCb, BiomeEnemy triggerEnemy)
        {
            this.stealCb = stealCb;
            var currentZ = 3;
            foreach (var character in party.ActiveCharacters)
            {
                players.Add(this.objectResolver.Resolve<BattlePlayer, BattlePlayer.Description>(c =>
                {
                    c.Translation = new Vector3(4, 0, currentZ);
                    c.CharacterSheet = character.CharacterSheet;
                    c.Inventory = character.Inventory;
                    c.PlayerSprite = character.PlayerSprite;
                    c.Gamepad = (GamepadId)character.Player;
                }));

                currentZ -= 2;
            }

            var rand = new Random(battleSeed);
            IEnumerable<Enemy> createEnemies;
            if (boss)
            {
                backgroundMusic = zoneManager.Current.Biome.BossBattleMusic;
                createEnemies = battleBuilder.CreateBoss(this.objectResolver, party, zoneManager.Current.Biome, rand, level);
            }
            else
            {
                backgroundMusic = zoneManager.Current.Biome.BattleMusic;
                createEnemies = battleBuilder.CreateEnemies(this.objectResolver, party, zoneManager.Current.Biome, rand, level, triggerEnemy);
            }

            enemies.AddRange(createEnemies);
        }

        public void SetActive(bool active)
        {
            if (active != this.Active)
            {
                this.Active = active;
                if (active)
                {
                    cursor.BattleStarted();
                    numbers.Clear();

                    backgroundMusicPlayer.SetBattleTrack(backgroundMusic);
                    var allTimers = players.Select(i => i.CharacterTimer).Concat(enemies.Select(i => i.CharacterTimer));
                    var baseDexTotal = 0;
                    foreach(var player in players)
                    {
                        baseDexTotal += player.BaseDexterity;
                        player.CharacterTimer.TurnTimerActive = !player.IsDead;
                    }
                    turnTimer.Restart(0, baseDexTotal);

                    eventManager[EventLayers.Battle].OnUpdate += eventManager_OnUpdate;
                    cameraMover.Position = new Vector3(-1.0354034f, 2.958224f, -12.394701f);
                    cameraMover.Orientation = new Quaternion(0.057467595f, 0.0049917176f, -0.00028734046f, 0.9983348f);
                    cameraMover.SceneCenter = new Vector3(0f, 0f, 0f);

                    var instantAttackChance = targetRandom.Next(100);
                    if (instantAttackChance < 3)
                    {
                        //Instant attack, players start with turns and enemies start at 0
                        foreach (var player in players.Where(i => !i.IsDead))
                        {
                            player.CharacterTimer.SetInstantTurn();
                        }
                    }
                    else
                    {
                        //Adjust highest timer until it is 57344, adjust all others the same amount
                        long highestTimer = 0;
                        var allLivingTimers = players
                            .Where(i => !i.IsDead)
                            .Select(i => i.CharacterTimer)
                            .Concat(enemies.Select(i => i.CharacterTimer));

                        foreach (var timer in allLivingTimers)
                        {
                            timer.TurnTimer = targetRandom.Next(32767);
                            if (timer.TurnTimer > highestTimer)
                            {
                                highestTimer = timer.TurnTimer;
                            }
                        }

                        long adjustment = 57344 - highestTimer;
                        foreach (var timer in allLivingTimers)
                        {
                            timer.TurnTimer += adjustment;
                        }
                    }
                }
                else
                {
                    backgroundMusicPlayer.SetBattleTrack(null);

                    foreach (var player in players)
                    {
                        player.RequestDestruction();
                    }
                    players.Clear();
                    foreach (var enemy in enemies)
                    {
                        enemy.RequestDestruction();
                    }
                    enemies.Clear();
                    killedEnemies.Clear();
                    eventManager[EventLayers.Battle].OnUpdate -= eventManager_OnUpdate;
                }
            }
        }

        public IBattleManager.Result Update(Clock clock)
        {
            var result = IBattleManager.Result.ContinueBattle;
            if (turnQueue.Count > 0)
            {
                var turn = turnQueue.Peek();
                if (turn.Invoke(clock) && turnQueue.Count > 0)
                {
                    turnQueue.Dequeue();
                }
            }

            //This order means if all the players and enemies die it is not a game over.
            //But all players wil have 0 hp. This is probably not what we want.
            if (enemies.Count == 0)
            {
                var goldReward = 0L;
                foreach (var killed in killedEnemies)
                {
                    goldReward += killed.GoldReward;
                }

                goldRewardText.Text = $"Gold: {goldReward}";

                cursor.Visible = false;

                var layout =
                    new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                    new MaxWidthLayout(scaleHelper.Scaled(300),
                    new ColumnLayout(goldRewardText, endBattle) { Margin = new IntPad(10) }
                    ));
                var desiredSize = layout.GetDesiredSize(sharpGui);

                layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

                sharpGui.Text(goldRewardText);

                //TODO: Hacky to just use the button 4 times, add a way to process multiple pads
                if (sharpGui.Button(endBattle, GamepadId.Pad1) || sharpGui.Button(endBattle, GamepadId.Pad2) || sharpGui.Button(endBattle, GamepadId.Pad3) || sharpGui.Button(endBattle, GamepadId.Pad4))
                {
                    party.Gold += goldReward;

                    result = IBattleManager.Result.ReturnToExploration;
                }
            }
            else
            {
                turnTimer.Update(clock);

                BattlePlayer activePlayer = GetActivePlayer();
                while (activePlayer?.IsDead == true && activePlayers.Count > 0)
                {
                    activePlayers.Dequeue();
                    activePlayer = GetActivePlayer();
                }
                if (cursor.Targeting)
                {
                    cursor.Visible = true;
                    IBattleTarget target;
                    Vector3 targetPos;
                    if (cursor.TargetPlayers)
                    {
                        target = players[(int)(cursor.PlayerTargetIndex % players.Count)];
                        targetPos = target.CursorDisplayLocation;

                        foreach (var player in players)
                        {
                            player.DrawInfoGui(clock, sharpGui, target == player);
                        }
                    }
                    else
                    {
                        target = enemies[(int)(cursor.EnemyTargetIndex % enemies.Count)];
                        targetPos = target.CursorDisplayLocation;
                    }
                    
                    cursor.UpdateCursor(this, target, targetPos, activePlayer);
                }
                else
                {
                    battleScreenLayout.LayoutCommonItems();
                    foreach (var player in players)
                    {
                        player.DrawInfoGui(clock, sharpGui);
                    }

                    cursor.Visible = false;

                    if (AllowActivePlayerGui)
                    {
                        activePlayer?.UpdateActivePlayerGui(sharpGui);
                    }
                }

                bool allDead = true;
                foreach (var player in players)
                {
                    allDead &= player.IsDead;
                    player.SetGuiActive(player == activePlayer);
                }
                if (allDead)
                {
                    result = IBattleManager.Result.GameOver;
                }

                for (var i = 0; i < numbers.Count;)
                {
                    var number = numbers[i];
                    sharpGui.Text(number.Text);
                    number.TimeRemaining -= clock.DeltaTimeMicro;
                    number.UpdatePosition();
                    if (number.TimeRemaining < 0)
                    {
                        numbers.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
            return result;
        }

        public void SwitchPlayer()
        {
            var activePlayer = GetActivePlayer();
            activePlayers.Dequeue();
            activePlayers.Enqueue(activePlayer);
        }

        public BattlePlayer GetActivePlayer()
        {
            if(activePlayers.Count == 0)
            {
                return null;
            }
            return activePlayers.Peek();
        }

        public bool Active { get; private set; }

        private void eventManager_OnUpdate(EventLayer eventLayer)
        {
            eventLayer.alertEventsHandled();
        }

        private void ZoneManager_ZoneChanged(IZoneManager levelManager)
        {
            battleArena?.RequestDestruction();

            battleArena = objectResolver.Resolve<BattleArena, BattleArena.Description>(o =>
            {
                o.Scale = new Vector3(20, 0.1f, 20);
                o.Translation = new Vector3(0f, o.Scale.y / -2f, 0f);
                o.Texture = levelManager.Current.Biome.FloorTexture;
                o.Reflective = levelManager.Current.Biome.ReflectFloor;
            });
        }

        public IBattleTarget ValidateTarget(IBattleTarget attacker, IBattleTarget target)
        {
            //Make sure target still exists
            switch (target.BattleTargetType)
            {
                case BattleTargetType.Enemy:
                    if (!enemies.Contains(target))
                    {
                        target = enemies[targetRandom.Next(enemies.Count)];
                    }
                    break;
                case BattleTargetType.Player:
                    if (!players.Contains(target) || (target as BattlePlayer).IsDead)
                    {
                        target = GetRandomPlayer();
                    }
                    break;
            }

            return target;
        }

        public bool IsStillValidTarget(IBattleTarget target)
        {
            //Make sure target still exists
            switch (target.BattleTargetType)
            {
                case BattleTargetType.Enemy:
                    return enemies.Contains(target);
                case BattleTargetType.Player:
                    return players.Contains(target);
            }

            return false;
        }

        public IBattleTarget GetBlocker(IBattleTarget attacker, IBattleTarget target)
        {
            IBattleTarget blocker = null;
            var blockers = Enumerable.Empty<IBattleTarget>();
            switch (target.BattleTargetType)
            {
                case BattleTargetType.Enemy:
                    break;
                case BattleTargetType.Player:
                    blockers = blockingPlayers;
                    break;
            }

            if (!blockers.Contains(target))
            {
                blocker = blockers
                    .Where(i => damageCalculator.Block(attacker.Stats, i.Stats))
                    .FirstOrDefault();
            }

            return blocker;
        }

        public void Attack(IBattleTarget attacker, IBattleTarget target)
        {
            target = ValidateTarget(attacker, target);

            if (damageCalculator.PhysicalHit(attacker.Stats, target.Stats))
            {
                var damage = damageCalculator.Physical(attacker.Stats, target.Stats, 16);

                foreach (var attackElement in attacker.Stats.AttackElements)
                {
                    var resistance = target.Stats.GetResistance(attackElement);
                    damage = damageCalculator.ApplyResistance(damage, resistance);
                    if (resistance == Resistance.Death || resistance == Resistance.Recovery)
                    {
                        //This is enough to handle death and recovery for any element.
                        //The damage returned will be the number needed to apply the effect
                        //and we just want to stop modifying it immediately and apply it
                        break;
                    }
                }

                //TODO: This may not be the best spot for this since it will randomize the death
                //and recovery values too
                damage = damageCalculator.RandomVariation(damage);

                AddDamageNumber(target, damage);
                target.ApplyDamage(damageCalculator, damage);
                HandleDeath(target);
            }
            else
            {
                AddDamageNumber(target, "Miss", Color.White);
            }
        }

        public void HandleDeath(IBattleTarget target)
        {
            if (target.IsDead)
            {
                if (enemies.Contains(target))
                {
                    var enemy = target as Enemy;
                    target.RequestDestruction();
                    enemies.Remove(enemy);
                    killedEnemies.Add(enemy);
                    if (enemies.Count == 0)
                    {
                        BattleEnded();
                        backgroundMusicPlayer.SetBattleTrack("Music/freepd/Alexander Nakarada - Fanfare X.ogg");
                    }
                }
                else if (target.BattleTargetType == BattleTargetType.Player)
                {
                    blockingPlayers.Remove(target);
                }
            }
        }

        public IEnumerable<IBattleTarget> GetTargetsInGroup(IBattleTarget target)
        {
            if (enemies.Contains(target))
            {
                return enemies;
            }

            if (players.Contains(target))
            {
                return players;
            }

            return Enumerable.Empty<IBattleTarget>();
        }

        public void AddDamageNumber(IBattleTarget target, long damage)
        {
            var color = Color.White;
            if(damage < 0)
            {
                color = Color.Green;
                damage *= -1;
            }
            AddDamageNumber(target, damage.ToString(), color);
        }

        public void AddDamageNumber(IBattleTarget target, String damage, Color color)
        {
            var targetPos = target.DamageDisplayLocation - cameraMover.SceneCenter;
            var screenPos = cameraProjector.Project(targetPos);

            numbers.Add(new DamageNumber(damage.ToString(), NumberDisplayTime, screenPos, scaleHelper, color));
        }

        public void DeactivateCurrentPlayer()
        {
            activePlayers.Dequeue();
        }

        public void QueueTurn(Func<Clock, bool> turn)
        {
            this.turnQueue.Enqueue(turn);
        }

        public void AddToActivePlayers(BattlePlayer player)
        {
            this.activePlayers.Enqueue(player);
        }

        public Task<IBattleTarget> GetTarget(bool targetPlayers)
        {
            return cursor.GetTarget(targetPlayers);
        }

        public IBattleTarget GetRandomPlayer()
        {
            //get number of living players
            var living = players.Where(i => !i.IsDead).ToList();
            var rand = targetRandom.Next(living.Count);
            return living[rand];
        }

        public void PlayerDead(BattlePlayer battlePlayer)
        {
            if(battlePlayer == GetActivePlayer())
            {
                cursor.Cancel();
                activePlayers.Dequeue();
            }

            var living = players.Where(i => !i.IsDead).Sum(i => 1);
            if(living == 0)
            {
                BattleEnded();
            }
        }

        private void BattleEnded()
        {
            cursor.Cancel();
            turnQueue.Clear();
            activePlayers.Clear();
            blockingPlayers.Clear();
        }

        public IDamageCalculator DamageCalculator => damageCalculator;

        public IEnumerable<ITreasure> Steal()
        {
            return stealCb?.Invoke() ?? Enumerable.Empty<ITreasure>();
        }

        public void ChangeBlockingStatus(IBattleTarget blocker)
        {
            switch (blocker.BattleTargetType)
            {
                case BattleTargetType.Player:
                    if (blockingPlayers.Contains(blocker))
                    {
                        blockingPlayers.Remove(blocker);
                    }
                    else
                    {
                        blockingPlayers.Add(blocker);
                    }
                    break;
                case BattleTargetType.Enemy:
                    break;
            }
        }
    }
}
