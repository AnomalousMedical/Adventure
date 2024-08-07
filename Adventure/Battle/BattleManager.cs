﻿using Adventure.Assets;
using Adventure.Assets.Music;
using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using Adventure.Services;
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
        void Attack(IBattleTarget attacker, IBattleTarget target, bool isCounter, bool blocked, bool fumbleBlock, bool isPower, bool triggered, bool triggerSpammed);
        Task<IBattleTarget> GetTarget(bool targetPlayers);

        /// <summary>
        /// Called by players before they queue their turn to remove them from the active player list.
        /// </summary>
        void DeactivateCurrentPlayer();

        /// <summary>
        /// Called by everything to start the turn.
        /// </summary>
        /// <param name="turn"></param>
        void QueueTurn(Func<Clock, bool> turn, bool queueFront = false);

        /// <summary>
        /// Set battle mode active / inactive.
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);
        void SetupBattle(int battleSeed, int level, bool boss, Func<IEnumerable<ITreasure>> Steal, BiomeEnemy triggerEnemy, int? nextLevel);
        Result Update(Clock clock);
        IBattleTarget ValidateTarget(IBattleTarget attacker, IBattleTarget target);
        IBattleTarget GetRandomPlayer();
        void PlayerDead(BattlePlayer battlePlayer);
        IDamageCalculator DamageCalculator { get; }
        ISoundEffectPlayer SoundEffectPlayer { get; }

        bool AllowActivePlayerGui { get; set; }

        void HandleDeath(IBattleTarget target);

        public void AddDamageNumber(IBattleTarget target, long damage);

        public void AddDamageNumber(IBattleTarget target, long damage, Color color);

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
        IBattleTarget GetGuard(IBattleTarget attacker, IBattleTarget target);
        IBattleTarget GetTargetForStats(IBattleStats stats);
        void CancelTargeting();
        BattlePlayer GetPlayerForTarget(IBattleStats stats);
    }

    class BattleManager : IDisposable, IBattleManager
    {
        public ISoundEffectPlayer SoundEffectPlayer => soundEffectPlayer;

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
        private readonly BuffManager buffManager;
        private readonly IScopedCoroutine coroutine;
        private readonly BattleAssetLoader battleAssetLoader;
        private readonly ISoundEffectPlayer soundEffectPlayer;
        private readonly ILanguageService languageService;
        private readonly IObjectResolver objectResolver;
        private BattleArena battleArena;
        private List<BattleBackgroundItem> bgItems = new List<BattleBackgroundItem>();

        private SharpButton endBattle = new SharpButton() { Text = "End Battle" };
        private SharpText goldRewardText = new SharpText() { Color = Color.UIWhite };
        private SharpText victoryText = new SharpText() { Color = Color.UIWhite };
        private SharpText nextRankText = new SharpText() { Color = Color.UIWhite };
        private SharpPanel victoryPanel = new SharpPanel();
        private SharpStyle panelStyle = new SharpStyle() { Background = Color.UITransparentBg };

        private List<Enemy> enemies = new List<Enemy>(20);
        private List<Enemy> killedEnemies = new List<Enemy>(20);
        private List<BattlePlayer> players = new List<BattlePlayer>(4);
        private List<DamageNumber> numbers = new List<DamageNumber>(10);
        private Queue<BattlePlayer> activePlayers = new Queue<BattlePlayer>(4);
        bool allowBattleFinish = false;
        bool showEndBattleButton = false;
        int? nextLevel;
        long startBattleDelay;
        bool isBoss;


        class TurnQueueEntry
        {
            public Func<Clock, bool> Turn { get; init; }

            public bool Priority { get; set; }

            public int SkipCount { get; set; } = 0;
        }
        private List<TurnQueueEntry> turnQueue = new List<TurnQueueEntry>(30);
        private TurnQueueEntry currentTurn = null;

        private TargetCursor cursor;

        private Random targetRandom = new Random();
        private String backgroundMusic;
        private Func<IEnumerable<ITreasure>> stealCb;

        public bool AllowActivePlayerGui { get; set; } = true;

        public BattleManager
        (
            EventManager eventManager,
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
            IBattleBuilder battleBuilder,
            BuffManager buffManager,
            IScopedCoroutine coroutine,
            BattleAssetLoader battleAssetLoader,
            ISoundEffectPlayer soundEffectPlayer,
            FontLoader fontLoader,
            ILanguageService languageService
        )
        {
            victoryText.Font = fontLoader.TitleFont;
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
            this.buffManager = buffManager;
            this.coroutine = coroutine;
            this.battleAssetLoader = battleAssetLoader;
            this.soundEffectPlayer = soundEffectPlayer;
            this.languageService = languageService;
            this.objectResolver = objectResolverFactory.Create();

            cursor = this.objectResolver.Resolve<TargetCursor>();

            zoneManager.ZoneChanged += ZoneManager_ZoneChanged;
        }

        public void Dispose()
        {
            zoneManager.ZoneChanged -= ZoneManager_ZoneChanged;
            objectResolver.Dispose();
        }

        public void SetupBattle(int battleSeed, int level, bool boss, Func<IEnumerable<ITreasure>> stealCb, BiomeEnemy triggerEnemy, int? nextLevel)
        {
            this.isBoss = boss;
            this.nextLevel = nextLevel;
            startBattleDelay = (long)(0.7f * Clock.SecondsToMicro);
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
                    c.CharacterStyleIndex = character.StyleIndex;
                }));

                currentZ -= 2;
            }

            var rand = new FIRandom(battleSeed);
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

        private static readonly Vector3 CameraTranslation = new Vector3(-1.0354034f, 2.958224f, -12.394701f);
        private static readonly Quaternion CameraRotation = new Quaternion(0.057467595f, 0.0049917176f, -0.00028734046f, 0.9983348f);

        public void SetActive(bool active)
        {
            if (active != this.Active)
            {
                this.Active = active;
                if (active)
                {
                    cursor.BattleStarted();
                    cursor.Visible = false; 
                    numbers.Clear();
                    allowBattleFinish = false;
                    showEndBattleButton = false;

                    backgroundMusicPlayer.SetBackgroundSong(backgroundMusic);
                    var allTimers = players.Select(i => i.CharacterTimer).Concat(enemies.Select(i => i.CharacterTimer));
                    var baseDexTotal = 0L;
                    foreach(var player in players)
                    {
                        baseDexTotal += player.BaseDexterity;
                        player.CharacterTimer.TurnTimerActive = !player.IsDead;
                    }
                    turnTimer.Restart(0, baseDexTotal);

                    eventManager[EventLayers.Battle].OnUpdate += eventManager_OnUpdate;
                    cameraMover.SetPosition(CameraTranslation, CameraRotation);

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
            if(startBattleDelay > 0)
            {
                cursor.Visible = false;
                startBattleDelay -= clock.DeltaTimeMicro;
                return IBattleManager.Result.ContinueBattle;
            }

            buffManager.Update(clock);

            var result = IBattleManager.Result.ContinueBattle;
            if (turnQueue.Count > 0)
            {
                if(currentTurn == null)
                {
                    currentTurn = turnQueue[0];
                }
                if (currentTurn.Turn.Invoke(clock))
                {
                    turnQueue.Remove(currentTurn);
                    currentTurn = null;
                }
            }

            if (allowBattleFinish)
            {
                var goldReward = 0L;
                foreach (var killed in killedEnemies)
                {
                    goldReward += killed.GoldReward;
                }

                goldRewardText.Text = "Gold: " + goldReward;

                cursor.Visible = false;

                {
                    var layout =
                        new MarginLayout(new IntPad(scaleHelper.Scaled(17)),
                        new ColumnLayout(endBattle)
                        { Margin = new IntPad(3) });

                    var desiredSize = layout.GetDesiredSize(sharpGui);
                    layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));
                }

                {
                    victoryText.Text = isBoss ? "Boss Terminated" : "Enemies Defeated";

                    var columns = new ColumnLayout(victoryText, new KeepWidthCenterLayout(goldRewardText)) { Margin = scaleHelper.Scaled(new IntPad(10)) };
                    var layout = new PanelLayout(victoryPanel, columns);

                    if (nextLevel != null)
                    {
                        columns.Add(new KeepWidthCenterLayout(nextRankText));
                        nextRankText.Text = "New Rank: " + languageService.Current.Levels.GetText(nextLevel.Value);
                    }

                    var desiredSize = layout.GetDesiredSize(sharpGui);
                    layout.SetRect(screenPositioner.GetCenterTopRect(desiredSize));
                }

                sharpGui.Panel(victoryPanel, panelStyle);
                sharpGui.Text(victoryText);
                sharpGui.Text(goldRewardText);
                if(nextLevel != null)
                {
                    sharpGui.Text(nextRankText);
                }

                //TODO: Hacky to just use the button 4 times, add a way to process multiple pads
                if (showEndBattleButton &&
                    (sharpGui.Button(endBattle, GamepadId.Pad1) || sharpGui.Button(endBattle, GamepadId.Pad2) || sharpGui.Button(endBattle, GamepadId.Pad3) || sharpGui.Button(endBattle, GamepadId.Pad4)))
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
                    IBattleTarget target;
                    Vector3 targetPos = Vector3.Zero;
                    if (cursor.TargetPlayers)
                    {
                        target = players[(int)(cursor.PlayerTargetIndex % players.Count)];
                        targetPos = target.CursorDisplayLocation;

                        foreach (var player in players)
                        {
                            player.DrawInfoGui(clock, sharpGui, target == player);
                        }
                    }
                    else if (enemies.Count == 0)
                    {
                        target = null;
                    }
                    else
                    {
                        target = enemies[(int)(cursor.EnemyTargetIndex % enemies.Count)];
                        targetPos = target.CursorDisplayLocation;
                    }

                    if(activePlayer == null || target == null)
                    {
                        cursor.Cancel();
                        cursor.Visible = false;
                    }
                    else
                    {
                        cursor.Visible = true;
                        cursor.UpdateCursor(this, target, targetPos, activePlayer);
                    }
                }
                else
                {
                    battleScreenLayout.LayoutCommonItems();
                    foreach (var player in players)
                    {
                        player.DrawInfoGui(clock, sharpGui);
                    }

                    cursor.Visible = false;

                    if (AllowActivePlayerGui && enemies.Count > 0)
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
            foreach(var placeable in bgItems)
            {
                placeable.RequestDestruction();
            }
            bgItems.Clear();

            battleArena?.RequestDestruction();

            battleArena = objectResolver.Resolve<BattleArena, BattleArena.Description>(o =>
            {
                o.Scale = new Vector3(20, 0.1f, 20);
                o.Translation = new Vector3(0f, o.Scale.y / -2f, 0f);
                o.Biome = levelManager.Current.Biome;
                o.CameraZItemDeadzone = CameraTranslation.z - 0.1f;
            });

            var biome = levelManager.Current.Biome;
            var bgItemsRandom = new FIRandom(levelManager.Current.Index); //TODO: Get better, but still consistent seed for here

            if (biome.BackgroundItems.Count > 0)
            {
                var mustBeEven = false;
                var lastX = 0.0f;
                foreach(var location in battleArena.BgItemLocations())
                {
                    if(location.x != lastX)
                    {
                        lastX = location.x;
                        mustBeEven = !mustBeEven;
                    }

                    BiomeBackgroundItem add = null;
                    var roll = bgItemsRandom.Next(0, biome.MaxBackgroundItemRoll);
                    foreach (var item in biome.BackgroundItems)
                    {
                        if (roll < item.Chance)
                        {
                            add = item;
                            break;
                        }
                    }

                    if (add != null)
                    {
                        var bgItem = objectResolver.Resolve<BattleBackgroundItem, BattleBackgroundItem.Description>(o =>
                        {
                            var mapUnitX = battleArena.Step * add.XPlacementRange;
                            var halfUnitX = mapUnitX * 0.5f;
                            var mapUnitZ = battleArena.Step * add.ZPlacementRange;
                            var halfUnitZ = mapUnitZ * 0.5f;

                            var scale = Vector3.ScaleIdentity * (bgItemsRandom.NextSingle() * add.ScaleRange + add.ScaleMin);
                            var mapLoc = location;
                            var keyAsset = add.Asset;
                            var sprite = keyAsset.CreateSprite();
                            mapLoc.x += bgItemsRandom.NextSingle() * mapUnitX - halfUnitX;
                            if (keyAsset.GroundAttachmentChannel.HasValue)
                            {
                                var groundOffset = sprite.GetCurrentFrame().Attachments[keyAsset.GroundAttachmentChannel.Value].translate;
                                mapLoc += groundOffset * scale * sprite.BaseScale;
                            }
                            var zOffsetBucket = bgItemsRandom.Next(9);
                            if (mustBeEven)
                            {
                                if (zOffsetBucket % 2 != 0)
                                {
                                    zOffsetBucket += 1;
                                }
                            }
                            else //Odd
                            {
                                if (zOffsetBucket % 2 != 1)
                                {
                                    zOffsetBucket += 1;
                                }
                            }
                            mapLoc.z += zOffsetBucket * 0.1f * mapUnitZ - halfUnitZ;

                            o.MapOffset = mapLoc;
                            o.Translation = o.MapOffset;
                            o.Sprite = keyAsset.CreateSprite();
                            o.SpriteMaterial = keyAsset.CreateMaterial();
                            o.Scale = scale;
                        });
                        this.bgItems.Add(bgItem);
                    }
                }
            }
        }

        public IBattleTarget ValidateTarget(IBattleTarget attacker, IBattleTarget target)
        {
            //Make sure target still exists, this will not handle the enemies list being empty
            switch (target.BattleTargetType)
            {
                case BattleTargetType.Enemy:
                    if (!enemies.Contains(target))
                    {
                        if (enemies.Count > 0)
                        {
                            target = enemies[targetRandom.Next(enemies.Count)];
                        }
                        else
                        {
                            target = null;
                        }
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

        public IBattleTarget GetGuard(IBattleTarget attacker, IBattleTarget target)
        {
            IBattleTarget guard = null;

            switch (target.BattleTargetType)
            {
                case BattleTargetType.Enemy:
                    break;
                    
                case BattleTargetType.Player:
                    guard = players
                        .Where(i => !i.IsDead && i != target && i.Stats.CanBlock && !i.IsDefending)
                        .OrderByDescending(i => i.Stats.CurrentHp)
                        .FirstOrDefault();

                    //Since the target is ignored above, make sure the guard
                    //is actually healthier than the target if the target can
                    //block for themselves.
                    if(guard != null)
                    {
                        if (target.Stats.CanBlock)
                        {
                            if(target.Stats.CurrentHp > guard.Stats.CurrentHp)
                            {
                                guard = null;
                            }
                        }
                    }

                    break;        
            }

            return guard;
        }

        public void Attack(IBattleTarget attacker, IBattleTarget target, bool isCounter, bool blocked, bool fumbleBlock, bool isPower, bool triggered, bool triggerSpammed)
        {
            target = ValidateTarget(attacker, target);

            if (damageCalculator.PhysicalHit(attacker.Stats, target.Stats))
            {
                var weaponSet = HammerSoundEffects.Instance;
                ISoundEffect soundEffect = null;
                ISoundEffect blockedSound = null;
                Element? magicElement = null;
                var dualHit = false;

                var hitEffect = battleAssetLoader.NormalHit;
                var damage = damageCalculator.Physical(attacker.Stats, target.Stats, 16);
                var randomizeDamage = true;
                var numShakes = 1;

                foreach (var attackElement in attacker.Stats.AttackElements)
                {
                    switch (attackElement)
                    {
                        case Element.Piercing:
                            weaponSet = SpearSoundEffects.Instance;
                            break;
                        case Element.Slashing:
                            weaponSet = SwordSoundEffects.Instance;
                            break;
                        case Element.Bludgeoning:
                            weaponSet = HammerSoundEffects.Instance;
                            break;
                    }

                    var resistance = target.Stats.GetResistance(attackElement);
                    damage = damageCalculator.ApplyResistance(damage, resistance);
                    switch (resistance)
                    {
                        case Resistance.Resist:
                            hitEffect = battleAssetLoader.BlockedHit;
                            soundEffect = soundEffect ?? weaponSet.Blocked;
                            numShakes = 0;
                            break;
                        case Resistance.Weak:
                            hitEffect = battleAssetLoader.StrongHit;
                            soundEffect = soundEffect ?? weaponSet.Heavy;
                            numShakes = 5;
                            break;
                        case Resistance.Death:
                        case Resistance.Recovery:
                            //This is enough to handle death and recovery for any element.
                            //The damage returned will be the number needed to apply the effect
                            //and we just want to stop modifying it immediately and apply it
                            randomizeDamage = false;
                            break;
                    }

                    switch (attackElement)
                    {
                        case Element.Piercing:
                        case Element.Slashing:
                        case Element.Bludgeoning:
                            soundEffect = soundEffect ?? weaponSet.Normal;
                            break;

                        case Element.Fire:
                        case Element.Ice:
                        case Element.Electricity:
                            magicElement = attackElement;
                            break;
                    }
                }

                soundEffect = soundEffect ?? attacker.DefaultAttackSoundEffect;

                bool isCritical = false;
                var color = Color.White;
                if (randomizeDamage)
                {
                    damage = damageCalculator.RandomVariation(damage);

                    isCritical = damageCalculator.CriticalHit(attacker.Stats, target.Stats);
                    if (isCritical)
                    {
                        damage *= 2;
                        color = Color.Orange;
                    }
                    if (isPower)
                    {
                        damage *= 2;
                    }
                    if (blocked)
                    {
                        damage -= (long)(damage * target.Stats.BlockDamageReduction);
                        color = Color.Grey;
                        hitEffect = battleAssetLoader.BlockedHit;
                        blockedSound = BlockedSoundEffect.Instance;
                        numShakes = 0;
                    }
                    if (fumbleBlock)
                    {
                        damage += (long)(damage * 0.25f);
                        color = Color.Red;
                        hitEffect = battleAssetLoader.StrongHit;
                        numShakes = 5;
                    }

                    if (triggered)
                    {
                        if (attacker.Stats.CanTriggerAttack)
                        {
                            damage += (long)(damage * 0.5f);
                            dualHit = true;
                            numShakes += 2;
                        }
                        else //Penalized if you can't trigger
                        {
                            damage -= (long)(damage * 0.5f);
                            color = Color.Grey;
                            numShakes = 0;
                        }
                    }

                    if (triggerSpammed)
                    {
                        damage -= (long)(damage * 0.5f);
                        color = Color.Grey;
                        numShakes = 0;
                    }

                    if (target.IsDefending)
                    {
                        damage -= (long)(damage * 0.5f);
                        color = Color.LightBlue;
                        numShakes = 0;
                    }
                }

                //With the way enemy positions are tracked, we can't shake during a counter attack
                if (isCounter)
                {
                    numShakes = 0;
                }

                AddDamageNumber(target, damage, color);
                ShowHit(target, hitEffect, soundEffect, blockedSound, magicElement, dualHit, numShakes);
                target.ApplyDamage(attacker, damageCalculator, damage);
                var attackerIsPlayer = players.Contains(attacker);
                var targetIsPlayer = players.Contains(target);
                if (!isCounter && !target.IsDead && attackerIsPlayer != targetIsPlayer && damageCalculator.Counter(attacker.Stats, target.Stats))
                {
                    target.AttemptMeleeCounter(attacker);
                }

                HandleDeath(target);
            }
            else
            {
                AddDamageNumber(target, "Miss", Color.White);
            }
        }

        private readonly Vector3 HitScale = Vector3.ScaleIdentity * 0.5f;
        private readonly Vector3 SecondaryHitScale = Vector3.ScaleIdentity * 0.4f;

        private void ShowHit(IBattleTarget target, ISpriteAsset asset, ISoundEffect soundEffect, ISoundEffect blockedSound, Element? magicElement, bool dualHit, int numShakes)
        {
            var applyEffects = new List<Attachment<BattleScene>>();

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
            {
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });
            
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, HitScale);
            applyEffects.Add(applyEffect);

            if (dualHit)
            {
                applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                {
                    o.RenderShadow = false;
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                });
                applyEffect.SetPosition(target.MagicHitLocation - new Vector3(0.2f, 0f, 0f), Quaternion.Identity, SecondaryHitScale);
                applyEffects.Add(applyEffect);
            }

            if (soundEffect != null)
            {
                soundEffectPlayer.PlaySound(soundEffect);
            }

            if(blockedSound != null)
            {
                soundEffectPlayer.PlaySound(blockedSound);
            }

            ISoundEffect elementalSound = null;
            ISpriteAsset elementalEffect = null;

            if (magicElement != null)
            {
                switch (magicElement)
                {
                    case Element.Fire:
                        elementalSound = FireSpellSoundEffect.Instance;
                        elementalEffect = new FireEffect();
                        break;
                    case Element.Ice:
                        elementalSound = IceSpellSoundEffect.Instance;
                        elementalEffect = new IceEffect();
                        break;
                    case Element.Electricity:
                        elementalSound = LightningSpellSoundEffect.Instance;
                        elementalEffect = new ElectricEffect();
                        break;
                }
            }

            if(elementalSound != null)
            {
                soundEffectPlayer.PlaySound(elementalSound);
            }

            if(elementalEffect != null)
            {
                applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                {
                    o.RenderShadow = false;
                    o.Sprite = elementalEffect.CreateSprite();
                    o.SpriteMaterial = elementalEffect.CreateMaterial();
                    o.Light = new Light
                    {
                        Color = ElementColors.GetElementalColor(magicElement.Value),
                        Length = 2.3f,
                    };
                    o.LightOffset = new Vector3(0, 0, -0.1f);
                });
                applyEffect.SetPosition(target.MagicHitLocation + new Vector3(-0.1f, 0.2f, 0f), Quaternion.Identity, SecondaryHitScale);
                applyEffects.Add(applyEffect);
            }

            IEnumerator<YieldAction> run()
            {
                const double waitTime = 0.075f;
                var totalSeconds = 0.55;
                var shook = true;
                for(var i = 0; i < numShakes; ++i)
                {
                    target.SetShakePosition(shook);
                    shook = !shook;
                    totalSeconds -= waitTime;
                    yield return coroutine.WaitSeconds(waitTime);
                }
                if (numShakes > 0)
                {
                    target.SetShakePosition(false);
                }
                yield return coroutine.WaitSeconds(totalSeconds);
                foreach (var effect in applyEffects)
                {
                    effect.RequestDestruction();
                }
            }
            coroutine.Run(run());
        }

        private void ShowEnemyDeath(IBattleTarget target)
        {
            var applyEffects = new List<Attachment<BattleScene>>();

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
            {
                ISpriteAsset asset = new EnemyDeathEffect();
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, target.EffectScale);
            applyEffects.Add(applyEffect);

            soundEffectPlayer.PlaySound(DeathSoundEffect.Instance);

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(.95);
                foreach (var effect in applyEffects)
                {
                    effect.RequestDestruction();
                }
            }
            coroutine.Run(run());
        }

        public void HandleDeath(IBattleTarget target)
        {
            if (target.IsDead)
            {
                if (enemies.Contains(target))
                {
                    var enemy = target as Enemy;
                    var enemyIndex = enemies.IndexOf(enemy);
                    if (cursor.EnemyTargetIndex > enemyIndex)
                    {
                        --cursor.EnemyTargetIndex;
                    }
                    enemies.RemoveAt(enemyIndex);

                    var fanfareDone = false; //This does nothing unless the enemies are all dead, but it must be declared here for scope
                    if (enemies.Count == 0)
                    {
                        //Add a turn that will hold the turn queue up until the coroutine below is complete
                        turnQueue.Insert(0, new TurnQueueEntry()
                        {
                            Turn = c => fanfareDone,
                            Priority = true
                        });
                    }

                    IEnumerator<YieldAction> run()
                    {
                        yield return coroutine.WaitSeconds(0.45);
                        ShowEnemyDeath(target);
                        target.RequestDestruction();
                        killedEnemies.Add(enemy);
                        if(enemies.Count == 0)
                        {
                            yield return coroutine.WaitSeconds(0.4);
                            backgroundMusicPlayer.SetBackgroundSong(VictoryMusic.File);
                            foreach (var player in players)
                            {
                                player.SetVictorious();
                            }
                            BattleEnded();
                            allowBattleFinish = true;

                            const double END_BATTLE_WAIT_TIME = 1.85;

                            if (nextLevel != null)
                            {
                                const double EFFECT_WAIT_TIME = 0.4;
                                const double REMAINING_TIME = END_BATTLE_WAIT_TIME - EFFECT_WAIT_TIME - RestoreMpEffect.Duration;

                                yield return coroutine.WaitSeconds(EFFECT_WAIT_TIME);

                                soundEffectPlayer.PlaySound(LevelBoostSoundEffect.Instance);

                                var applyEffects = new List<Attachment<BattleScene>>();
                                foreach (var player in players)
                                {
                                    var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                                    {
                                        var asset = new StatBoostEffect();
                                        o.RenderShadow = false;
                                        o.Sprite = asset.CreateSprite();
                                        o.SpriteMaterial = asset.CreateMaterial();
                                        o.Light = new Light
                                        {
                                            Color = Items.Actions.LevelBoost.CastColor,
                                            Length = 2.3f,
                                        };
                                        o.LightOffset = new Vector3(0, 0, -0.1f);
                                    });
                                    applyEffect.SetPosition(player.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);
                                    applyEffects.Add(applyEffect);
                                }

                                yield return coroutine.WaitSeconds(RestoreMpEffect.Duration);

                                foreach (var effect in applyEffects)
                                {
                                    effect.RequestDestruction();
                                }

                                yield return coroutine.WaitSeconds(REMAINING_TIME);
                            }
                            else
                            {
                                yield return coroutine.WaitSeconds(END_BATTLE_WAIT_TIME);
                            }

                            showEndBattleButton = true;
                            fanfareDone = true;
                        }
                    }
                    coroutine.Run(run());
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
            AddDamageNumber(target, damage, Color.White);
        }

        public void AddDamageNumber(IBattleTarget target, long damage, Color color)
        {
            if(damage < 0)
            {
                color = Color.Green;
                damage *= -1;
            }
            AddDamageNumber(target, damage.ToString(), color);
        }

        public void AddDamageNumber(IBattleTarget target, String damage, Color color)
        {
            var targetPos = target.DamageDisplayLocation;
            var screenPos = cameraProjector.Project(targetPos);

            numbers.Add(new DamageNumber(damage.ToString(), screenPos, scaleHelper, color));
        }

        public void DeactivateCurrentPlayer()
        {
            activePlayers.Dequeue();
        }

        public void QueueTurn(Func<Clock, bool> turn, bool queueFront = false)
        {
            if (queueFront)
            {
                var i = 0;
                for (; i < turnQueue.Count; ++i)
                {
                    var item = this.turnQueue[i];
                    if (!item.Priority)
                    {
                        ++item.SkipCount;

                        if(item.SkipCount > 3)
                        {
                            item.Priority = true;
                            //Go to the next item and set it as the answer by breaking
                            //This prevents skipCount for incrementing for that item too
                            ++i;
                        }

                        break;
                    }
                }

                var entity = new TurnQueueEntry()
                {
                    Turn = turn,
                    Priority = true
                };
                
                if (i < turnQueue.Count)
                {
                    this.turnQueue.Insert(i, entity);
                }
                else
                {
                    this.turnQueue.Add(entity);
                }
            }
            else
            {
                this.turnQueue.Add(new TurnQueueEntry()
                {
                    Turn = turn,
                    Priority = false
                });
            }
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
                currentTurn = null; //If all players died clear the current turn, this is player only, enemies are different
                BattleEnded();
            }
        }

        private void BattleEnded()
        {
            cursor.Cancel();
            turnQueue.Clear();
            activePlayers.Clear();
        }

        public IDamageCalculator DamageCalculator => damageCalculator;

        public IEnumerable<ITreasure> Steal()
        {
            return stealCb?.Invoke() ?? Enumerable.Empty<ITreasure>();
        }

        public IBattleTarget GetTargetForStats(IBattleStats stats)
        {
            return players.FirstOrDefault(i => i.Stats == stats) as IBattleTarget ?? enemies.FirstOrDefault(i => i.Stats == stats);
        }

        public void CancelTargeting()
        {
            cursor.Cancel();
        }

        public BattlePlayer GetPlayerForTarget(IBattleStats stats)
        {
            return players.FirstOrDefault(i => i.Stats == stats);
        }
    }
}
