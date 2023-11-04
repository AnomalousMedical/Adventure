using Adventure.Assets;
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
        void SetupBattle(int battleSeed, int level, bool boss, Func<IEnumerable<ITreasure>> Steal, BiomeEnemy triggerEnemy);
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
    }

    class BattleManager : IDisposable, IBattleManager
    {
        const long NumberDisplayTime = (long)(0.9f * Clock.SecondsToMicro);

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
        private readonly IObjectResolver objectResolver;
        private BattleArena battleArena;
        private List<BattleBackgroundItem> bgItems = new List<BattleBackgroundItem>();

        private SharpButton endBattle = new SharpButton() { Text = "End Battle" };
        private SharpText goldRewardText = new SharpText() { Color = Color.White };

        private List<Enemy> enemies = new List<Enemy>(20);
        private List<Enemy> killedEnemies = new List<Enemy>(20);
        private List<BattlePlayer> players = new List<BattlePlayer>(4);
        private List<DamageNumber> numbers = new List<DamageNumber>(10);
        private Queue<BattlePlayer> activePlayers = new Queue<BattlePlayer>(4);
        private List<Func<Clock, bool>> turnQueue = new List<Func<Clock, bool>>(30);
        private Func<Clock, bool> currentTurn = null;
        bool allowBattleFinish = false;
        bool showEndBattleButton = false;

        private TargetCursor cursor;

        private Random targetRandom = new Random();
        private String backgroundMusic;
        private Func<IEnumerable<ITreasure>> stealCb;

        public bool AllowActivePlayerGui { get; set; } = true;

        private List<SharpStyle> characterStyles;

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
            IBattleBuilder battleBuilder,
            BuffManager buffManager,
            IScopedCoroutine coroutine,
            BattleAssetLoader battleAssetLoader,
            ISoundEffectPlayer soundEffectPlayer)
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
            this.buffManager = buffManager;
            this.coroutine = coroutine;
            this.battleAssetLoader = battleAssetLoader;
            this.soundEffectPlayer = soundEffectPlayer;
            this.objectResolver = objectResolverFactory.Create();

            cursor = this.objectResolver.Resolve<TargetCursor>();

            zoneManager.ZoneChanged += ZoneManager_ZoneChanged;

            characterStyles = new List<SharpStyle>()
            {
                SharpStyle.CreateComplete(scaleHelper, OpenColor.Blue),
                SharpStyle.CreateComplete(scaleHelper, OpenColor.Grape),
                SharpStyle.CreateComplete(scaleHelper, OpenColor.Green),
                SharpStyle.CreateComplete(scaleHelper, OpenColor.Orange),
            };
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
            var styleIndex = 0;
            foreach (var character in party.ActiveCharacters)
            {
                players.Add(this.objectResolver.Resolve<BattlePlayer, BattlePlayer.Description>(c =>
                {
                    c.Translation = new Vector3(4, 0, currentZ);
                    c.CharacterSheet = character.CharacterSheet;
                    c.Inventory = character.Inventory;
                    c.PlayerSprite = character.PlayerSprite;
                    c.Gamepad = (GamepadId)character.Player;
                    c.UiStyle = characterStyles[styleIndex++ % characterStyles.Count];
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
            buffManager.Update(clock);

            var result = IBattleManager.Result.ContinueBattle;
            if (turnQueue.Count > 0)
            {
                if(currentTurn == null)
                {
                    currentTurn = turnQueue[0];
                }
                if (currentTurn.Invoke(clock))
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
                foreach(var location in battleArena.BgItemLocations())
                {
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
                            o.MapOffset = Vector3.Zero;
                            o.Translation = location + o.MapOffset;
                            var keyAsset = add.Asset;
                            o.Sprite = keyAsset.CreateSprite();
                            o.SpriteMaterial = keyAsset.CreateMaterial();
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
                var dualHit = false;

                var hitEffect = battleAssetLoader.NormalHit;
                var damage = damageCalculator.Physical(attacker.Stats, target.Stats, 16);
                var randomizeDamage = true;

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
                            break;
                        case Resistance.Weak:
                            hitEffect = battleAssetLoader.StrongHit;
                            soundEffect = soundEffect ?? weaponSet.Heavy;
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
                    }
                    if (fumbleBlock)
                    {
                        damage += (long)(damage * 0.25f);
                        color = Color.Red;
                        hitEffect = battleAssetLoader.StrongHit;
                    }

                    if (triggered)
                    {
                        if (attacker.Stats.CanTriggerAttack)
                        {
                            damage += (long)(damage * 0.5f);
                            dualHit = true;
                        }
                        else //Penalized if you can't trigger
                        {
                            damage -= (long)(damage * 0.5f);
                            color = Color.Grey;
                        }
                    }

                    if (triggerSpammed)
                    {
                        damage -= (long)(damage * 0.5f);
                        color = Color.Grey;
                    }

                    if (target.IsDefending)
                    {
                        damage -= (long)(damage * 0.5f);
                        color = Color.LightBlue;
                    }
                }

                AddDamageNumber(target, damage, color);
                ShowHit(target, hitEffect, soundEffect, blockedSound, dualHit);
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

        private void ShowHit(IBattleTarget target, ISpriteAsset asset, ISoundEffect soundEffect, ISoundEffect blockedSound, bool dualHit)
        {
            var applyEffects = new List<Attachment<BattleScene>>();

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
            {
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });
            
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, HitScale);
            applyEffects.Add(applyEffect);

            if (dualHit)
            {
                applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
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

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(0.5);
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

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
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
                    enemies.Remove(enemy);

                    var fanfareDone = false; //This does nothing unless the enemies are all dead, but it must be declared here for scope
                    if (enemies.Count == 0)
                    {
                        //Add a turn that will hold the turn queue up until the coroutine below is complete
                        turnQueue.Insert(0, c => fanfareDone);
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
                            backgroundMusicPlayer.SetBackgroundSong("Music/freepd/Alexander Nakarada - Fanfare X.ogg");
                            foreach (var player in players)
                            {
                                player.SetVictorious();
                            }
                            BattleEnded();
                            allowBattleFinish = true;
                            yield return coroutine.WaitSeconds(1.85);
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

            numbers.Add(new DamageNumber(damage.ToString(), NumberDisplayTime, screenPos, scaleHelper, color));
        }

        public void DeactivateCurrentPlayer()
        {
            activePlayers.Dequeue();
        }

        public void QueueTurn(Func<Clock, bool> turn, bool queueFront = false)
        {
            if (queueFront)
            {
                this.turnQueue.Insert(0, turn);
            }
            else
            {
                this.turnQueue.Add(turn);
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
    }
}
