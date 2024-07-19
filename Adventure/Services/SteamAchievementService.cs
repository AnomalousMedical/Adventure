using Microsoft.Extensions.Logging;
using Steamworks;
using System;

namespace Adventure.Services;

enum Achievements
{
    GetAirship,
    FinishGame,
    FinishGameOldSchool,
    FinishGameUndefeated,
    FullyUpgradedHelpBook,
    ElementalMastery
}

internal interface IAchievementService
{
    string AccountName { get; }

    void SetAchievement(Achievements achievement);
    void Update();
}

class SteamAchievementService : IDisposable, IAchievementService
{
    public static IAchievementService Instance { get; private set; } = new NullAchievementService();

    public static void Init()
    {
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
                Instance = new SteamAchievementService();
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
    }

    private SteamAchievementService()
    {
        
    }

    public void Dispose()
    {
        Console.WriteLine("Shut down Steam.");
        SteamAPI.Shutdown();
    }

    public void Update()
    {
        SteamAPI.RunCallbacks();
    }

    public void SetAchievement(Achievements achievement)
    {
        SteamUserStats.SetAchievement(achievement.ToString());
        SteamUserStats.StoreStats();
    }

    public String AccountName => SteamFriends.GetPersonaName();
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
