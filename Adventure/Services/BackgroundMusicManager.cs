using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class BackgroundMusicManager : IDisposable
    {
        private readonly IZoneManager zoneManager;
        private readonly IBackgroundMusicPlayer backgroundMusicManager;
        private readonly ITimeClock timeClock;

        public BackgroundMusicManager
        (
            IZoneManager zoneManager, 
            IBackgroundMusicPlayer backgroundMusicManager,
            ITimeClock timeClock
        )
        {
            this.zoneManager = zoneManager;
            this.backgroundMusicManager = backgroundMusicManager;
            this.timeClock = timeClock;

            zoneManager.ZoneChanged += ZoneManager_ZoneChanged;
            timeClock.DayStarted += TimeClock_DayStarted;
            timeClock.NightStarted += TimeClock_NightStarted;
        }

        public void Dispose()
        {
            zoneManager.ZoneChanged -= ZoneManager_ZoneChanged;
            timeClock.DayStarted -= TimeClock_DayStarted;
            timeClock.NightStarted -= TimeClock_NightStarted;
        }

        private void TimeClock_NightStarted(TimeClock obj)
        {
            var song = zoneManager.Current.Biome.BgMusicNight;
            backgroundMusicManager.SetBackgroundSong(song);
        }

        private void TimeClock_DayStarted(TimeClock obj)
        {
            var song = zoneManager.Current.Biome.BgMusic;
            backgroundMusicManager.SetBackgroundSong(song);
        }

        private void ZoneManager_ZoneChanged(IZoneManager obj)
        {
            var song = zoneManager.Current.Biome.BgMusic;
            if (!timeClock.IsDay)
            {
                song = zoneManager.Current.Biome.BgMusicNight;
            }
            backgroundMusicManager.SetBackgroundSong(song);
        }
    }
}
