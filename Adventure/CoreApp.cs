using Adventure.Assets;
using Adventure.Battle;
using Adventure.Battle.Skills;
using Adventure.Exploration;
using Adventure.GameOver;
using Adventure.Items;
using Adventure.Items.Actions;
using Adventure.Items.Creators;
using Adventure.Menu;
using Adventure.Services;
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
        private OptionsWriter optionsWriter = new OptionsWriter();
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
                o.AddConsole();
            });

            services.AddDiligentEngine(pluginManager, o =>
            {
                o.Features = DiligentEngine.GraphicsEngine.FeatureFlags.RayTracing;
                o.RenderApi = options.RenderApi;
            })
            .AddDiligentEngineRt();

            services.AddOSPlatform(pluginManager, o =>
            {
                o.EventLayersType = typeof(EventLayers);
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
            services.AddBepuPlugin<ZoneScene>()
                    .AddBepuSceneType<WorldMapScene>();

            services.AddRpgMath();

            //Add this app's services
            services.AddSingleton<Options>(options);
            services.AddSingleton<FlyCameraManager>();
            services.AddSingleton<SceneTestUpdateListener>();
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
            services.AddScoped<Attachment<ZoneScene>>();
            services.AddScoped<Attachment<ZoneScene>.Description>();
            services.AddScoped<Attachment<BattleScene>>();
            services.AddScoped<Attachment<BattleScene>.Description>();
            services.AddScoped<Attachment<WorldMapScene>>();
            services.AddScoped<Attachment<WorldMapScene>.Description>();
            services.AddScoped<IBattleBuilder, BattleBuilder>();
            services.AddScoped<Zone>();
            services.AddScoped<Zone.Description>();
            services.AddScoped<ZoneConnector>();
            services.AddScoped<ZoneConnector.Description>();
            services.AddScoped<StorePhilip>();
            services.AddScoped<StorePhilip.Description>();
            services.AddScoped<WorldWater>();
            services.AddScoped<WorldWater.Description>();
            services.AddScoped<IslandPortal>();
            services.AddScoped<IslandPortal.Description>();
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
            services.AddScoped<Gate>();
            services.AddScoped<Gate.Description>();
            services.AddScoped<Key>();
            services.AddScoped<Key.Description>();
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
            services.AddScoped<Philip>();
            services.AddScoped<Philip.Description>();
            services.AddScoped<IBattleSkills, BattleSkills>();
            services.AddScoped<BattleItemMenu>();
            services.AddScoped<StealEffect>();
            services.AddScoped<AttackEffect>();
            services.AddSingleton<TreasureMenu>();
            services.AddSingleton<LightManager>();
            services.AddSingleton<IZoneManager, ZoneManager>();
            services.AddSingleton<IWorldManager, WorldManager>();
            services.AddSingleton<IWorldDatabase, WorldDatabase>();
            services.AddSingleton<IBattleManager, BattleManager>();
            services.AddSingleton<IMonsterMaker, MonsterMaker>();
            services.AddSingleton<TextDialog>();
            services.AddScoped<BattleArena>();
            services.AddScoped<BattleArena.Description>();
            services.AddSingleton<IBiomeManager, BiomeManager>();
            services.AddSingleton<IGameStateLinker, GameStateLinker>();
            services.AddSingleton<CameraMover>();
            services.AddSingleton<ICollidableTypeIdentifier<IExplorationGameState>, CollidableTypeIdentifier<IExplorationGameState>>();
            services.AddSingleton<ICollidableTypeIdentifier<WorldMapScene>, CollidableTypeIdentifier<WorldMapScene>>();
            services.AddSingleton<IBackgroundMusicPlayer, BackgroundMusicPlayer>();
            services.AddSingleton<ICameraProjector, CameraProjector>();
            services.AddSingleton<IBattleScreenLayout, BattleScreenLayout>();
            services.AddSingleton<IFirstGameStateBuilder, FirstGameStateBuilder>();
            services.AddSingleton<IExplorationGameState, ExplorationGameState>();
            services.AddSingleton<ISimpleActivator, SimpleActivator>();
            services.AddSingleton<IAssetFactory, AssetFactory>();
            services.AddSingleton<ISkillFactory, SkillFactory>();
            services.AddSingleton<Party>();
            services.AddSingleton<ISetupGameState, SetupGameState>();
            services.AddSingleton<IExplorationMenu, ExplorationMenu>();
            services.AddSingleton<IContextMenu, ContextMenu>();
#if !RELEASE
            services.AddSingleton<ISeedProvider>(s => new ConstantSeedProvider(0)); //Set to 0 for debugging
#else
            services.AddSingleton<ISeedProvider, RandomSeedProvider>();
#endif
            services.AddSingleton<IPersistenceWriter, PersistenceWriter>();
            services.AddSingleton<PhilipRootMenu>();
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
            services.AddSingleton<AccessoryCreator>();
            services.AddSingleton<ArmorCreator>();
            services.AddSingleton<PotionCreator>();
            services.AddSingleton<BookCreator>();
            services.AddSingleton<DaggerCreator>();
            services.AddSingleton<ElementalStaffCreator>();
            services.AddSingleton<BuyMenu>();
            services.AddSingleton<ConfirmBuyMenu>();
            services.AddSingleton<RestManager>();
            services.AddSingleton<PickUpTreasureMenu>();
            services.AddSingleton<OptionsMenu>();
            services.AddSingleton<App>(this);
            services.AddSingleton<Persistence>();

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
            var updateListener = serviceProvider.GetRequiredService<SceneTestUpdateListener>();
            mainTimer.addUpdateListener(updateListener);

            PerformanceMonitor.setupEnabledState(serviceProvider.GetRequiredService<SystemTimer>());

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
