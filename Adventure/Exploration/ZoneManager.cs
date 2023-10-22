using BepuPlugin;
using Engine;
using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Platform;

namespace Adventure
{
    interface IZoneManager
    {

        event Action<IZoneManager> ZoneChanged;

        bool ChangingZone { get; }
        Zone Current { get; }
        Task GoNext();
        Task GoNext(Vector3 triggerLoc);
        Task GoPrevious();
        Task GoPrevious(Vector3 triggerLoc);
        Task Restart(bool allowHoldZone = true);
        Task WaitForCurrent();
        Task WaitForNext();
        Task WaitForPrevious();
        void StopPlayer();
        void GoStartPoint();
        void GoEndPoint();
        void ResetPlaceables();
        Vector3 GetPlayerLoc();
        void ManagePlayers();
        void Update();
        void CenterCamera();
        void DestroyPlayers();
    }

    class ZoneManager : IDisposable, IZoneManager
    {
        private bool changingZone = false;
        private Zone currentZone;
        private Zone nextZone;
        private Zone previousZone;

        private Player[] players = new Player[4];

        private IObjectResolver objectResolver;
        private readonly Party party;
        private readonly IWorldManager worldManager;
        private readonly Persistence persistence;
        private readonly Sky sky;

        public event Action<IZoneManager> ZoneChanged;

        public bool ChangingZone => changingZone;

        public Zone Current => currentZone;

        public ZoneManager(
            Party party,
            IWorldManager worldManager,
            IObjectResolverFactory objectResolverFactory,
            Persistence persistence,
            IBepuScene<ZoneScene> bepuScene, //Inject this so it is created earlier and destroyed later
            Sky sky
        )
        {
            objectResolver = objectResolverFactory.Create();

            this.party = party;
            this.worldManager = worldManager;
            this.persistence = persistence;
            this.sky = sky;
        }

        public async Task Restart(bool allowHoldZone = true)
        {
            if (changingZone)
            {
                return;
            }

            changingZone = true;
            Zone holdZone = null;
            var currentZoneIndex = persistence.Current.Zone.CurrentIndex;
            int? holdZoneIndex = allowHoldZone ? currentZoneIndex : null;

            if (previousZone != null)
            {
                await previousZone.WaitForGeneration();
                if (previousZone.Index == holdZoneIndex)
                {
                    holdZone = previousZone;
                    holdZone.SetPosition(new Vector3(0, 0, 0));
                }
                else
                {
                    previousZone.RequestDestruction();
                }
            }

            if(currentZone != null)
            {
                await currentZone.WaitForGeneration();
                if (currentZone.Index == holdZoneIndex)
                {
                    holdZone = currentZone;
                    currentZone.ResetPlaceables();
                    currentZone.DestroyPhysics();
                }
                else
                {
                    currentZone.RequestDestruction();
                }
            }

            if(nextZone != null)
            {
                await nextZone.WaitForGeneration();
                if (nextZone.Index == holdZoneIndex)
                {
                    holdZone = nextZone;
                    holdZone.SetPosition(new Vector3(0, 0, 0));
                }
                else
                {
                    nextZone.RequestDestruction();
                }
            }

            if (holdZone != null)
            {
                currentZone = holdZone;
            }
            else
            {
                currentZone = CreateZone(new Vector3(0, 0, 0), currentZoneIndex);
                await currentZone.WaitForGeneration();
            }

            if (currentZone.LoadNextLevel)
            {
                nextZone = CreateZone(new Vector3(150, 0, 0), currentZoneIndex + 1);
            }
            else
            {
                nextZone = null;
            }

            if(currentZone.LoadPreviousLevel)
            {
                previousZone = CreateZone(new Vector3(-150, 0, 0), currentZoneIndex - 1);
            }
            else
            {
                previousZone = null;
            }

            currentZone.SetupPhysics();

            ManagePlayers();

            foreach(var player in players)
            {
                player?.RestorePersistedLocation(currentZone.StartPoint, currentZone.EndPoint, currentZone.StartEnd);
                player?.CenterCamera();
            }

            ZoneChanged?.Invoke(this);

            if (nextZone != null)
            {
                await nextZone.WaitForGeneration();
                var nextOffset = currentZone.LocalEndPoint - nextZone.LocalStartPoint;
                nextZone.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
            }

            if (previousZone != null)
            {
                await previousZone.WaitForGeneration();
                var previousOffset = currentZone.LocalStartPoint - previousZone.LocalEndPoint;
                previousZone.SetPosition(new Vector3(-150, previousOffset.y, previousOffset.z));
            }

            changingZone = false;
        }

        public void Dispose()
        {
            objectResolver.Dispose();
        }

        public Task WaitForCurrent()
        {
            return currentZone?.WaitForGeneration() ?? Task.CompletedTask;
        }

        public Task WaitForNext()
        {
            return nextZone?.WaitForGeneration() ?? Task.CompletedTask;
        }

        public Task WaitForPrevious()
        {
            return previousZone?.WaitForGeneration() ?? Task.CompletedTask;
        }

        public Task GoNext()
        {
            return GoNext(players[0].GetLocation());
        }

        public async Task GoNext(Vector3 triggerLoc)
        {
            if (changingZone)
            {
                return;
            }

            changingZone = true;
            if (previousZone != null)
            {
                await previousZone.WaitForGeneration(); //This is pretty unlikely, but have to stop here if zone isn't created yet
            }
            if (nextZone != null)
            {
                await nextZone.WaitForGeneration(); //Also unlikely, but next zone might not be loaded yet
            }

            //Shuffle zones
            previousZone?.SetPosition(new Vector3(-150 * 2, 0, 0)); //TODO: Hack, move the zone to an out of the way position, the flickering zones are the zone being removed
            previousZone?.RequestDestruction();
            previousZone = currentZone;
            currentZone = nextZone;

            //Change zone index
            ++persistence.Current.Zone.CurrentIndex;
            var nextZoneIndex = persistence.Current.Zone.CurrentIndex + 1;

            //Create new zone
            if (currentZone.LoadNextLevel)
            {
                nextZone = CreateZone(new Vector3(150, 0, 0), nextZoneIndex);
            }
            else
            {
                nextZone = null;
            }

            //Physics changeover
            previousZone.DestroyPhysics();
            var previousOffset = currentZone.LocalStartPoint - previousZone.LocalEndPoint;
            previousZone.SetPosition(new Vector3(-150, previousOffset.y, previousOffset.z));
            currentZone.SetPosition(new Vector3(0, 0, 0));
            currentZone.SetupPhysics();

            var playerLoc = triggerLoc;
            var playerOffset = new Vector3(-150f, previousOffset.y, previousOffset.z);
            playerLoc += playerOffset;
            sky.CelestialOffset += playerOffset;
            var offsetCamera = true;
            foreach (var player in players)
            {
                player?.SetLocation(playerLoc);
                if (offsetCamera)
                {
                    player.OffsetCamera(playerOffset);
                    offsetCamera = false;
                }
            }

            ZoneChanged?.Invoke(this);

            changingZone = false;

            //Keep this last after setting changingZone
            if (nextZone != null)
            {
                await nextZone.WaitForGeneration();
                var nextOffset = currentZone.LocalEndPoint - nextZone.LocalStartPoint;
                nextZone.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
            }
        }

        public Task GoPrevious()
        {
            return GoPrevious(players[0].GetLocation());
        }

        public async Task GoPrevious(Vector3 triggerLoc)
        {
            if (changingZone)
            {
                return;
            }

            //Change zone index
            --persistence.Current.Zone.CurrentIndex;
            if (persistence.Current.Zone.CurrentIndex < 0)
            {
                //Below 0, do nothing
                persistence.Current.Zone.CurrentIndex = 0;
                return;
            }

            changingZone = true;
            if (previousZone != null)
            {
                await previousZone.WaitForGeneration(); //This is pretty unlikely, but have to stop here if zone isn't created yet
            }
            if (nextZone != null)
            {
                await nextZone.WaitForGeneration(); //Also unlikely, but next zone might not be loaded yet
            }

            //Shuffle zones
            nextZone?.SetPosition(new Vector3(150 * 2, 0, 0)); //TODO: Hack, move the zone to an out of the way position, the flickering zones are the zone being removed
            nextZone?.RequestDestruction();
            nextZone = currentZone;
            currentZone = previousZone;

            if (currentZone.LoadPreviousLevel)
            {
                var previousZoneIndex = persistence.Current.Zone.CurrentIndex - 1;
                previousZone = CreateZone(new Vector3(-150, 0, 0), previousZoneIndex);
            }
            else
            {
                previousZone = null;
            }

            //Physics changeover
            nextZone.DestroyPhysics();
            var nextOffset = currentZone.LocalEndPoint - nextZone.LocalStartPoint;
            nextZone.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
            currentZone.SetPosition(new Vector3(0, 0, 0));
            currentZone.SetupPhysics();

            var playerLoc = triggerLoc;
            var playerOffset = new Vector3(150f, nextOffset.y, nextOffset.z);
            playerLoc += playerOffset;
            sky.CelestialOffset += playerOffset;
            var offsetCamera = true;
            foreach (var player in players)
            {
                player?.SetLocation(playerLoc);
                if (offsetCamera)
                {
                    player.OffsetCamera(playerOffset);
                    offsetCamera = false;
                }
            }

            ZoneChanged?.Invoke(this);

            changingZone = false;

            //Keep this last after the changingzones = false;
            if(previousZone != null)
            {
                await previousZone.WaitForGeneration();
                var previousOffset = currentZone.LocalStartPoint - previousZone.LocalEndPoint;
                previousZone.SetPosition(new Vector3(-150, previousOffset.y, previousOffset.z));
            }
        }

        private Zone CreateZone(Vector3 translation, int zoneIndex)
        {
            return this.objectResolver.Resolve<Zone, Zone.Description>(o =>
            {
                worldManager.SetupZone(zoneIndex, o);
                o.Translation = translation;
            });
        }

        public void CenterCamera()
        {
            foreach (var player in players)
            {
                if (player != null)
                {
                    player.CenterCamera();
                }
            }
        }

        public void GoStartPoint()
        {
            foreach (var player in players)
            {
                if (player != null)
                {
                    player.SetLocation(currentZone.StartPoint);
                    player.StopMovement();
                    player.CenterCamera();
                }
            }
        }

        public void GoEndPoint()
        {
            foreach (var player in players)
            {
                if (player != null)
                {
                    player.SetLocation(currentZone.EndPoint);
                    player.StopMovement();
                    player.CenterCamera();
                }
            }
        }

        public void StopPlayer()
        {
            foreach (var player in players)
            {
                player?.StopMovement();
            }
        }

        public Vector3 GetPlayerLoc() => players[0].GetLocation();

        public void ResetPlaceables()
        {
            currentZone.ResetPlaceables();
            previousZone?.ResetPlaceables();
            nextZone?.ResetPlaceables();
            currentZone.DestroyPhysics();
            currentZone.SetupPhysics();
        }

        public void ManagePlayers()
        {
            for(int i = 0; i < players.Length; i++)
            {
                var currentPlayerCharacters = party.ActiveCharacters.Where(c => c.Player == i);
                var playerCharacter = currentPlayerCharacters.FirstOrDefault();
                if (playerCharacter != null)
                {
                    if (players[i] == null)
                    {
                        players[i] = this.objectResolver.Resolve<Player, Player.Description>(c =>
                        {
                            c.Translation = currentZone.StartPoint;
                            c.PlayerSprite = playerCharacter.PlayerSprite;
                            c.CharacterSheet = playerCharacter.CharacterSheet;
                            c.Gamepad = (GamepadId)i;
                        });
                    }

                    players[i].CreateFollowers(currentPlayerCharacters.Skip(1));
                }
                else
                {
                    if (players[i] != null)
                    {
                        players[i].RequestDestruction();
                        players[i] = null;
                    }
                }
            }
        }

        public void DestroyPlayers()
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    players[i].RequestDestruction();
                    players[i] = null;
                }
            }
        }

        public void Update()
        {
            if (currentZone != null)
            {
                foreach(var player in players)
                {
                    if(player != null)
                    {
                        currentZone.CheckZoneConnectorCollision(player.GetLocation());
                    }
                }
            }
        }
    }
}
