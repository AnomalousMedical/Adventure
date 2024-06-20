using Adventure.Services;
using BepuPlugin;
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
        void Update(Clock clock);
        void MovePlayer(in Vector3 loc);
        void SetPlayerVisible(bool visible);
        void CenterCamera();
        Vector3 GetCellCenterpoint(in IntVector2 cell);
        void MakePlayerIdle();
    }

    class WorldMapManager : IDisposable, IWorldMapManager
    {
        private readonly IObjectResolver objectResolver;
        private readonly IWorldDatabase worldDatabase;
        private readonly Party party;
        private readonly PartyMemberManager partyMemberManager;
        private readonly IBepuScene<WorldMapScene> bepuScene;
        private WorldMapInstance worldMapInstance;
        private WorldMapPlayer player;
        private Airship airship;

        public WorldMapManager
        (
            IObjectResolverFactory objectResolverFactory,
            IWorldDatabase worldDatabase,
            IBepuScene<WorldMapScene> bepuScene, //Inject this here so it is created earlier and destroyed later
            Party party,
            PartyMemberManager partyMemberManager
        )
        {
            this.objectResolver = objectResolverFactory.Create();
            this.worldDatabase = worldDatabase;
            this.party = party;
            this.partyMemberManager = partyMemberManager;
            this.bepuScene = bepuScene;
            this.partyMemberManager.PartyChanged += PartyMemberManager_PartyChanged;
        }

        public void Dispose()
        {
            objectResolver.Dispose();
            this.partyMemberManager.PartyChanged -= PartyMemberManager_PartyChanged;
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
            MakePlayerIdle();
        }

        public void MakePlayerIdle()
        {
            player.MakeIdle();
        }

        public void SetPlayerVisible(bool visible)
        {
            player.SetGraphicsActive(visible);
        }

        public void CenterCamera()
        {
            player.CenterCamera();
            airship.CenterCamera();
        }

        public async Task SetupWorldMap()
        {
            player?.RequestDestruction();
            player = null;
            worldMapInstance?.RequestDestruction();
            airship?.RequestDestruction();

            CreatePlayersAndFollowers();

            airship = objectResolver.Resolve<Airship, Airship.Description>(o =>
            {
                o.Scale = new Vector3(0.4f, 0.4f, 0.4f);
            });

            worldMapInstance = objectResolver.Resolve<WorldMapInstance, WorldMapInstance.Description>(o =>
            {
                o.csIslandMaze = worldDatabase.WorldMap.Map;
                o.Areas = worldDatabase.AreaBuilders;
                o.AirshipSquare = worldDatabase.AirshipStartSquare;
                o.BiomePropLocations = worldDatabase.BiomePropLocations;
            });

            await worldMapInstance.WaitForLoad();
            await airship.SetMap(worldMapInstance);
            worldMapInstance.SetupPhysics();
        }

        private void CreatePlayersAndFollowers()
        {
            if (party.ActiveCharacters.Any())
            {
                if (player == null)
                {
                    var playerCharacter = party.ActiveCharacters.FirstOrDefault();
                    player = objectResolver.Resolve<WorldMapPlayer, WorldMapPlayer.Description>(o =>
                    {
                        o.PlayerSprite = playerCharacter.PlayerSprite;
                        o.CharacterSheet = playerCharacter.CharacterSheet;
                        o.Gamepad = GamepadId.Pad1;
                    });
                }

                player?.CreateFollowers(party.ActiveCharacters.Skip(1));
            }
        }

        public Vector3 GetCellCenterpoint(in IntVector2 cell)
        {
            return worldMapInstance.GetCellCenterpoint(cell);
        }

        public void Update(Clock clock)
        {
            airship.UpdateInput(clock);
        }

        private void PartyMemberManager_PartyChanged()
        {
            CreatePlayersAndFollowers();
        }
    }
}
