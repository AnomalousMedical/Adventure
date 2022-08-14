﻿using Adventure.Services;
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
        Task SetupWorldMap();
        Vector3 GetPortal(int portalIndex);
        void Update(Clock clock);
        void MovePlayer(in Vector3 loc);
        void SetPlayerVisible(bool visible);
        Vector3 GetAirshipPortal();
        Task WaitForWorldMapLoad();
    }

    class WorldMapManager : IDisposable, IWorldMapManager
    {
        private readonly IObjectResolver objectResolver;
        private readonly IWorldDatabase worldDatabase;
        private readonly Party party;
        private WorldMapInstance worldMapInstance;
        private WorldMapPlayer player;
        private Airship airship;

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

            MovePlayer(loc + new Vector3(0f, 0f, -0.30f));
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

        public async Task SetupWorldMap()
        {
            worldMapInstance?.RequestDestruction();

            if(airship == null)
            {
                airship = objectResolver.Resolve<Airship, Airship.Description>(o =>
                {
                    o.Scale = new Vector3(0.4f, 0.4f, 0.4f);
                });
            }

            worldMapInstance = objectResolver.Resolve<WorldMapInstance, WorldMapInstance.Description>(o =>
            {
                o.csIslandMaze = worldDatabase.WorldMap.Map;
                o.Areas = worldDatabase.AreaBuilders;
                o.PortalLocations = worldDatabase.PortalLocations;
                o.AirshipSquare = worldDatabase.AirshipStartSquare;
                o.AirshipPortalSquare = worldDatabase.AirshipPortalSquare;
            });

            await worldMapInstance.WaitForLoad();
            await airship.SetMap(worldMapInstance);
            worldMapInstance.SetupPhysics();
        }

        public Task WaitForWorldMapLoad()
        {
            return worldMapInstance?.WaitForLoad() ?? Task.CompletedTask;
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
