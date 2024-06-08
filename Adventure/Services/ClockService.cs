using Engine.Platform;

namespace Adventure.Services
{
    interface IClockService
    {
        Clock Clock { get; }

        void Update(Clock clock);
    }

    class ClockService : IClockService
    {
        private Clock clock = new Clock();

        public Clock Clock => clock;

        public void Update(Clock clock)
        {
            this.clock = clock;
        }
    }
}
