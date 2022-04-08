using Adventure.Services;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class RestManager
    {
        private readonly Persistence persistence;
        private readonly ITimeClock timeClock;
        private readonly IZoneManager zoneManager;

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

        public void Rest(IExplorationGameState explorationGameState)
        {
            persistence.Current.BattleTriggers.ClearData();
            timeClock.SetTimeRatio(100);

            long? endTime = null;

            explorationGameState.SetExplorationEvent(c =>
            {
                if (endTime == null)
                {
                    endTime = c.CurrentTimeMicro + (long)(Clock.SecondsToMicro * 1.3f);
                }

                if (c.CurrentTimeMicro > endTime)
                {
                    zoneManager.ResetPlaceables();

                    foreach (var member in persistence.Current.Party.Members)
                    {
                        member.CharacterSheet.Rest();
                    }

                    timeClock.ResetTimeFactor();
                    return false;
                }

                return true;
            });
        }
    }
}
