// This code is based on the example https://github.com/rlabrecque/Steamworks.NET-Example
// That code is Public Domain https://github.com/rlabrecque/Steamworks.NET-Example/blob/master/LICENSE.txt

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Adventure.Services;

enum Achievements
{
    GetAirship,
    FinishGame,
    FinishGameOldSchool,
    FinishGameUndefeated,
    FullyUpgradedHelpBook,
    ElementalMastery,
    SeeTheWorld,
}

internal interface IAchievementService
{
    string AccountName { get; }

    string AppId { get; }

    void UnlockAchievement(Achievements achievement);
    void Update();
}

static class SteamAchievementServiceExt
{
    public static void AddSteamAchievements(this IServiceCollection services, Action<SteamAchievementService.Options> configure = null)
    {
        var addSteamService = false;

        try
        {
            if (Packsize.Test())
            {
                if (DllCheck.Test())
                {
                    if (SteamAPI.Init())
                    {
                        Console.WriteLine("Connected to Steam.");
                        addSteamService = true;
                    }
                    else
                    {
                        Console.WriteLine("SteamAPI_Init() failed.");
                    }
                }
                else
                {
                    Console.WriteLine("DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
                }
            }
            else
            {
                Console.WriteLine("Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("Could not load [lib]steam_api.dll/so/dylib. Steam services will be unavailable.");
        }

        if (addSteamService)
        {
            var options = new SteamAchievementService.Options();
            configure?.Invoke(options);
            services.AddSingleton<SteamAchievementService.Options>(options);
            services.AddSingleton<IAchievementService, SteamAchievementService>();
        }
        else
        {
            services.AddSingleton<IAchievementService, NullAchievementService>();
        }
    }
}

class SteamAchievementService : IDisposable, IAchievementService
{
    public class Options
    {
        public uint? AppId { get; set; }

        public uint? PlaytestAppId { get; set; }
    }

    private readonly Callback<UserStatsReceived_t> userStatsReceived;
    private readonly Callback<UserStatsStored_t> userStatsStored;
    private readonly Callback<UserAchievementStored_t> userAchievementStored;
    private readonly CGameID gameId;
    private readonly HashSet<Achievements> currentAchievements = new HashSet<Achievements>();
    private readonly HashSet<Achievements> loadedAchievements = new HashSet<Achievements>();
    private readonly ILogger<SteamAchievementService> logger;

    private bool storeStats = false;
    private bool statsValid = false;
    private HashSet<Achievements> missedAchievements;

    public String AccountName => SteamFriends.GetPersonaName();

    public string AppId => gameId.ToString();

    public SteamAchievementService(ILogger<SteamAchievementService> logger, Options options)
    {
        this.logger = logger;
        var appId = SteamUtils.GetAppID();
        this.gameId = new CGameID(appId);

        bool subscribed;
        if (appId.m_AppId == options.AppId)
        {
            subscribed = SteamApps.BIsSubscribedApp(new AppId_t(options.AppId.Value));
        }
        else if (appId.m_AppId == options.PlaytestAppId)
        {
            subscribed = SteamApps.BIsSubscribedApp(new AppId_t(options.PlaytestAppId.Value));
        }
        else
        {
            subscribed = false;
        }

        if (subscribed)
        {
            logger.LogInformation("Running in full mode.");

            userStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            userStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            userAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

            logger.LogInformation("Loading achievements.");
            if (!SteamUserStats.RequestCurrentStats())
            {
                logger.LogWarning("Cannot request user stats.");
            }
        }
        else
        {
            logger.LogInformation("Running in demo mode.");
        }
    }

    public void Dispose()
    {
        logger.LogInformation("Shut down Steam.");
        SteamAPI.Shutdown();
    }

    public void Update()
    {
        SteamAPI.RunCallbacks();
        if (storeStats)
        {
            storeStats = false;
            SteamUserStats.StoreStats();
        }
    }

    public void UnlockAchievement(Achievements achievement)
    {
        if (currentAchievements.Contains(achievement))
        {
            return;
        }

        if (!loadedAchievements.Contains(achievement))
        {
            logger.LogInformation("Achievement '{achievement}' is not loaded from Steam.", achievement);
            return;
        }

        if (statsValid)
        {
            currentAchievements.Add(achievement);
            SteamUserStats.SetAchievement(achievement.ToString());
            storeStats = true;
        }
        else
        {
            missedAchievements = missedAchievements ?? new HashSet<Achievements>();
            missedAchievements.Add(achievement);
        }
    }

    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)gameId == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                logger.LogInformation("Received stats and achievements from Steam");

                statsValid = true;

                // load achievements
                foreach (var ach in Enum.GetValues(typeof(Achievements)) as Achievements[])
                {
                    bool ret = SteamUserStats.GetAchievement(ach.ToString(), out var achieved);
                    if (ret)
                    {
                        logger.LogInformation("Loaded achievement '{ach}' achieved: {status}", ach, achieved);
                        loadedAchievements.Add(ach);
                        if (achieved)
                        {
                            currentAchievements.Add(ach);
                        }
                    }
                    else
                    {
                        logger.LogWarning("SteamUserStats.GetAchievement failed for Achievement '{ach}'. Is it registered in the Steam Partner site?", ach);
                    }
                }

                //Update any missed achievements
                if (missedAchievements != null)
                {
                    logger.LogInformation("Adding {missing} achievements that were achieved while offline.", missedAchievements.Count);
                    var localMissed = missedAchievements;
                    missedAchievements = null;
                    foreach (var achievement in localMissed)
                    {
                        UnlockAchievement(achievement);
                    }
                }
            }
            else
            {
                logger.LogError("RequestStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    private void OnUserStatsStored(UserStatsStored_t pCallback)
    {
        // we may get callbacks for other games' stats arriving, ignore them
        if ((ulong)gameId == pCallback.m_nGameID)
        {
            if (EResult.k_EResultOK == pCallback.m_eResult)
            {
                logger.LogInformation("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                logger.LogWarning("StoreStats - some failed to validate");

                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t callback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)gameId;
                OnUserStatsReceived(callback);
            }
            else
            {
                logger.LogError("StoreStats - failed, " + pCallback.m_eResult);
            }
        }
    }

    private void OnAchievementStored(UserAchievementStored_t pCallback)
    {
        // We may get callbacks for other games' stats arriving, ignore them
        if ((ulong)gameId == pCallback.m_nGameID)
        {
            if (0 == pCallback.m_nMaxProgress)
            {
                logger.LogInformation("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
            }
            else
            {
                logger.LogInformation("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
            }
        }
    }
}

class NullAchievementService : IAchievementService
{
    public string AccountName => null;

    public string AppId => null;

    public void UnlockAchievement(Achievements achievement)
    {
        
    }

    public void Update()
    {
        
    }
}
