using Adventure.Services;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration
{
    interface IWorldManager
    {
        void SetupZone(int zoneIndex, Zone.Description o);
    }

    class WorldManager : IWorldManager
    {
        private readonly IBiomeManager biomeManager;
        private List<int> createdZoneSeeds = new List<int>();
        private Random zoneRandom;

        public WorldManager
        (
            Persistence persistence,
            IBiomeManager biomeManager
        )
        {
            zoneRandom = new Random(persistence.World.Seed);
            this.biomeManager = biomeManager;
        }

        public void SetupZone(int zoneIndex, Zone.Description o)
        {
            o.Index = zoneIndex;
            o.RandomSeed = GetZoneSeed(zoneIndex);
            o.Width = 50;
            o.Height = 50;
            o.CorridorSpace = 10;
            o.RoomDistance = 3;
            o.RoomMin = new IntSize2(2, 2);
            o.RoomMax = new IntSize2(6, 6); //Between 3-6 is good here, 3 for more cityish with small rooms, 6 for more open with more big rooms, sometimes connected
            o.CorridorMaxLength = 4;
            o.GoPrevious = zoneIndex != 0;
            o.EnemyLevel = 20;
            o.Biome = biomeManager.GetBiome(Math.Abs(o.RandomSeed) % biomeManager.Count);
        }

        private int GetZoneSeed(int index)
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
