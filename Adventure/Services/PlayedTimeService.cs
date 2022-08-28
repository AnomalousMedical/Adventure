using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class PlayedTimeService
    {
        private readonly Persistence persistence;

        public PlayedTimeService(Persistence persistence)
        {
            this.persistence = persistence;
        }

        public void Update(Clock clock)
        {
            persistence.Current.Time.Total += clock.DeltaTimeMicro;
        }
    }
}
