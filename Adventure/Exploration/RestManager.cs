using Adventure.Services;
using Engine.Platform;

namespace Adventure
{
    class RestManager
    {
        public enum RestTarget
        {
            Dawn,
            Noon,
            Dusk,
            Midnight
        }

        private readonly Persistence persistence;
        private readonly ITimeClock timeClock;
        private readonly IZoneManager zoneManager;
        private long? endTime = null;
        private bool active = false;
        private RestTarget restTarget;

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

        public bool Active => active;

        public void Rest(RestTarget restTarget)
        {
            this.restTarget = restTarget;
            persistence.Current.BattleTriggers.ClearData();
            timeClock.SetTimeMultiplier(300);
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
                long sleepEndGameTime;
                switch (restTarget)
                {
                    default:
                    case RestTarget.Dawn:
                        sleepEndGameTime = timeClock.DayStart;
                        break;
                    case RestTarget.Noon:
                        sleepEndGameTime = 12L * 60 * 60 * Clock.SecondsToMicro;
                        break;
                    case RestTarget.Dusk:
                        sleepEndGameTime = timeClock.DayEnd;
                        break;
                    case RestTarget.Midnight:
                        sleepEndGameTime = 0L;
                        break;
                }
                endTime = clock.CurrentTimeMicro + timeClock.GetRealWallTimeUntil(sleepEndGameTime);
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
