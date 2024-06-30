using Engine.Platform;
using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface ITimeClock
    {
        long CurrentTimeMicro { get; set; }
        float TimePercent { get; }
        long DayEnd { get; set; }
        float DayFactor { get; }
        long DayStart { get; set; }
        bool IsDay { get; }
        float NightFactor { get; }

        void ResetTimeFactor();
        void SetTimeMultiplier(long speedup);
        void Update(Clock clock);
        void ResetToPersistedTime();
        long GetRealWallTimeUntil(long timeOfDay);

        public event Action<TimeClock> NightStarted;

        public event Action<TimeClock> DayStarted;
    }

    class TimeClock : ITimeClock
    {
        public event Action<TimeClock> NightStarted;

        public event Action<TimeClock> DayStarted;

        const long HoursToMicro = 60L * 60L * Clock.SecondsToMicro;
        const long HoursPerDay = 24L;
        private readonly Persistence persistence;
        long currentTime;
        //long timeFactor = 25000L; //Pretty Fast
        //long timeFactor = 10000L;
        const long RegularTimeFactor = 100L;
        long timeVelocity = RegularTimeFactor;
        long period = HoursPerDay * HoursToMicro;
        long halfPeriod;
        long dayStart = 6L * HoursToMicro;
        long dayEnd = 18L * HoursToMicro;
        float dayEndFactor;
        float nightEndFactor;

        public TimeClock(Persistence persistence)
        {
            halfPeriod = period / 2;
            dayEndFactor = (dayEnd - dayStart) * Clock.MicroToSeconds;
            nightEndFactor = (dayStart + period - dayEnd) * Clock.MicroToSeconds;
            this.persistence = persistence;
        }

        public void ResetToPersistedTime()
        {
            if(persistence.Current != null)
            {
                CurrentTimeMicro = persistence.Current.Time.Current ?? 10L * HoursToMicro;
            }
            else
            {
                CurrentTimeMicro = 10L * HoursToMicro;
            }
        }

        public void Update(Clock clock)
        {
            bool wasDay = IsDay;

            currentTime += clock.DeltaTimeMicro * timeVelocity;
            currentTime %= period;
            persistence.Current.Time.Current = currentTime;

            bool nowDay = IsDay;
            HandleDayNightEvents(wasDay, nowDay);
        }

        public void ResetTimeFactor()
        {
            timeVelocity = 100L;
        }

        public void SetTimeMultiplier(long speedup)
        {
            timeVelocity = RegularTimeFactor * speedup;
        }

        public long GetRealWallTimeUntil(long timeOfDay)
        {
            timeOfDay %= period;
            if(currentTime > timeOfDay)
            {
                timeOfDay += period;
            }
            var delta = timeOfDay - currentTime;
            return delta / timeVelocity;
        }

        public bool IsDay => currentTime > dayStart && currentTime <= dayEnd;

        public long CurrentTimeMicro
        {
            get
            {
                return currentTime;
            }
            set
            {
                bool wasDay = IsDay;
                currentTime = value % period;
                bool nowDay = IsDay;
                HandleDayNightEvents(wasDay, nowDay);
            }
        }

        public float TimePercent => currentTime / (float)period;

        public long DayStart
        {
            get
            {
                return dayStart;
            }
            set
            {
                dayStart = value % period;
            }
        }

        public long DayEnd
        {
            get
            {
                return dayEnd;
            }
            set
            {
                dayEnd = value % period;
            }
        }

        public float DayFactor
        {
            get
            {
                if (!IsDay)
                {
                    return 0.0f;
                }
                return (currentTime - dayStart) * Clock.MicroToSeconds / dayEndFactor;
            }
        }

        public float NightFactor
        {
            get
            {
                if (IsDay)
                {
                    return 0.0f;
                }
                if (currentTime > dayEnd)
                {
                    return (currentTime - dayEnd) * Clock.MicroToSeconds / nightEndFactor;
                }
                //All thats left is (currentTime < dayStart)
                return (currentTime) * Clock.MicroToSeconds / nightEndFactor + 0.5f;
            }
        }

        private void HandleDayNightEvents(bool wasDay, bool nowDay)
        {
            if (wasDay != nowDay)
            {
                if (wasDay && !nowDay)
                {
                    NightStarted?.Invoke(this);
                }
                else //This is the only remaining case, so don't need any logic
                {
                    DayStarted?.Invoke(this);
                }
            }
        }
    }
}
