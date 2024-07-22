using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
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

    static class AchievementServiceExt
    {
        public static void AddAchievements(this IServiceCollection services)
        {
            services.TryAddSingleton<IAchievementService, NullAchievementService>();
        }
    }
}
