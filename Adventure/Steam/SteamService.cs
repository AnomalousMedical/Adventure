// This code is based on the example https://github.com/rlabrecque/Steamworks.NET-Example
// That code is Public Domain https://github.com/rlabrecque/Steamworks.NET-Example/blob/master/LICENSE.txt

using Adventure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steamworks;
using System;
using System.Collections.Generic;

namespace Adventure.Steam;

static class SteamServiceExt
{
    private static bool addSteamService = false;

    public static void AddSteam(this IServiceCollection services, Action<SteamAchievementService.Options> configure = null)
    {
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
            services.AddSingleton(options);
            services.AddSingleton<IAchievementService, SteamAchievementService>();
            services.AddSingleton<IOnscreenKeyboardService, SteamOnscreenKeyboardService>();
            services.AddSingleton<SteamPauseService>();
        }
    }

    public static void ActivateSteamServices(this IServiceProvider serviceProvider)
    {
        if (addSteamService)
        {
            serviceProvider.GetRequiredService<SteamPauseService>();
        }
    }
}
