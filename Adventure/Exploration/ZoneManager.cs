using BepuPlugin;
using Engine;
using Adventure.Exploration;
using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface IZoneManager
    {

        event Action<IZoneManager> ZoneChanged;

        bool ChangingZone { get; }
        Zone CurrentZone { get; }
        bool IsPlayerMoving { get; }
        Task GoNextLevel();
        Task GoPreviousLevel();
        Task Restart();
        Task WaitForCurrentLevel();
        Task WaitForNextLevel();
        Task WaitForPreviousLevel();
        void StopPlayer();
        void GoStartPoint();
        void GoEndPoint();
        void RebuildPhysics();
        Vector3 GetPlayerLoc();
    }

    class ZoneManager : IDisposable, IZoneManager
    {
        private bool changingLevels = false;
        private Zone currentLevel;
        private Zone nextLevel;
        private Zone previousLevel;

        private Player player;
        private IObjectResolver objectResolver;
        private readonly Party party;
        private readonly IWorldManager worldManager;
        private readonly Persistence persistence;

        public event Action<IZoneManager> ZoneChanged;

        public bool ChangingZone => changingLevels;

        public Zone CurrentZone => currentLevel;

        public bool IsPlayerMoving => player?.IsMoving == true;

        public ZoneManager(
            Party party,
            IWorldManager worldManager,
            IObjectResolverFactory objectResolverFactory,
            IBackgroundMusicManager backgroundMusicManager,
            Persistence persistence,
            IBepuScene bepuScene //Inject this so it is created earlier and destroyed later
        )
        {
            objectResolver = objectResolverFactory.Create();

            backgroundMusicManager.SetBackgroundSong("Music/freepd/Rafael Krux - Black Knight.ogg");
            this.party = party;
            this.worldManager = worldManager;
            this.persistence = persistence;
        }

        public async Task Restart()
        {
            if (changingLevels)
            {
                return;
            }

            changingLevels = true;

            if(previousLevel != null)
            {
                await previousLevel.WaitForLevelGeneration();
                previousLevel.RequestDestruction();
            }

            if(currentLevel != null)
            {
                await currentLevel.WaitForLevelGeneration();
                currentLevel.RequestDestruction();
            }

            if(nextLevel != null)
            {
                await nextLevel.WaitForLevelGeneration();
                nextLevel.RequestDestruction();
            }

            var currentLevelIndex = persistence.Zone.CurrentIndex;
            currentLevel = CreateLevel(worldManager.GetLevelSeed(currentLevelIndex), new Vector3(0, 0, 0), currentLevelIndex);
            nextLevel = CreateLevel(worldManager.GetLevelSeed(currentLevelIndex + 1), new Vector3(150, 0, 0), currentLevelIndex + 1);
            if(currentLevelIndex - 1 >= 0)
            {
                previousLevel = CreateLevel(worldManager.GetLevelSeed(currentLevelIndex - 1), new Vector3(-150, 0, 0), currentLevelIndex - 1);
            }

            await currentLevel.WaitForLevelGeneration();

            currentLevel.SetupPhysics();

            if (player == null)
            {
                player = this.objectResolver.Resolve<Player, Player.Description>(c =>
                {
                    c.Translation = currentLevel.StartPoint;
                    var leader = party.ActiveCharacters.First();
                    c.PlayerSpriteInfo = leader.PlayerSprite;
                    c.PrimaryHandItem = leader.PrimaryHandAsset;
                    c.SecondaryHandItem = leader.SecondaryHandAsset;
                });
            }
            else
            {
                player.SetLocation(persistence.Player.Position ?? currentLevel.StartPoint);
            }

            ZoneChanged?.Invoke(this);

            await nextLevel.WaitForLevelGeneration();
            var nextOffset = currentLevel.LocalEndPoint - nextLevel.LocalStartPoint;
            nextLevel.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
            if (previousLevel != null)
            {
                await previousLevel.WaitForLevelGeneration();
                var previousOffset = currentLevel.LocalStartPoint - previousLevel.LocalEndPoint;
                previousLevel.SetPosition(new Vector3(-150, previousOffset.y, previousOffset.z));
            }

            changingLevels = false;
        }

        public void Dispose()
        {
            objectResolver.Dispose();
        }

        public Task WaitForCurrentLevel()
        {
            return currentLevel?.WaitForLevelGeneration();
        }

        public Task WaitForNextLevel()
        {
            return nextLevel?.WaitForLevelGeneration();
        }

        public Task WaitForPreviousLevel()
        {
            return previousLevel?.WaitForLevelGeneration();
        }

        public async Task GoNextLevel()
        {
            if (changingLevels)
            {
                return;
            }

            changingLevels = true;
            if (previousLevel != null)
            {
                await previousLevel.WaitForLevelGeneration(); //This is pretty unlikely, but have to stop here if level isn't created yet
            }
            await nextLevel.WaitForLevelGeneration(); //Also unlikely, but next level might not be loaded yet

            //Shuffle levels
            previousLevel?.SetPosition(new Vector3(-150 * 2, 0, 0)); //TODO: Hack, move the level to an out of the way position, the flickering levels are the level being removed
            previousLevel?.RequestDestruction();
            previousLevel = currentLevel;
            currentLevel = nextLevel;

            //Change level index
            ++persistence.Zone.CurrentIndex;
            var nextLevelIndex = persistence.Zone.CurrentIndex + 1;
            var levelSeed = worldManager.GetLevelSeed(nextLevelIndex);

            //Create new level
            nextLevel = CreateLevel(levelSeed, new Vector3(150, 0, 0), nextLevelIndex);

            //Physics changeover
            previousLevel.DestroyPhysics();
            var previousOffset = currentLevel.LocalStartPoint - previousLevel.LocalEndPoint;
            previousLevel.SetPosition(new Vector3(-150, previousOffset.y, previousOffset.z));
            currentLevel.SetPosition(new Vector3(0, 0, 0));
            currentLevel.SetupPhysics();

            var playerLoc = player.GetLocation();
            playerLoc += new Vector3(-150f, previousOffset.y, previousOffset.z);
            player.SetLocation(playerLoc);

            ZoneChanged?.Invoke(this);

            changingLevels = false;

            //Keep this last after setting changingLevels
            await nextLevel.WaitForLevelGeneration();
            var nextOffset = currentLevel.LocalEndPoint - nextLevel.LocalStartPoint;
            nextLevel.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
        }

        public async Task GoPreviousLevel()
        {
            if (changingLevels)
            {
                return;
            }

            //Change level index
            --persistence.Zone.CurrentIndex;
            if (persistence.Zone.CurrentIndex < 0)
            {
                //Below 0, do nothing
                persistence.Zone.CurrentIndex = 0;
                return;
            }

            changingLevels = true;
            if (previousLevel != null)
            {
                await previousLevel.WaitForLevelGeneration(); //This is pretty unlikely, but have to stop here if level isn't created yet
            }
            await nextLevel.WaitForLevelGeneration(); //Also unlikely, but next level might not be loaded yet

            //Shuffle levels
            nextLevel?.SetPosition(new Vector3(150 * 2, 0, 0)); //TODO: Hack, move the level to an out of the way position, the flickering levels are the level being removed
            nextLevel?.RequestDestruction();
            nextLevel = currentLevel;
            currentLevel = previousLevel;

            if (persistence.Zone.CurrentIndex > 0)
            {
                var previousLevelIndex = persistence.Zone.CurrentIndex - 1;
                var levelSeed = worldManager.GetLevelSeed(previousLevelIndex);
                previousLevel = CreateLevel(levelSeed, new Vector3(-150, 0, 0), previousLevelIndex);
            }
            else
            {
                previousLevel = null;
            }

            //Physics changeover
            nextLevel.DestroyPhysics();
            var nextOffset = currentLevel.LocalEndPoint - nextLevel.LocalStartPoint;
            nextLevel.SetPosition(new Vector3(150, nextOffset.y, nextOffset.z));
            currentLevel.SetPosition(new Vector3(0, 0, 0));
            currentLevel.SetupPhysics();

            var playerLoc = player.GetLocation();
            playerLoc += new Vector3(150f, nextOffset.y, nextOffset.z);
            player.SetLocation(playerLoc);

            ZoneChanged?.Invoke(this);

            changingLevels = false;

            //Keep this last after the changingLevels = false;
            if(previousLevel != null)
            {
                await previousLevel.WaitForLevelGeneration();
                var previousOffset = currentLevel.LocalStartPoint - previousLevel.LocalEndPoint;
                previousLevel.SetPosition(new Vector3(-150, previousOffset.y, previousOffset.z));
            }
        }

        private Zone CreateLevel(int levelSeed, Vector3 translation, int levelIndex)
        {
            return this.objectResolver.Resolve<Zone, Zone.Description>(o =>
            {
                o.Index = levelIndex;
                o.Translation = translation;
                o.RandomSeed = levelSeed;
                o.Width = 50;
                o.Height = 50;
                o.CorridorSpace = 10;
                o.RoomDistance = 3;
                o.RoomMin = new IntSize2(2, 2);
                o.RoomMax = new IntSize2(6, 6); //Between 3-6 is good here, 3 for more cityish with small rooms, 6 for more open with more big rooms, sometimes connected
                o.CorridorMaxLength = 4;
                o.GoPrevious = levelIndex != 0;
                o.EnemyLevel = 20;
            });
        }

        public void GoStartPoint()
        {
            player.SetLocation(currentLevel.StartPoint);
        }

        public void GoEndPoint()
        {
            player.SetLocation(currentLevel.EndPoint);
        }

        public void StopPlayer()
        {
            player.StopMovement();
        }

        public Vector3 GetPlayerLoc() => player.GetLocation();

        public void RebuildPhysics()
        {
            currentLevel.DestroyPhysics();
            currentLevel.SetupPhysics();
        }
    }
}
