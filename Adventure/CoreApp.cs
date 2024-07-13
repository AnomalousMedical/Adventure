using Adventure.Assets;
using Adventure.Battle;
using Adventure.Exploration;
using Adventure.GameOver;
using Adventure.Items;
using Adventure.Items.Actions;
using Adventure.Items.Creators;
using Adventure.Menu;
using Adventure.Services;
using Adventure.Skills;
using Adventure.Text;
using Adventure.WorldMap;
using Anomalous.OSPlatform;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RpgMath;
using System;
using System.IO;

namespace Adventure
{
    public class CoreApp : App
    {
        private GameOptionsWriter optionsWriter = new GameOptionsWriter();
        private NativeOSWindow mainWindow;
        private UpdateTimer mainTimer;

        public CoreApp()
        {
            
        }

        public override void Dispose()
        {
            optionsWriter.Dispose();
            PerformanceMonitor.destroyEnabledState();

            base.DisposeGlobalScope();
            EasyNativeWindow.Destroy(mainWindow);
            base.FinalDispose();
        }

        public override bool OnInit(IServiceCollection services, PluginManager pluginManager)
        {
            var options = optionsWriter.Load();

            mainWindow = EasyNativeWindow.Create(services, this, o =>
            {
                o.Title = "Anomalous Adventure";
                o.Fullscreen = options.Fullscreen;
            });

            services.AddLogging(o =>
            {
                o.AddSimpleConsole(o =>
                {
                    o.SingleLine = true;
                    o.TimestampFormat = "HH:mm:ss:fffff ";
                });
            });

            services.AddDiligentEngine(pluginManager, o =>
            {
                o.Features = DiligentEngine.GraphicsEngine.FeatureFlags.RayTracing;
                o.RenderApi = options.RenderApi;
                o.DeviceId = options.DeviceId;
                o.UpsamplingMethod = options.UpsamplingMethod;
                o.FSR1RenderPercentage = options.FSR1RenderPercentage;
            })
            .AddDiligentEngineRt();

            services.AddOSPlatform(pluginManager, o =>
            {
                o.LayerKeys = Enum.GetValues<EventLayers>();
            });
            services.AddSoundPlugin(pluginManager, o =>
            {
                o.MasterVolume = options.MasterVolume;
            });
            services.AddSharpGui();
            services.AddFirstPersonFlyCamera(o =>
            {
                o.EventLayer = EventLayers.Exploration;
            });
            services.AddBepuPlugin<ZoneScene>(o =>
            {
                o.Gravity = new System.Numerics.Vector3(0f, -100f, 0f);
            })
            .AddBepuSceneType<WorldMapScene>();

            services.AddRpgMath();

            //Add this app's services
            services.AddSingleton<EventManagerTracker>();
            services.AddSingleton<GameOptions>(options);
            services.AddSingleton<FlyCameraManager>();
            services.AddSingleton<GameUpdateListener>();
            services.AddSingleton<ITimeClock, TimeClock>();
            services.AddSingleton<PlayedTimeService>();
            services.AddSingleton<BuffManager>();
            services.AddSingleton<IDebugGui, DebugGui>();
            services.AddSingleton<IRootMenu, RootMenu>();
            services.AddSingleton<IBattleGameState, BattleGameState>();
            services.AddSingleton<IGameOverGameState, GameOverGameState>();
            services.AddSingleton<ISetupRespawnGameState, SetupRespawnGameState>();
            services.AddSingleton<IWorldMapGameState, WorldMapGameState>();
            services.AddSingleton<IWorldMapManager, WorldMapManager>();
            services.AddSingleton<IStartExplorationGameState, StartExplorationGameState>();
            services.AddSingleton<PartyMemberManager>();
            services.AddSingleton<CharacterStatsTextService>();
            services.AddSingleton<CharacterStyleService>();
            services.AddSingleton<IGcService, GcService>();
            services.AddScoped<IInventoryFunctions>(s => new InventoryFunctions(s));
            services.AddScoped<FollowerManager>();
            services.AddScoped(typeof(Follower<>));
            services.AddScoped<FollowerDescription>();
            services.AddScoped<WorldMapPlayer>();
            services.AddScoped<WorldMapPlayer.Description>();
            services.AddScoped<Airship>();
            services.AddScoped<Airship.Description>();
            services.AddScoped<Player>();
            services.AddScoped<Player.Description>();
            services.AddSingleton<BattleAssetLoader>();
            services.AddScoped<BattlePlayer>();
            services.AddScoped<BattlePlayer.Description>();
            services.AddScoped<Enemy>();
            services.AddScoped<Enemy.Desc>();
            services.AddSingleton<ISoundEffectPlayer, SoundEffectPlayer>();
            services.AddSingleton<RTInstances<ZoneScene>>();
            services.AddSingleton<RTInstances<BattleScene>>();
            services.AddSingleton<RTInstances<WorldMapScene>>();
            services.AddSingleton<RTInstances<EmptyScene>>();
            services.AddSingleton<IGameStateRequestor, GameStateRequestor>();
            services.AddSingleton<EquipmentTextService>();
            services.AddSingleton<RestMenu>();
            services.AddScoped<IAttachment.Description>();
            services.AddScoped<Attachment<ZoneScene>>();
            services.AddScoped<Attachment<BattleScene>>();
            services.AddScoped<Attachment<WorldMapScene>>();
            services.AddScoped<IBattleBuilder, BattleBuilder>();
            services.AddScoped<Zone>();
            services.AddScoped<Zone.Description>();
            services.AddScoped<ZoneConnector>();
            services.AddScoped<ZoneConnector.Description>();
            services.AddScoped<BlacksmithUpgrade>();
            services.AddScoped<BlacksmithUpgrade.Description>();
            services.AddScoped<AlchemistUpgrade>();
            services.AddScoped<AlchemistUpgrade.Description>();
            services.AddScoped<Innkeeper>();
            services.AddScoped<Innkeeper.Description>();
            services.AddScoped<Blacksmith>();
            services.AddScoped<Blacksmith.Description>();
            services.AddScoped<AirshipEngineer>();
            services.AddScoped<AirshipEngineer.Description>();
            services.AddScoped<Alchemist>();
            services.AddScoped<Alchemist.Description>();
            services.AddScoped<FortuneTeller>();
            services.AddScoped<FortuneTeller.Description>();
            services.AddScoped<WorldMapProp>();
            services.AddScoped<WorldMapProp.Description>();
            services.AddScoped<ElementalStone>();
            services.AddScoped<ElementalStone.Description>();
            services.AddScoped<WorldWater>();
            services.AddScoped<WorldWater.Description>();
            services.AddScoped<WorldMapInstance>();
            services.AddScoped<WorldMapInstance.Description>();
            services.AddScoped<ZoneEntrance>();
            services.AddScoped<ZoneEntrance.Description>();
            services.AddScoped<BattleTrigger>();
            services.AddScoped<BattleTrigger.Description>();
            services.AddScoped<TreasureTrigger>();
            services.AddScoped<TreasureTrigger.Description>();
            services.AddScoped<PartyMemberTrigger>();
            services.AddScoped<PartyMemberTrigger.Description>();
            services.AddSingleton<PartyMemberTriggerManager>();
            services.AddScoped<Gate>();
            services.AddScoped<Gate.Description>();
            services.AddScoped<Torch>();
            services.AddScoped<Torch.Description>();
            services.AddScoped<Key>();
            services.AddScoped<Key.Description>();
            services.AddScoped<HelpBook>();
            services.AddScoped<HelpBook.Description>();
            services.AddScoped<PlotItemPlaceable>();
            services.AddScoped<PlotItemPlaceable.Description>();
            services.AddScoped<BackgroundItem>();
            services.AddScoped<BackgroundItem.Description>();
            services.AddScoped<BattleBackgroundItem>();
            services.AddScoped<BattleBackgroundItem.Description>();
            services.AddScoped<RestArea>();
            services.AddScoped<RestArea.Description>();
            services.AddScoped<LootDropTrigger>();
            services.AddScoped<LootDropTrigger.Description>();
            services.AddScoped<Sky>();
            services.AddScoped<TargetCursor>();
            services.AddScoped<IBattleSkills, BattleSkills>();
            services.AddScoped<BattleItemMenu>();
            services.AddScoped<StealEffect>();
            services.AddScoped<StealEffect.Description>();
            services.AddScoped<AttackEffect>();
            services.AddSingleton<TreasureMenu>();
            services.AddSingleton<LightManager>();
            services.AddSingleton<IZoneManager, ZoneManager>();
            services.AddSingleton<IWorldManager, WorldManager>();
            services.AddSingleton<IWorldDatabase, WorldDatabase>();
            services.AddSingleton<IBattleManager, BattleManager>();
            services.AddSingleton<IMonsterMaker, MonsterMaker>();
            services.AddSingleton<IResetGameState, ResetGameState>();
            services.AddSingleton<TextDialog>();
            services.AddScoped<BattleArena>();
            services.AddScoped<BattleArena.Description>();
            services.AddSingleton<IBiomeManager, BiomeManager>();
            services.AddSingleton<IGameStateLinker, GameStateLinker>();
            services.AddSingleton<CameraMover>();
            services.AddSingleton<ICollidableTypeIdentifier<ZoneScene>, CollidableTypeIdentifier<ZoneScene>>();
            services.AddSingleton<ICollidableTypeIdentifier<WorldMapScene>, CollidableTypeIdentifier<WorldMapScene>>();
            services.AddSingleton<IBackgroundMusicPlayer, BackgroundMusicPlayer>();
            services.AddSingleton<ICameraProjector, CameraProjector>();
            services.AddSingleton<IBattleScreenLayout, BattleScreenLayout>();
            services.AddSingleton<IFirstGameStateBuilder, FirstGameStateBuilder>();
            services.AddSingleton<IExplorationGameState, ExplorationGameState>();
            services.AddSingleton<IAssetFactory, AssetFactory>();
            services.AddSingleton<ISkillFactory, SkillFactory>();
            services.AddSingleton<GraphicsOptionsMenu>();
            services.AddSingleton<SoundOptionsMenu>();
            services.AddSingleton<ConfirmMenu>();
            services.AddSingleton<Party>();
            services.AddSingleton<ISetupGameState, SetupGameState>();
            services.AddSingleton<IExplorationMenu, ExplorationMenu>();
            services.AddSingleton<PlotItemMenu>();
            services.AddSingleton<IClockService, ClockService>();
            services.AddSingleton<IContextMenu, ContextMenu>();
            services.AddSingleton<IAnimationService<WorldMapScene>, AnimationService<WorldMapScene>>();
            services.AddSingleton<ItemVoidMenu>();
            services.AddScoped<EndGameTrigger>();
            services.AddScoped<EndGameTrigger.Description>();
            services.AddScoped<ItemStorage>();
            services.AddScoped<ItemStorage.Description>();
            services.AddScoped<GoldPile>();
            services.AddScoped<GoldPile.Description>();
            services.AddSingleton<ILanguageService>(s => new LanguageService(EnglishLanguage.Create()));
#if !RELEASE
            services.AddSingleton<ISeedProvider>(s => new ConstantSeedProvider(0)); //Set to 0 for debugging
#else
            services.AddSingleton<ISeedProvider, RandomSeedProvider>();
#endif
            services.AddSingleton<IPersistenceWriter, PersistenceWriter>();
            services.AddSingleton<IGenesysModule, GenesysModule>();
            services.AddSingleton<IEquipmentCurve, StandardEquipmentCurve>();
            services.AddSingleton<ItemMenu>();
            services.AddSingleton<UseItemMenu>();
            services.AddSingleton<SkillMenu>();
            services.AddSingleton<PlayerMenu>();
            services.AddSingleton<SwordCreator>();
            services.AddSingleton<SpearCreator>();
            services.AddSingleton<MaceCreator>();
            services.AddSingleton<ShieldCreator>();
            services.AddSingleton<TypedLightManager<ZoneScene>>();
            services.AddSingleton<TypedLightManager<WorldMapScene>>();
            services.AddSingleton<TypedLightManager<BattleScene>>();
            services.AddSingleton<AccessoryCreator>();
            services.AddSingleton<ArmorCreator>();
            services.AddSingleton<PotionCreator>();
            services.AddSingleton<BookCreator>();
            services.AddSingleton<DaggerCreator>();
            services.AddSingleton<ElementalStaffCreator>();
            services.AddSingleton<BuyMenu>();
            services.AddSingleton<ConfirmBuyMenu>();
            services.AddSingleton<ChooseCharacterMenu>();
            services.AddSingleton<FadeScreenMenu>();
            services.AddSingleton<RestManager>();
            services.AddSingleton<PickUpTreasureMenu>();
            services.AddSingleton<StealMenu>();
            services.AddSingleton<OptionsMenu>();
            services.AddSingleton<FileMenu>();
            services.AddSingleton<CreditsMenu>();
            services.AddSingleton<App>(this);
            services.AddSingleton<Persistence>();
            services.AddSingleton<EarthquakeMenu>();
            services.AddSingleton<HelpMenu>();
            services.AddSingleton<PlayerCage<ZoneScene>>();
            services.AddSingleton<PlayerCage<ZoneScene>.Description>(new PlayerCage<ZoneScene>.Description());
            services.AddSingleton<PlayerCage<WorldMapScene>>();
            services.AddSingleton<PlayerCage<WorldMapScene>.Description>(new PlayerCage<WorldMapScene>.Description()
            {
                PlayerAreaSize = 3.0f
            });

            services.AddSingleton<ICharacterMenuPositionTracker<ZoneScene>, CharacterMenuPositionTracker<ZoneScene>>();
            services.AddSingleton<ICharacterMenuPositionTracker<WorldMapScene>>(s => s.GetRequiredService<WrappingCharacterMenuPositionTracker<WorldMapScene>>());
            services.AddSingleton<WrappingCharacterMenuPositionTracker<WorldMapScene>>(s => new WrappingCharacterMenuPositionTracker<WorldMapScene>(new CharacterMenuPositionTracker<WorldMapScene>()));
            services.AddSingleton<CharacterMenuPositionService>();
            services.AddSingleton<ICreditsService, CreditsService>();
            services.AddSingleton<UserInputMenu>();
            services.AddSingleton<FontLoader>();
            services.AddSingleton<IVictoryGameState, VictoryGameState>();
            services.AddSingleton<MultiCameraMover<ZoneScene>>();
            services.AddSingleton<MultiCameraMover<ZoneScene>.Description>(new MultiCameraMover<ZoneScene>.Description());
            services.AddSingleton<MultiCameraMover<WorldMapScene>>();
            services.AddSingleton<MultiCameraMover<WorldMapScene>.Description>(new MultiCameraMover<WorldMapScene>.Description() 
            {
                cameraOffset = new Vector3(0, 3, -12),
                cameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 15f) 
            });

            //Add Item Actions
            services.AddTransient<EquipMainHand>();
            services.AddTransient<EquipOffHand>();
            services.AddTransient<EquipAccessory>();
            services.AddTransient<EquipBody>();
            services.AddTransient<LevelBoost>();
            services.AddTransient<StrengthBoost>();
            services.AddTransient<MagicBoost>();
            services.AddTransient<SpiritBoost>();
            services.AddTransient<VitalityBoost>();
            services.AddTransient<DexterityBoost>();
            services.AddTransient<LuckBoost>();
            services.AddTransient<RestoreHp>();
            services.AddTransient<RestoreMp>();
            services.AddTransient<Revive>();

            return true;
        }

        public override bool OnLink(IServiceProvider serviceProvider)
        {
            var log = serviceProvider.GetRequiredService<ILogger<CoreApp>>();
            log.LogInformation("Running from directory {0}", FolderFinder.ExecutableFolder);

            //Setup virtual file system
            var vfs = serviceProvider.GetRequiredService<VirtualFileSystem>();

            //This needs to be less hardcoded.
            var assetPath = Path.GetFullPath(Path.Combine(FolderFinder.ExecutableFolder, "AdventureAssets"));
            if (!Directory.Exists(assetPath))
            {
                //If no local assets, load from dev location, try for both self-contained and normal
                assetPath = Path.GetFullPath("../../../../../../AdventureAssets");
                if (!Directory.Exists(assetPath))
                {
                    assetPath = Path.GetFullPath("../../../../../AdventureAssets");

                    if (!Directory.Exists(assetPath))
                    {
                        throw new InvalidOperationException("Cannot find AdventureAssets");
                    }
                }
            }
            vfs.addArchive(assetPath);

            mainTimer = serviceProvider.GetRequiredService<UpdateTimer>();

            var linker = serviceProvider.GetRequiredService<IGameStateLinker>(); //This links the game states together.
            var updateListener = serviceProvider.GetRequiredService<GameUpdateListener>();
            mainTimer.addUpdateListener(updateListener);

            PerformanceMonitor.setupEnabledState(serviceProvider.GetRequiredService<SystemTimer>());

            serviceProvider.GetRequiredService<EventManagerTracker>();

            return true;
        }

        public override int OnExit()
        {
            return 0;
        }

        public override void OnIdle()
        {
            mainTimer?.OnIdle();
        }
    }
}
