﻿using BepuPlugin;
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
        Task Restart(Func<Task> waitForMainThreadWorkCb, bool allowHoldZone = true);
        Task WaitForCurrent();
        void StopPlayer();
        void GoStartPoint();
        void GoEndPoint();
        void ResetPlaceables();
        Vector3 GetPlayerLoc();
        void ManagePlayers();
        void Update();
        void CenterCamera();
        void DestroyPlayers();
        Task<BattleTrigger> FindTrigger(int index, bool isBoss);
    }

    class ZoneManager : IDisposable, IZoneManager
    {
        private bool changingZone = false;
        private Zone currentZone;

        private Player[] players = new Player[4];

        private IObjectResolver objectResolver;
        private readonly Party party;
        private readonly IWorldManager worldManager;
        private readonly IObjectResolverFactory objectResolverFactory;
        private readonly Persistence persistence;
        private readonly Sky sky;
        private readonly PartyMemberManager partyMemberManager;
        private readonly IGcService gcService;

        public event Action<IZoneManager> ZoneChanged;

        public bool ChangingZone => changingZone;

        public Zone Current => currentZone;

        public ZoneManager(
            Party party,
            IWorldManager worldManager,
            IObjectResolverFactory objectResolverFactory,
            Persistence persistence,
            IBepuScene<ZoneScene> bepuScene, //Inject this so it is created earlier and destroyed later
            Sky sky,
            PartyMemberManager partyMemberManager,
            IGcService gcService
        )
        {
            objectResolver = objectResolverFactory.Create();

            this.party = party;
            this.worldManager = worldManager;
            this.objectResolverFactory = objectResolverFactory;
            this.persistence = persistence;
            this.sky = sky;
            this.partyMemberManager = partyMemberManager;
            this.gcService = gcService;
            this.partyMemberManager.PartyChanged += PartyMemberManager_PartyChanged;
        }

        public async Task Restart(Func<Task> waitForMainThreadWorkCb, bool allowHoldZone = true)
        {
            if (changingZone)
            {
                return;
            }

            changingZone = true;
            Zone newZone;
            var currentZoneIndex = persistence.Current.Zone.CurrentIndex;

            if (allowHoldZone && currentZone?.Index == currentZoneIndex) //Reuse the current zone that already exists in memory, if it matches the new one
            {
                newZone = currentZone;
            }
            else
            {
                newZone = CreateZone(new Vector3(0, 0, 0), currentZoneIndex);
            }

            await waitForMainThreadWorkCb();
            await newZone.WaitForGeneration();

            if (currentZone != null)
            {
                await currentZone.WaitForGeneration();
                if (allowHoldZone && currentZone.Index == currentZoneIndex) //This is for reusing the zone, so reset its objects don't destroy it
                {
                    currentZone.ResetPlaceables();
                    currentZone.DestroyPhysics();
                }
                else
                {
                    currentZone.RequestDestruction();
                }
            }

            currentZone = newZone;

            currentZone.SetupPhysics();

            ManagePlayers();

            objectResolverFactory.Flush(); //FLushing this here ensures the old followers are deleted

            foreach (var player in players)
            {
                player?.RestorePersistedLocation(currentZone.StartPoint, currentZone.EndPoint, currentZone.StartEnd, currentZone.ZoneAlignment);
                player?.CenterCamera();
            }

            ZoneChanged?.Invoke(this);

            gcService.Collect();

            changingZone = false;
        }

        public void Dispose()
        {
            objectResolver.Dispose();
            partyMemberManager.PartyChanged -= PartyMemberManager_PartyChanged;
        }

        public Task WaitForCurrent()
        {
            return currentZone?.WaitForFullLoad() ?? Task.CompletedTask;
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
                    player.SetLocation(currentZone.StartPoint, currentZone.ZoneAlignment);
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
                    player.SetLocation(currentZone.EndPoint, Zone.GetEndAlignment(currentZone.ZoneAlignment));
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
            if (currentZone != null)
            {
                currentZone.ResetPlaceables();
                currentZone.DestroyPhysics();
                currentZone.SetupPhysics();
            }
        }

        public void ManagePlayers()
        {
            if(currentZone == null)
            {
                return;
            }

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

        private void PartyMemberManager_PartyChanged()
        {
            ManagePlayers();
        }

        public Task<BattleTrigger> FindTrigger(int index, bool isBoss)
        {
            return currentZone.FindTrigger(index, isBoss);
        }
    }
}
