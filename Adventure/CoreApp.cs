﻿using Anomalous.OSPlatform;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RpgMath;
using Adventure.Battle;
using Adventure.Exploration;
using Adventure.Exploration.Menu;
using Adventure.GameOver;
using Adventure.Services;
using System;
using System.Globalization;
using System.IO;
using Adventure.Exploration.Menu.Asimov;
using Adventure.Assets;
using Adventure.Items;
using Adventure.Items.Creators;
using Adventure.Battle.Skills;

namespace Adventure
{
    public class CoreApp : App
    {
        private NativeOSWindow mainWindow;
        private UpdateTimer mainTimer;

        public CoreApp()
        {

        }

        public override void Dispose()
        {
            PerformanceMonitor.destroyEnabledState();

            base.DisposeGlobalScope();
            EasyNativeWindow.Destroy(mainWindow);
            base.FinalDispose();
        }

        public override bool OnInit(IServiceCollection services, PluginManager pluginManager)
        {
            mainWindow = EasyNativeWindow.Create(services, this, o =>
            {
                o.Title = "Anomalous Adventure";
            });

            services.AddLogging(o =>
            {
                o.AddConsole();
            });

            services.AddDiligentEngine(pluginManager, o =>
            {
                o.Features = DiligentEngine.GraphicsEngine.FeatureFlags.RayTracing;
            })
            .AddDiligentEngineRt();

            services.AddOSPlatform(pluginManager, o =>
            {
                o.EventLayersType = typeof(EventLayers);
            });
            services.AddSoundPlugin(pluginManager, o =>
            {
                o.MasterVolume = 0.25f;
            });
            services.AddSharpGui();
            services.AddFirstPersonFlyCamera(o =>
            {
                o.EventLayer = EventLayers.Exploration;
            });
            services.AddBepuPlugin();
            services.AddRpgMath();

            //Add this app's services
            services.AddSingleton<FlyCameraManager>();
            services.AddSingleton<SceneTestUpdateListener>();
            services.AddSingleton<ITimeClock, TimeClock>();
            services.AddSingleton<IDebugGui, DebugGui>();
            services.AddSingleton<IRootMenu, RootMenu>();
            services.AddSingleton<IBattleGameState, BattleGameState>();
            services.AddSingleton<IGameOverGameState, GameOverGameState>();
            services.AddScoped<Player>();
            services.AddScoped<Player.Description>();
            services.AddScoped<BattlePlayer>();
            services.AddScoped<BattlePlayer.Description>();
            services.AddScoped<Enemy>();
            services.AddScoped<Enemy.Desc>();
            services.AddSingleton<ShaderPreloader>();
            services.AddSingleton<RTInstances<IZoneManager>>();
            services.AddSingleton<RTInstances<IBattleManager>>();
            services.AddScoped<Attachment<IZoneManager>>();
            services.AddScoped<Attachment<IZoneManager>.Description>();
            services.AddScoped<Attachment<IBattleManager>>();
            services.AddScoped<Attachment<IBattleManager>.Description>();
            services.AddScoped<IBattleBuilder, BattleBuilder>();
            services.AddScoped<Zone>();
            services.AddScoped<Zone.Description>();
            services.AddScoped<ZoneConnector>();
            services.AddScoped<ZoneConnector.Description>();
            services.AddScoped<BattleTrigger>();
            services.AddScoped<BattleTrigger.Description>();
            services.AddScoped<TreasureTrigger>();
            services.AddScoped<TreasureTrigger.Description>();
            services.AddScoped<Gate>();
            services.AddScoped<Gate.Description>();
            services.AddScoped<Key>();
            services.AddScoped<Key.Description>();
            services.AddScoped<RestArea>();
            services.AddScoped<RestArea.Description>();
            services.AddScoped<LootDropTrigger>();
            services.AddScoped<LootDropTrigger.Description>();
            services.AddScoped<Sky>();
            services.AddScoped<RTGui>();
            services.AddScoped<TargetCursor>();
            services.AddScoped<Asimov>();
            services.AddScoped<Asimov.Description>();
            services.AddScoped<IBattleSkills, BattleSkills>();
            services.AddScoped<BattleItemMenu>();
            services.AddScoped<StealEffect>();
            services.AddScoped<AttackEffect>();
            services.AddSingleton<TreasureMenu>();
            services.AddSingleton<IZoneManager, ZoneManager>();
            services.AddSingleton<IWorldManager, WorldManager>();
            services.AddSingleton<IBattleManager, BattleManager>();
            services.AddSingleton<INameGenerator, NameGenerator>();
            services.AddScoped<BattleArena>();
            services.AddScoped<BattleArena.Description>();
            services.AddSingleton<IBiomeManager, BiomeManager>();
            services.AddSingleton<IGameStateLinker, GameStateLinker>();
            services.AddSingleton<CameraMover>();
            services.AddSingleton<ICollidableTypeIdentifier, CollidableTypeIdentifier>();
            services.AddSingleton<IBackgroundMusicPlayer, BackgroundMusicPlayer>();
            services.AddSingleton<BackgroundMusicManager>();
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
            services.AddSingleton<IPersistenceWriter, PersistenceWriter>();
            services.AddSingleton<AsimovRootMenu>();
            services.AddSingleton<LevelUpMenu>();
            services.AddSingleton<IGenesysModule, GenesysModule>();
            services.AddSingleton<IEquipmentCurve, StandardEquipmentCurve>();
            services.AddSingleton<ItemMenu>();
            services.AddSingleton<UseItemMenu>();
            services.AddSingleton<SkillMenu>();
            services.AddSingleton<SwordCreator>();
            services.AddSingleton<SpearCreator>();
            services.AddSingleton<MaceCreator>();
            services.AddSingleton<ShieldCreator>();
            services.AddSingleton<FireStaffCreator>();
            services.AddSingleton<IceStaffCreator>();
            services.AddSingleton<AccessoryCreator>();
            services.AddSingleton<ArmorCreator>();
            services.AddSingleton<PotionCreator>();
            services.AddSingleton<AxeCreator>();
            services.AddSingleton<DaggerCreator>();
            services.AddSingleton<BuyMenu>();
            services.AddSingleton<ConfirmBuyMenu>();
            services.AddSingleton<RestManager>();
            services.AddSingleton<PickUpTreasureMenu>();
            services.AddSingleton<Persistence>(s =>
            {
                var writer = s.GetRequiredService<IPersistenceWriter>();
                return writer.Load(() =>
                {
                    var genesysModule = s.GetRequiredService<IGenesysModule>();
#if !RELEASE
                    genesysModule.Seed = 0; //Set to 0 for debugging, but by default is a random number
#endif
                    return genesysModule.SeedWorld(genesysModule.Seed);
                });
            });

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
                //If no local assets, load from dev location
                assetPath = Path.GetFullPath("../../../../../../AdventureAssets");
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
