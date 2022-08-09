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
        Vector3 GetPortal(int portalIndex);
        void Update(Clock clock);
        void MovePlayer(in Vector3 loc);
        void SetPlayerVisible(bool visible);
        Vector3 GetAirshipPortal();
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

            MovePlayer(loc);
        }

        public void MovePlayer(in Vector3 loc)
        {
            player.SetLocation(loc);
            player.MakeIdle();
        }

        public void SetPlayerVisible(bool visible)
        {
            player.SetGraphicsActive(visible);
        }

        public void SetupWorldMap()
        {
            airship?.RequestDestruction();
            worldMapInstance?.RequestDestruction();

            worldMapInstance = objectResolver.Resolve<WorldMapInstance, WorldMapInstance.Description>(o =>
            {
                o.csIslandMaze = worldDatabase.WorldMap.Map;
                o.Areas = worldDatabase.AreaBuilders;
                o.AreaLocationSeed = worldDatabase.CurrentSeed;
            });

            airship = objectResolver.Resolve<Airship, Airship.Description>(o =>
            {
                o.Scale = new Vector3(0.4f, 0.4f, 0.4f);
                o.Map = worldMapInstance;
                o.WorldMapManager = this;
            });

            scopedCoroutine.RunTask(async () =>
            {
                await worldMapInstance.WaitForLoad();
                worldMapInstance.SetupPhysics();
            });
        }

        public Vector3 GetPortal(int portalIndex)
        {
            return worldMapInstance?.GetPortalLocation(portalIndex) ?? Vector3.Zero;
        }

        public Vector3 GetAirshipPortal()
        {
            return worldMapInstance?.GetAirshipPortalLocation() ?? Vector3.Zero;
        }

        public void Update(Clock clock)
        {
            airship.UpdateInput(clock);
        }
    }
}
