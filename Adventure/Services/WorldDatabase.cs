using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface IWorldDatabase
    {
        int GetZoneSeed(int index);
    }

    class WorldDatabase : IWorldDatabase
    {
        private List<int> createdZoneSeeds;
        private int currentSeed;
        private Random zoneRandom;
        private readonly Persistence persistence;

        public WorldDatabase(Persistence persistence)
        {
            Reset(persistence.Current.World.Seed);
            this.persistence = persistence;
        }

        public int GetZoneSeed(int zoneIndex)
        {
            CheckSeed();

            var end = zoneIndex + 1;
            for (var i = createdZoneSeeds.Count; i < end; ++i)
            {
                createdZoneSeeds.Add(zoneRandom.Next(int.MinValue, int.MaxValue));
            }
            return createdZoneSeeds[zoneIndex];
        }

        private void CheckSeed()
        {
            if (persistence.Current.World.Seed != currentSeed)
            {
                Reset(persistence.Current.World.Seed);
            }
        }

        private void Reset(int newSeed)
        {
            createdZoneSeeds = new List<int>();
            zoneRandom = new Random(newSeed);
            currentSeed = newSeed;
        }
    }
}
