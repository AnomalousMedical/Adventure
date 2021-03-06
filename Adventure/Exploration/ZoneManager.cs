using BepuPlugin;
using Engine;
using Adventure.Exploration;
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
        Task Restart();
        Task WaitForCurrent();
        Task WaitForNext();
        Task WaitForPrevious();
        void StopPlayer();
        void GoStartPoint();
        void GoEndPoint();
        void ResetPlaceables();
        Vector3 GetPlayerLoc();
        void ManagePlayers();
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

        public event Action<IZoneManager> ZoneChanged;

        public bool ChangingZone => changingZone;

        public Zone Current => currentZone;

        public ZoneManager(
            Party party,
            IWorldManager worldManager,
            IObjectResolverFactory objectResolverFactory,
            Persistence persistence,
            IBepuScene bepuScene //Inject this so it is created earlier and destroyed later
        )
        {
            objectResolver = objectResolverFactory.Create();

            this.party = party;
            this.worldManager = worldManager;
            this.persistence = persistence;
        }

        public async Task Restart()
        {
            if (changingZone)
            {
                return;
            }

            changingZone = true;

            if(previousZone != null)
            {
                await previousZone.WaitForGeneration();
                previousZone.RequestDestruction();
            }

            if(currentZone != null)
            {
                await currentZone.WaitForGeneration();
                currentZone.RequestDestruction();
            }

            if(nextZone != null)
            {
                await nextZone.WaitForGeneration();
                nextZone.RequestDestruction();
            }

            var currentZoneIndex = persistence.Current.Zone.CurrentIndex;
            currentZone = CreateZone(new Vector3(0, 0, 0), currentZoneIndex);
            nextZone = CreateZone(new Vector3(150, 0, 0), currentZoneIndex + 1);
            if(currentZoneIndex - 1 >= 0)
            {
                previousZone = CreateZone(new Vector3(-150, 0, 0), currentZoneIndex - 1);
            }

            await currentZone.WaitForGeneration();

            currentZone.SetupPhysics();

            ManagePlayers();

            foreach(var player in players)
            {
                player?.RestorePersistedLocation(currentZone.StartPoint);
            }

            ZoneChanged?.Invoke(this);

            await nextZone.WaitForGeneration();
            var nextOffset = currentZone.LocalEndPoint - nextZone.LocalStartPoint;
            nextZone.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
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
            return currentZone?.WaitForGeneration();
        }

        public Task WaitForNext()
        {
            return nextZone?.WaitForGeneration();
        }

        public Task WaitForPrevious()
        {
            return previousZone?.WaitForGeneration();
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
            await nextZone.WaitForGeneration(); //Also unlikely, but next zone might not be loaded yet

            //Shuffle zones
            previousZone?.SetPosition(new Vector3(-150 * 2, 0, 0)); //TODO: Hack, move the zone to an out of the way position, the flickering zones are the zone being removed
            previousZone?.RequestDestruction();
            previousZone = currentZone;
            currentZone = nextZone;

            //Change zone index
            ++persistence.Current.Zone.CurrentIndex;
            var nextZoneIndex = persistence.Current.Zone.CurrentIndex + 1;

            //Create new zone
            nextZone = CreateZone(new Vector3(150, 0, 0), nextZoneIndex);

            //Physics changeover
            previousZone.DestroyPhysics();
            var previousOffset = currentZone.LocalStartPoint - previousZone.LocalEndPoint;
            previousZone.SetPosition(new Vector3(-150, previousOffset.y, previousOffset.z));
            currentZone.SetPosition(new Vector3(0, 0, 0));
            currentZone.SetupPhysics();

            var playerLoc = triggerLoc;
            playerLoc += new Vector3(-150f, previousOffset.y, previousOffset.z);
            foreach(var player in players)
            {
                player?.SetLocation(playerLoc);
            }

            ZoneChanged?.Invoke(this);

            changingZone = false;

            //Keep this last after setting changingZone
            await nextZone.WaitForGeneration();
            var nextOffset = currentZone.LocalEndPoint - nextZone.LocalStartPoint;
            nextZone.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
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
            await nextZone.WaitForGeneration(); //Also unlikely, but next zone might not be loaded yet

            //Shuffle zones
            nextZone?.SetPosition(new Vector3(150 * 2, 0, 0)); //TODO: Hack, move the zone to an out of the way position, the flickering zones are the zone being removed
            nextZone?.RequestDestruction();
            nextZone = currentZone;
            currentZone = previousZone;

            if (persistence.Current.Zone.CurrentIndex > 0)
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
            playerLoc += new Vector3(150f, nextOffset.y, nextOffset.z);
            foreach (var player in players)
            {
                player?.SetLocation(playerLoc);
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

        public void GoStartPoint()
        {
            foreach (var player in players)
            {
                player?.SetLocation(currentZone.StartPoint);
            }
        }

        public void GoEndPoint()
        {
            foreach (var player in players)
            {
                player?.SetLocation(currentZone.EndPoint);
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
                var playerCharacter = party.ActiveCharacters.Where(c => c.Player == i).FirstOrDefault();
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
    }
}
