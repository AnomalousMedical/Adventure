using Adventure.Services;
using Engine.Platform;

namespace Adventure
{
    class RestManager
    {
        private readonly Persistence persistence;
        private readonly ITimeClock timeClock;
        private readonly IZoneManager zoneManager;
        private long? endTime = null;
        private bool active = false;

        public RestManager
        (
            Persistence persistence,
            ITimeClock timeClock,
            IZoneManager zoneManager
        )
        {
            this.persistence = persistence;
            this.timeClock = timeClock;
            this.zoneManager = zoneManager;
        }

        public void Rest()
        {
            persistence.Current.BattleTriggers.ClearData();
            timeClock.SetTimeRatio(100);
            active = true;
            endTime = null;
        }

        public void Update(Clock clock)
        {
            if (!active)
            {
                return;
            }

            if (endTime == null)
            {
                endTime = clock.CurrentTimeMicro + (long)(Clock.SecondsToMicro * 1.3f);
            }

            if (clock.CurrentTimeMicro > endTime)
            {
                zoneManager.ResetPlaceables();

                foreach (var member in persistence.Current.Party.Members)
                {
                    member.CharacterSheet.Rest();
                }

                timeClock.ResetTimeFactor();
                active = false;
            }
        }
    }
}
