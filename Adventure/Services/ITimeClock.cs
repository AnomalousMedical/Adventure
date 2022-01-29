﻿using Engine.Platform;

namespace Adventure
{
    interface ITimeClock
    {
        long CurrentTimeMicro { get; set; }
        float TimeFactor { get; }
        long DayEnd { get; set; }
        float DayFactor { get; }
        long DayStart { get; set; }
        bool IsDay { get; }
        float NightFactor { get; }

        void ResetTimeFactor();
        void SetTimeRatio(long speedup);
        void Update(Clock clock);
    }
}