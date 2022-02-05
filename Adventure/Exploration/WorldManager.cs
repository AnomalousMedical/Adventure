using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration
{
    interface IWorldManager
    {
        int GetZoneSeed(int index);
    }

    class WorldManager : IWorldManager
    {
        private List<int> createdZoneSeeds = new List<int>();
        private Random zoneRandom;

        public WorldManager
        (
            Persistence persistence
        )
        {
            zoneRandom = new Random(persistence.World.Seed);
        }

        public int GetZoneSeed(int index)
        {
            var end = index + 1;
            for (var i = createdZoneSeeds.Count; i < end; ++i)
            {
                createdZoneSeeds.Add(zoneRandom.Next(int.MinValue, int.MaxValue));
            }
            return createdZoneSeeds[index];
        }
    }
}
