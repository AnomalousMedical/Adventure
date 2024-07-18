using Microsoft.Extensions.Logging;
using Steamworks;
using System;

namespace Adventure.Services;

internal interface IAchievementService
{
    string AccountName { get; }

    void Update();
}

class SteamAchievementService : IDisposable, IAchievementService
{
    public static IAchievementService Create(ILogger<SteamAchievementService> logger)
    {
        if (!Packsize.Test())
        {
            logger.LogError("Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
        }

        if (!DllCheck.Test())
        {
            logger.LogError("DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
        }

        try
        {
            var m_bInitialized = SteamAPI.Init();
            if (!m_bInitialized)
            {
                logger.LogError("SteamAPI_Init() failed.");

                return new NullAchievementService();
            }

            return new SteamAchievementService(logger);
        }
        catch (DllNotFoundException e)
        {
            logger.LogError("Could not load [lib]steam_api.dll/so/dylib." + e);

            return new NullAchievementService();
        }
    }

    private readonly ILogger<SteamAchievementService> logger;
    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    private SteamAchievementService(ILogger<SteamAchievementService> logger)
    {
        this.logger = logger;
        logger.LogInformation("Connected to Steam.");

        logger.LogInformation(AccountName);
    }

    public void Dispose()
    {
        logger.LogInformation("Shutting down Steam.");
        SteamAPI.Shutdown();
    }

    public void Update()
    {
        SteamAPI.RunCallbacks();
    }

    public String AccountName => SteamFriends.GetPersonaName();
}

class NullAchievementService : IAchievementService
{
    public string AccountName => null;

    public void Update()
    {
        
    }
}
