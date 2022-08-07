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
        bool PhysicsActive { get; }

        void MovePlayerToArea(int area);
        void SetupWorldMap();
        void GoToNextPortal(int portalIndex);
        void Update(Clock clock);
    }

    class WorldMapManager : IDisposable, IWorldMapManager
    {
        private readonly IObjectResolver objectResolver;
        private readonly IWorldDatabase worldDatabase;
        private readonly Party party;
        private readonly IScopedCoroutine scopedCoroutine;
        private WorldMapInstance worldMapInstance;
        private WorldMapPlayer player;
        private Airship airship;

        public WorldMapManager
        (
            IObjectResolverFactory objectResolverFactory,
            IWorldDatabase worldDatabase,
            Party party,
            IScopedCoroutine scopedCoroutine
        )
        {
            this.objectResolver = objectResolverFactory.Create();
            this.worldDatabase = worldDatabase;
            this.party = party;
            this.scopedCoroutine = scopedCoroutine;
            var playerCharacter = party.ActiveCharacters.FirstOrDefault();
            player = objectResolver.Resolve<WorldMapPlayer, WorldMapPlayer.Description>(o =>
            {
                //o.Translation = currentZone.StartPoint;
                o.PlayerSprite = playerCharacter.PlayerSprite;
                o.CharacterSheet = playerCharacter.CharacterSheet;
                o.Gamepad = GamepadId.Pad1;
            });

            airship = objectResolver.Resolve<Airship, Airship.Description>(o =>
            {
                o.Scale = new Vector3(0.4f, 0.4f, 0.4f);
            });
            airship.CreatePhysics();
        }

        public void Dispose()
        {
            objectResolver.Dispose();
        }

        public bool PhysicsActive => worldMapInstance?.PhysicsActive == true && !airship.Active;

        public void MovePlayerToArea(int area)
        {
            Vector3 loc;

            if(worldMapInstance != null)
            {
                loc = worldMapInstance.GetAreaLocation(area);
            }
            else
            {
                loc = new Vector3(10, 10, 10);
            }

            player.SetLocation(loc);
            player.StopMovement();
        }

        public void SetupWorldMap()
        {
            worldMapInstance?.RequestDestruction();

            worldMapInstance = objectResolver.Resolve<WorldMapInstance, WorldMapInstance.Description>(o =>
            {
                o.csIslandMaze = worldDatabase.WorldMap.Map;
                o.Areas = worldDatabase.AreaBuilders;
                o.AreaLocationSeed = worldDatabase.CurrentSeed;
            });

            scopedCoroutine.RunTask(async () =>
            {
                await worldMapInstance.WaitForLoad();
                worldMapInstance.SetupPhysics();
            });
        }

        public void GoToNextPortal(int portalIndex)
        {
            var loc = worldMapInstance?.GetPortalLocation(portalIndex + 1) ?? Vector3.Zero;

            player.SetLocation(loc);
            player.StopMovement();
        }

        public void Update(Clock clock)
        {
            airship.UpdateInput(clock);
        }
    }
}
