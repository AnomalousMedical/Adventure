using Adventure.Services;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    interface IWorldMapManager
    {
        void SetupWorldMap();
    }

    class WorldMapManager : IDisposable, IWorldMapManager
    {
        private readonly IObjectResolver objectResolver;
        private readonly IWorldDatabase worldDatabase;
        private WorldMapInstance worldMapInstance;

        public WorldMapManager
        (
            IObjectResolverFactory objectResolverFactory,
            IWorldDatabase worldDatabase
        )
        {
            this.objectResolver = objectResolverFactory.Create();
            this.worldDatabase = worldDatabase;
        }

        public void Dispose()
        {
            objectResolver.Dispose();
        }

        public void SetupWorldMap()
        {
            worldMapInstance?.RequestDestruction();

            worldMapInstance = objectResolver.Resolve<WorldMapInstance, WorldMapInstance.Description>(o =>
            {
                o.csIslandMaze = worldDatabase.WorldMap.Map;
            });
        }
    }
}
