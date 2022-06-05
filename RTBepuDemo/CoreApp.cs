﻿using Anomalous.OSPlatform;
using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;

namespace RTBepuDemo
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
                o.Title = "RT BEPU Physics 2";
            });

            services.AddLogging(o =>
            {
                o.AddConsole();
            });

            services.AddDiligentEngine(pluginManager, o =>
            {
                o.Features = GraphicsEngine.FeatureFlags.RayTracing;
            })
            .AddDiligentEngineRt(o =>
            {

            });
            services.AddSingleton<RTInstances>();

            services.AddOSPlatform(pluginManager);
            services.AddSharpGui();

            services.AddFirstPersonFlyCamera();
            services.AddBepuPlugin();

            //Add this app's services
            services.TryAddSingleton<BepuUpdateListener>();

            services.AddScoped<SceneCube>();
            services.AddScoped<SceneCube.Desc>();
            services.AddScoped<BodyPositionSync>();
            services.AddScoped<BodyPositionSync.Desc>();
            services.AddSingleton<RTGui>();

            return true;
        }

        public override bool OnLink(IServiceProvider serviceProvider)
        {
            var log = serviceProvider.GetRequiredService<ILogger<CoreApp>>();
            log.LogInformation("Running from directory {0}", FolderFinder.ExecutableFolder);

            //Setup virtual file system
            var vfs = serviceProvider.GetRequiredService<VirtualFileSystem>();
            //This needs to be less hardcoded.
            var assetPath = Path.GetFullPath(Path.Combine(FolderFinder.ExecutableFolder, "Engine-Next-Assets"));
            if (!Directory.Exists(assetPath))
            {
                //If no local assets, load from dev location
                assetPath = Path.GetFullPath("../../../../../Engine-Next-Assets");
            }
            vfs.addArchive(assetPath);

            mainTimer = serviceProvider.GetRequiredService<UpdateTimer>();

            var updateListener = serviceProvider.GetRequiredService<BepuUpdateListener>();
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
