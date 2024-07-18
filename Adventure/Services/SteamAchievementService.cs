using Anomalous.OSPlatform;
using Microsoft.Extensions.Logging;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Adventure.Services
{
    internal interface IAchievementService
    {
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

            logger.LogInformation(SteamFriends.GetPersonaName());
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
    }

    class NullAchievementService : IAchievementService
    {
        public void Update()
        {
            
        }
    }
}
