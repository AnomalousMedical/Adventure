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
    //FinishGame,
    //FinishGameOldSchool,
    //FinishGameUndefeated,
    //FullyUpgradedHelpBook,
    //ElementalMastery
}

internal interface IAchievementService
{
    string AccountName { get; }

    void SetAchievement(Achievements achievement);
    void Update();
}

static class SteamAchievementServiceExt
{
    public static void AddSteamAchievements(this IServiceCollection services)
    {
        var addSteamService = false;

        if (!Packsize.Test())
        {
            Console.WriteLine("Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            return;
        }

        if (!DllCheck.Test())
        {
            Console.WriteLine("DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            return;
        }

        try
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
        catch (DllNotFoundException e)
        {
            Console.WriteLine("Could not load [lib]steam_api.dll/so/dylib." + e);
        }

        if (addSteamService)
        {
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
    private readonly Callback<UserStatsReceived_t> userStatsReceived;
    private readonly Callback<UserStatsStored_t> userStatsStored;
    private readonly Callback<UserAchievementStored_t> userAchievementStored;
    private readonly CGameID gameId;
    private readonly HashSet<Achievements> currentAchievements = new HashSet<Achievements>();
    private readonly ILogger<SteamAchievementService> logger;

    private bool loggedIn = false;
    private bool storeStats = false;
    private bool statsValid = false;
    private HashSet<Achievements> missedAchievements;

    public String AccountName => loggedIn ? SteamFriends.GetPersonaName() : null;

    public SteamAchievementService(ILogger<SteamAchievementService> logger)
    {
        this.logger = logger;
        this.gameId = new CGameID(SteamUtils.GetAppID());

        if (SteamUser.BLoggedOn())
        {
            loggedIn = true;
            userStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            userStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            userAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

            if (!SteamUserStats.RequestCurrentStats())
            {
                logger.LogWarning("Cannot request user stats.");
            }
        }
        else
        {
            logger.LogInformation("User is not logged in.");
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

    public void SetAchievement(Achievements achievement)
    {
        if (currentAchievements.Contains(achievement))
        {
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
                        SetAchievement(achievement);
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

    public void SetAchievement(Achievements achievement)
    {
        
    }

    public void Update()
    {
        
    }
}
