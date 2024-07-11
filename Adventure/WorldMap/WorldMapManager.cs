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
        bool AllowBackgroundMusicChange { get; set; }

        void MovePlayerToArea(int area);
        Task SetupWorldMap();
        void Update(Clock clock);
        void MovePlayer(in Vector3 loc);
        void SetPlayerVisible(bool visible);
        void CenterCamera();
        Vector3 GetCellCenterpoint(in IntVector2 cell);
        void MakePlayerIdle();
        void CreatePlayersAndFollowers();
    }

    class WorldMapManager : IDisposable, IWorldMapManager
    {
        private readonly IObjectResolver objectResolver;
        private readonly IWorldDatabase worldDatabase;
        private readonly Party party;
        private readonly PartyMemberManager partyMemberManager;
        private readonly Persistence persistence;
        private readonly MultiCameraMover<WorldMapScene> multiCameraMover;
        private readonly PlayerCage<WorldMapScene> playerCage;
        private readonly IBepuScene<WorldMapScene> bepuScene;
        private WorldMapInstance worldMapInstance;
        private WorldMapPlayer[] players = new WorldMapPlayer[4];
        private Airship airship;

        public WorldMapManager
        (
            IObjectResolverFactory objectResolverFactory,
            IWorldDatabase worldDatabase,
            IBepuScene<WorldMapScene> bepuScene, //Inject this here so it is created earlier and destroyed later
            Party party,
            PartyMemberManager partyMemberManager,
            Persistence persistence,
            MultiCameraMover<WorldMapScene> multiCameraMover,
            PlayerCage<WorldMapScene> playerCage
        )
        {
            this.objectResolver = objectResolverFactory.Create();
            this.worldDatabase = worldDatabase;
            this.party = party;
            this.partyMemberManager = partyMemberManager;
            this.persistence = persistence;
            this.multiCameraMover = multiCameraMover;
            this.playerCage = playerCage;
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

            if (worldMapInstance != null)
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
            foreach(var player in players)
            {
                if(player != null)
                {
                    player.SetLocation(loc);
                    player.MakeIdle();
                }
            }
        }

        public void MakePlayerIdle()
        {
            foreach (var player in players)
            {
                player?.MakeIdle();
            }
        }

        public void SetPlayerVisible(bool visible)
        {
            foreach (var player in players)
            {
                player?.SetGraphicsActive(visible);
            }
        }

        public void CenterCamera()
        {
            if (persistence.Current.Player.InAirship)
            {
                airship.CenterCamera();
            }
            else
            {
                multiCameraMover.CenterCamera();
                playerCage.Update();
            }
        }

        public async Task SetupWorldMap()
        {
            for(int i = 0; i < players.Length; ++i)
            {
                players[i]?.RequestDestruction();
                players[i] = null;
            }
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

        public void CreatePlayersAndFollowers()
        {
            for (int i = 0; i < players.Length; i++)
            {
                var currentPlayerCharacters = party.ActiveCharacters.Where(c => c.Player == i);
                var playerCharacter = currentPlayerCharacters.FirstOrDefault();
                if (playerCharacter != null)
                {
                    //Is the player now a new character? If so delete the current instance
                    if (players[i] != null && players[i].CharacterSheet != playerCharacter.CharacterSheet)
                    {
                        players[i].RequestDestruction();
                        players[i] = null;
                    }

                    if (players[i] == null)
                    {
                        players[i] = objectResolver.Resolve<WorldMapPlayer, WorldMapPlayer.Description>(o =>
                        {
                            o.PlayerSprite = playerCharacter.PlayerSprite;
                            o.CharacterSheet = playerCharacter.CharacterSheet;
                            o.Gamepad = (GamepadId)i;
                        });
                    }

                    players[i].CreateFollowers(currentPlayerCharacters.Skip(1));
                }
                else
                {
                    //Despawning a player so clear its location
                    persistence.Current.Player.WorldPosition[i] = null;
                    if (players[i] != null)
                    {
                        players[i].RequestDestruction();
                        players[i] = null;
                    }
                }
            }

            if(persistence.Current.Player.InWorld && !persistence.Current.Player.InAirship)
            {
                multiCameraMover.CenterCamera();
                playerCage.Update();
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

        public bool AllowBackgroundMusicChange
        {
            get => airship.AllowMusicChange;
            set => airship.AllowMusicChange = value;
        }
    }
}
