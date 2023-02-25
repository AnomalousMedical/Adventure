using Adventure.Assets;
using Adventure.Items;
using Adventure.Items.Creators;
using Adventure.Services;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface IWorldManager
    {
        void SetupZone(int zoneIndex, Zone.Description o);
    }

    class WorldManager : IWorldManager
    {
        private readonly Persistence persistence;
        private readonly IWorldDatabase worldDatabase;

        public WorldManager
        (
            Persistence persistence,
            IWorldDatabase worldDatabase
        )
        {
            this.persistence = persistence;
            this.worldDatabase = worldDatabase;
        }

        public void SetupZone(int zoneIndex, Zone.Description o)
        {
            var initRandom = new FIRandom(worldDatabase.GetZoneSeed(zoneIndex));
            var areaBuilder = worldDatabase.GetAreaBuilder(zoneIndex);
            if(persistence.Current.World.CompletedAreaLevels.TryGetValue(areaBuilder.Index, out var level))
            {
                o.EnemyLevel = level;
            }
            else
            {
                o.EnemyLevel = areaBuilder.EnemyLevel ?? persistence.Current.World.Level;
            }
            areaBuilder.SetupZone(zoneIndex, o, initRandom);
        }
    }
}
