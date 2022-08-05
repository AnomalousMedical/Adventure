using Adventure.Services;
using Engine;
using Engine.Platform;
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
        private readonly Party party;
        private WorldMapInstance worldMapInstance;
        private WorldMapPlayer player;

        public WorldMapManager
        (
            IObjectResolverFactory objectResolverFactory,
            IWorldDatabase worldDatabase,
            Party party
        )
        {
            this.objectResolver = objectResolverFactory.Create();
            this.worldDatabase = worldDatabase;
            this.party = party;

            //var playerCharacter = party.ActiveCharacters.FirstOrDefault();
            //player = objectResolver.Resolve<WorldMapPlayer, WorldMapPlayer.Description>(o =>
            //{
            //    //o.Translation = currentZone.StartPoint;
            //    o.PlayerSprite = playerCharacter.PlayerSprite;
            //    o.CharacterSheet = playerCharacter.CharacterSheet;
            //    o.Gamepad = GamepadId.Pad1;
            //});
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
