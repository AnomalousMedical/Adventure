using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using DungeonGenerator;
using Engine;
using RogueLikeMapBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rectangle = Engine.IntRect;
using Point = Engine.IntVector2;
using Size = Engine.IntSize2;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using DiligentEngine.RT.HLSL;
using Adventure.Services;
using Adventure.Assets.World;
using FreeImageAPI;
using Adventure.Exploration;

namespace Adventure
{
    class Zone : IDisposable
    {
        public class Description
        {
            public int Index { get; set; }

            public Vector3 Translation { get; set; } = Vector3.Zero;

            public int LevelSeed { get; set; }

            public int EnemySeed { get; set; }

            public int Width { get; set; } = 50;

            public int Height { get; set; } = 50;

            public float MapUnitX { get; set; } = 3.0f;

            public float MapUnitY { get; set; } = 0.1f;

            public float MapUnitZ { get; set; } = 1.5f;

            public float CorridorSlopeMultiple { get; set; } = 1.0f;

            /// <summary>
            /// Room minimum size
            /// </summary>
            public Size RoomMin { get; set; } = new Size(3, 3);

            /// <summary>
            /// Room max size
            /// </summary>
            public Size RoomMax { get; set; } = new Size(10, 10);

            /// <summary>
            /// Number of rooms to build
            /// </summary>
            public int MaxRooms { get; set; } = 15;

            /// <summary>
            /// Minimum distance between rooms
            /// </summary>
            public int RoomDistance { get; set; } = 5;

            /// <summary>
            /// Minimum distance of room from existing corridors
            /// </summary>
            public int CorridorDistance { get; set; } = 2;

            /// <summary>
            /// Minimum corridor length
            /// </summary>
            public int CorridorMinLength { get; set; } = 3;
            /// <summary>
            /// Maximum corridor length
            /// </summary>
            public int CorridorMaxLength { get; set; } = 15;
            /// <summary>
            /// Maximum turns
            /// </summary>
            public int CorridorMaxTurns { get; set; } = 5;
            /// <summary>
            /// The distance a corridor has to be away from a closed cell for it to be built
            /// </summary>
            public int CorridorSpace { get; set; } = 10;

            /// <summary>
            /// Probability of building a corridor from a room or corridor. Greater than value = room
            /// </summary>
            public int BuildProb { get; set; } = 50;

            /// <summary>
            /// Break out
            /// </summary>
            public int BreakOut { get; set; } = 250;

            /// <summary>
            /// True if this zone has a go previous zone connector. Default: true
            /// </summary>
            public bool GoPrevious { get; set; } = true;

            /// <summary>
            /// Set this to true to use the end point as the starting point.
            /// </summary>
            public bool StartEnd { get; set; }

            /// <summary>
            /// Set this to true to make a rest area in this zone.
            /// </summary>
            public bool MakeRest { get; set; }

            /// <summary>
            /// Set this to true to make Philip in this zone.
            /// </summary>
            public bool MakePhilip { get; set; }

            public bool MakeBoss { get; set; }

            public bool MakeGate { get; set; }

            /// <summary>
            /// The level of the enemies from 1 to 99
            /// </summary>
            public int EnemyLevel { get; set; }

            public bool ConnectPreviousToWorld { get; set; }

            public bool ConnectNextToWorld { get; set; }

            /// <summary>
            /// The number of battles for the level's "main" corridor.
            /// Must be at least 1, default is int.MaxValue. This will
            /// base it on the actual size.
            /// </summary>
            public int MaxMainCorridorBattles { get; set; } = int.MaxValue;

            public IBiome Biome { get; set; }

            public IEnumerable<ITreasure> Treasure { get; set; }

            public IEnumerable<ITreasure> StealTreasure { get; set; }

            public IEnumerable<ITreasure> BossStealTreasure { get; set; }

            public IEnumerable<ITreasure> UniqueStealTreasure { get; set; }

            public IEnumerable<ITreasure> BossUniqueStealTreasure { get; set; }

            public PlotItems? PlotItem { get; set; }

            public IEnumerable<Persistence.CharacterData> PartyMembers { get; set; }

            public int Area { get; set; }
        }

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly RayTracingRenderer renderer;
        private readonly Persistence persistence;
        private readonly NoiseTextureManager noiseTextureManager;
        private readonly IDestructionRequest destructionRequest;
        private readonly IBepuScene<ZoneScene> bepuScene;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly ILogger<Zone> logger;
        private PrimaryHitShader floorShader;
        private CC0TextureResult floorTexture;
        private CC0TextureResult floorTexture2;
        private CC0TextureResult wallTexture;
        private CC0TextureResult wallTexture2;
        private CC0TextureResult noiseTexture;
        private readonly TLASInstanceData floorInstanceData;
        private List<StaticHandle> staticHandles = new List<StaticHandle>();
        private TypedIndex boundaryCubeShapeIndex;
        private TypedIndex floorCubeShapeIndex;
        private MapMesh mapMesh;
        private bool physicsActive = false;
        private readonly IObjectResolver objectResolver;
        private ZoneConnector nextZoneConnector;
        private ZoneConnector previousZoneConnector;
        private List<IZonePlaceable> placeables = new List<IZonePlaceable>();
        private IBiome biome;
        private bool goPrevious;
        private BlasInstanceData floorBlasInstanceData;
        private int enemySeed;
        private int index;
        private bool makeRestArea;
        private bool makePhilip;
        private bool makeBoss;
        private bool makeGate;
        private int enemyLevel;
        private int maxMainCorridorBattles;
        private IEnumerable<ITreasure> treasure;
        private IEnumerable<Persistence.CharacterData> partyMembers;
        private bool connectPreviousToWorld;
        private bool connectNextToWorld;
        private PlotItems? plotItem;
        private LootDropTrigger lootDropTrigger;
        private ushort startRoomIndex = ushort.MaxValue;

        private Task zoneGenerationTask;
        private Vector3 mapUnits;

        private Vector3 endPointLocal;
        private Vector3 startPointLocal;
        private Vector3 currentPosition;

        public bool StartEnd { get; init; }

        public Vector3 StartPoint => startPointLocal + currentPosition;
        public Vector3 EndPoint => endPointLocal + currentPosition;

        public Vector3 LocalStartPoint => startPointLocal;
        public Vector3 LocalEndPoint => endPointLocal;

        public bool PhysicsActive => physicsActive;

        public int Index => index;

        public bool LoadPreviousLevel => index - 1 > -1 && !connectPreviousToWorld;

        public bool LoadNextLevel => !connectNextToWorld;

        public int Area { get; init; }

        public Zone
        (
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<ZoneScene> bepuScene,
            Description description,
            ILogger<Zone> logger,
            IObjectResolverFactory objectResolverFactory,
            MeshBLAS floorMesh,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RTInstances<ZoneScene> rtInstances,
            RayTracingRenderer renderer,
            Persistence persistence,
            NoiseTextureManager noiseTextureManager,
            TerrainNoise terrainNoise
        )
        {
            this.plotItem = description.PlotItem;
            this.connectPreviousToWorld = description.ConnectPreviousToWorld;
            this.connectNextToWorld = description.ConnectNextToWorld;
            this.StartEnd = description.StartEnd;
            this.maxMainCorridorBattles = description.MaxMainCorridorBattles > 0 ? description.MaxMainCorridorBattles : throw new InvalidOperationException("You must have a max main corridor fight count of at least 1.");
            this.enemyLevel = description.EnemyLevel;
            this.index = description.Index;
            this.enemySeed = description.EnemySeed;
            this.makeRestArea = description.MakeRest;
            this.Area = description.Area;
            this.makePhilip = description.MakePhilip;
            this.makeBoss = description.MakeBoss;
            this.makeGate = description.MakeGate;
            this.mapUnits = new Vector3(description.MapUnitX, description.MapUnitY, description.MapUnitZ);
            this.objectResolver = objectResolverFactory.Create();
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.logger = logger;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.rtInstances = rtInstances;
            this.renderer = renderer;
            this.persistence = persistence;
            this.noiseTextureManager = noiseTextureManager;
            this.goPrevious = description.GoPrevious;
            this.biome = description.Biome;
            this.treasure = description.Treasure ?? Enumerable.Empty<ITreasure>();
            this.partyMembers = description.PartyMembers ?? Enumerable.Empty<Persistence.CharacterData>();

            this.currentPosition = description.Translation;

            this.floorInstanceData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("ZoneFloor"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, Quaternion.Identity)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                var floorTextureDesc = new CCOTextureBindingDescription(biome.FloorTexture, reflective: biome.ReflectFloor);
                var floorTextureDesc2 = new CCOTextureBindingDescription(biome.FloorTexture2 ?? biome.FloorTexture, reflective: biome.ReflectFloor);
                var wallTextureDesc = new CCOTextureBindingDescription(biome.WallTexture, reflective: biome.ReflectWall);
                var wallTextureDesc2 = new CCOTextureBindingDescription(biome.WallTexture2 ?? biome.WallTexture, reflective: biome.ReflectWall);

                var floorTextureTask = textureManager.Checkout(floorTextureDesc);
                var floorTexture2Task = textureManager.Checkout(floorTextureDesc2);
                var wallTextureTask = textureManager.Checkout(wallTextureDesc);
                var wallTexture2Task = textureManager.Checkout(wallTextureDesc2);

                var noise = biome.CreateNoise?.Invoke(description.LevelSeed) ?? terrainNoise.CreateBlendTerrainNoise(description.LevelSeed);

                var noiseTask = noiseTextureManager.GenerateTexture(noise, 4096, 4096);

                this.zoneGenerationTask = Task.Run(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var random = new FIRandom(description.LevelSeed);
                    var mapBuilder = new csMapbuilder(random, description.Width, description.Height)
                    {
                        BreakOut = description.BreakOut,
                        BuildProb = description.BuildProb,
                        CorridorDistance = description.CorridorDistance,
                        CorridorSpace = description.CorridorSpace,
                        Corridor_Max = description.CorridorMaxLength,
                        Corridor_MaxTurns = description.CorridorMaxTurns,
                        Corridor_Min = description.CorridorMinLength,
                        MaxRooms = description.MaxRooms,
                        RoomDistance = description.RoomDistance,
                        Room_Max = description.RoomMax,
                        Room_Min = description.RoomMin
                    };
                    mapBuilder.Build_ConnectedStartRooms();
                    mapBuilder.AddEastConnector();
                    if (description.GoPrevious)
                    {
                        mapBuilder.AddWestConnector();
                    }
                    mapBuilder.AddTopBottomPad(75, 75);

                    int startX, startY;
                    if (description.GoPrevious)
                    {
                        var startConnector = mapBuilder.WestConnector.Value;
                        startX = startConnector.x;
                        startY = startConnector.y;
                    }
                    else
                    {
                        Rectangle startRoom = new Rectangle(int.MaxValue, 0, 0, 0);
                        var numRooms = mapBuilder.Rooms.Count;
                        for(ushort i = 0; i < numRooms; i++)
                        {
                            var room = mapBuilder.Rooms[i];
                            if (room.Left < startRoom.Left)
                            {
                                startRoom = room;
                                this.startRoomIndex = i;
                            }
                        }

                        startX = startRoom.Left + startRoom.Width / 2;
                        startY = startRoom.Top + startRoom.Height / 2;
                    }

                    mapMesh = new MapMesh(mapBuilder, floorMesh, mapUnitX: description.MapUnitX, mapUnitY: description.MapUnitY, mapUnitZ: description.MapUnitZ, corridorSlopeMultiple: description.CorridorSlopeMultiple);

                    startPointLocal = mapMesh.PointToVector(startX, startY);
                    var endConnector = mapBuilder.EastConnector.Value;
                    endPointLocal = mapMesh.PointToVector(endConnector.x, endConnector.y);

                    sw.Stop();
                    logger.LogInformation($"Generated zone {description.Index} seed {description.LevelSeed} in {sw.ElapsedMilliseconds} ms.");
                });

                await zoneGenerationTask; //Need the zone before kicking off the calls to End() below.

                await floorMesh.End("ZoneFloor");

                //TODO: The zone BLASes must be loaded before the shaders, see todo in PrimaryHitShader
                var floorShaderSetup = primaryHitShaderFactory.Checkout();

                await Task.WhenAll
                (
                    floorTextureTask,
                    floorTexture2Task,
                    wallTextureTask,
                    wallTexture2Task,
                    floorShaderSetup,
                    noiseTask
                );

                this.floorShader = floorShaderSetup.Result;
                this.floorTexture = floorTextureTask.Result;
                this.floorTexture2 = floorTexture2Task.Result;
                this.wallTexture = wallTextureTask.Result;
                this.wallTexture2 = wallTexture2Task.Result;
                this.noiseTexture = noiseTask.Result;

                this.floorInstanceData.pBLAS = mapMesh.FloorMesh.Instance.BLAS.Obj;

                rtInstances.AddShaderTableBinder(Bind);
                floorBlasInstanceData = activeTextures.AddActiveTexture(floorTexture, floorTexture2, wallTexture, wallTexture2, noiseTexture);
                floorBlasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, biome.ReflectFloor, false, BlasSpecialMaterial.MultiTexture);
                floorBlasInstanceData.padding = 4; //The padding is the noise, which is the 5th texture
                rtInstances.AddTlasBuild(floorInstanceData);

                ResetPlacementData();
                var enemyRandom = new FIRandom(enemySeed);
                var usedCorridors = new HashSet<int>();

                var battleTriggers = new List<BattleTrigger>();
                SetupCorridors(enemyRandom, usedCorridors, battleTriggers);
                SetupRooms(enemyRandom, out var bossBattleTrigger, out var treasureStack);
                PlaceKeySafety(enemyRandom, usedCorridors);

                if (biome.BackgroundItems != null)
                {
                    CreateBackgroundItems(enemyRandom, biome);
                }

                ResetLootDrop();
                AddStolenTreasure(description, enemyRandom, battleTriggers, bossBattleTrigger, treasureStack);

                //Since this is async the physics can be active before the placeables are created
                if (physicsActive)
                {
                    foreach (var placeable in placeables)
                    {
                        placeable.CreatePhysics();
                    }
                }
            });
        }

        internal void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        public void Dispose()
        {
            foreach (var placeable in placeables)
            {
                placeable.RequestDestruction();
            }

            objectResolver.Dispose();
            DestroyPhysics();
            activeTextures.RemoveActiveTexture(wallTexture);
            activeTextures.RemoveActiveTexture(wallTexture2);
            activeTextures.RemoveActiveTexture(floorTexture);
            activeTextures.RemoveActiveTexture(floorTexture2);
            activeTextures.RemoveActiveTexture(noiseTexture);
            textureManager.TryReturn(wallTexture2);
            textureManager.TryReturn(wallTexture);
            textureManager.TryReturn(floorTexture);
            textureManager.TryReturn(floorTexture2);
            noiseTextureManager.ReturnTexture(noiseTexture);
            rtInstances.RemoveShaderTableBinder(Bind);
            primaryHitShaderFactory.TryReturn(floorShader);
            rtInstances.RemoveTlasBuild(floorInstanceData);
        }

        /// <summary>
        /// Zones are created in the background. Await this function to wait until it has finished
        /// being created. This only means the zone is defined, not that its mesh is created or textures loaded.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForGeneration()
        {
            if (zoneGenerationTask != null)
            {
                await zoneGenerationTask;
            }
        }

        public void SetPosition(in Vector3 position)
        {
            this.currentPosition = position;
            this.floorInstanceData.Transform = new InstanceMatrix(position, Quaternion.Identity);
            foreach (var placeable in placeables)
            {
                placeable.SetZonePosition(position);
            }
        }

        public void CheckZoneConnectorCollision(in Vector3 testPoint)
        {
            previousZoneConnector?.DetectCollision(testPoint);
            nextZoneConnector?.DetectCollision(testPoint);
        }

        public IBiome Biome => biome;

        public int EnemyLevel => enemyLevel;

        public void ResetPlaceables()
        {
            ResetLootDrop();
            foreach (var placeable in placeables)
            {
                placeable.Reset();
            }
        }

        /// <summary>
        /// Add physics shapes to scene. Should wait until the zone generation is complete first.
        /// </summary>
        public void SetupPhysics()
        {
            if (physicsActive)
            {
                //Don't do anything if physics are active
                return;
            }

            physicsActive = true;

            float yBoundaryScale = 50f;

            //Add stuff to physics scene
            var boundaryCubeShape = new Box(mapMesh.MapUnitX, mapMesh.MapUnitY * yBoundaryScale, mapMesh.MapUnitZ); //Each one creates its own, try to load from resources
            boundaryCubeShapeIndex = bepuScene.Simulation.Shapes.Add(boundaryCubeShape);

            var floorCubeShape = new Box(mapMesh.MapUnitX, mapMesh.MapUnitY, mapMesh.MapUnitZ); //Each one creates its own, try to load from resources
            floorCubeShapeIndex = bepuScene.Simulation.Shapes.Add(floorCubeShape);

            var boundaryOrientation = System.Numerics.Quaternion.Identity;

            foreach (var boundary in mapMesh.FloorCubeCenterPoints)
            {
                //TODO: Figure out where nans are coming from
                var orientation = boundary.Orientation.isNumber() ? boundary.Orientation : Quaternion.Identity;
                var staticHandle = bepuScene.Simulation.Statics.Add(
                    new StaticDescription(
                        (boundary.Position + currentPosition).ToSystemNumerics(),
                        orientation.ToSystemNumerics(),
                        new CollidableDescription(floorCubeShapeIndex, 0.1f)));

                staticHandles.Add(staticHandle);
            }

            foreach (var boundary in mapMesh.BoundaryCubeCenterPoints)
            {
                var staticHandle = bepuScene.Simulation.Statics.Add(
                    new StaticDescription(
                        (boundary + currentPosition).ToSystemNumerics(),
                        boundaryOrientation,
                        new CollidableDescription(boundaryCubeShapeIndex, 0.1f)));

                staticHandles.Add(staticHandle);
            }

            if (goPrevious)
            {
                this.previousZoneConnector = objectResolver.Resolve<ZoneConnector, ZoneConnector.Description>(o =>
                {
                    o.Scale = new Vector3(mapUnits.x, 50f, mapUnits.z);
                    o.Translation = StartPoint + new Vector3(-mapUnits.x * 2f, 0f, 0f);
                    o.GoPrevious = true;
                    o.GoWorld = connectPreviousToWorld;
                });
            }

            this.nextZoneConnector = objectResolver.Resolve<ZoneConnector, ZoneConnector.Description>(o =>
            {
                o.Scale = new Vector3(mapUnits.x, 50f, mapUnits.z);
                o.Translation = EndPoint + new Vector3(mapUnits.x * 2f, 0f, 0f);
                o.GoPrevious = false;
                o.GoWorld = connectNextToWorld;
            });

            foreach (var placeable in placeables)
            {
                placeable.CreatePhysics();
            }
        }

        private int restIndex;
        private int treasureIndex;
        private int enemyIndex;
        private bool placeRestArea;
        private bool placePhilip;
        private bool placeBoss;
        private bool placeGate;
        private bool placeKey;
        private bool placeFirstChest;

        private void ResetPlacementData()
        {
            restIndex = 0;
            treasureIndex = 0;
            enemyIndex = 0;
            placeRestArea = this.makeRestArea;
            placePhilip = this.makePhilip;
            placeBoss = this.makeBoss;
            placeKey = placeGate = makeGate;
            placeFirstChest = true;
        }

        private void ResetLootDrop()
        {
            if (lootDropTrigger != null)
            {
                lootDropTrigger.RequestDestruction();
                lootDropTrigger = null;
            }

            if (persistence.Current.Player.LootDropZone == index)
            {
                var lootDrop = objectResolver.Resolve<LootDropTrigger, LootDropTrigger.Description>(o =>
                {
                    o.MapOffset = persistence.Current.Player.LootDropPosition.Value;
                    o.Translation = currentPosition + o.MapOffset;
                    var treasure = new GoldPile();
                    o.Sprite = treasure.CreateSprite();
                    o.SpriteMaterial = treasure.CreateMaterial();
                });
                this.placeables.Add(lootDrop);
                lootDrop.Disposed += () => this.placeables.Remove(lootDrop);
                lootDropTrigger = lootDrop; //Don't combine this with above to keep disposed cb working
            }
        }

        private void SetupCorridors(FIRandom enemyRandom, HashSet<int> usedCorridors, List<BattleTrigger> battleTriggers)
        {
            var corridorStartIndex = 0;
            var corridors = mapMesh.MapBuilder.Corridors;
            var numCorridors = corridors.Count;
            var firstPoint = corridors[0];
            var currentCorridor = mapMesh.MapBuilder.map[firstPoint.x, firstPoint.y];
            for (var currentIndex = 0; currentIndex < numCorridors; ++currentIndex)
            {
                var corridorPoint = corridors[currentIndex];
                var testCorridor = mapMesh.MapBuilder.map[corridorPoint.x, corridorPoint.y];
                if (currentCorridor != testCorridor)
                {
                    if (currentCorridor != mapMesh.MapBuilder.EastConnectorIndex
                     && currentCorridor != mapMesh.MapBuilder.WestConnectorIndex
                     && currentCorridor != mapMesh.MapBuilder.NorthConnectorIndex
                     && currentCorridor != mapMesh.MapBuilder.SouthConnectorIndex)
                    {
                        if (currentCorridor >= csMapbuilder.CorridorCell)
                        {
                            if (currentCorridor == csMapbuilder.MainCorridorCell)
                            {
                                PopulateCorridor(enemyRandom, usedCorridors, corridorStartIndex, currentIndex, maxMainCorridorBattles, battleTriggers);
                            }
                            else
                            {
                                PopulateCorridor(enemyRandom, usedCorridors, corridorStartIndex, currentIndex, 1, battleTriggers);
                            }
                        }
                        corridorStartIndex = currentIndex;
                    }
                    currentCorridor = testCorridor;
                }
            }
        }

        private void PopulateCorridor(FIRandom enemyRandom, HashSet<int> usedCorridors, int corridorStartIndex, int currentIndex, int maxPossibleFights, List<BattleTrigger> battleTriggers)
        {
            var numSquares = currentIndex - corridorStartIndex;
            var maxFights = Math.Min(Math.Max(numSquares / 10, 2), maxPossibleFights);
            var minFights = Math.Max(numSquares / 20, 1);
            if (minFights > maxFights)
            {
                minFights = 1;
            }
            var numEnemies = enemyRandom.Next(minFights, maxFights);
            for (int i = 0; i < numEnemies; ++i)
            {
                var corridorTry = 0;
                var corridorIndex = enemyRandom.Next(corridorStartIndex, currentIndex);
                while (usedCorridors.Contains(corridorIndex))
                {
                    if (++corridorTry > 50)
                    {
                        //If we generate too many bad random numbers, just get the first index we can from the list
                        for (corridorIndex = corridorStartIndex; corridorIndex < currentIndex && usedCorridors.Contains(corridorIndex); ++corridorIndex) { }
                        if (corridorIndex >= currentIndex)
                        {
                            throw new InvalidOperationException("This should not happen, but ran out of corridors trying to place enemies. This is guarded in the constructor.");
                        }
                    }
                    else
                    {
                        corridorIndex = enemyRandom.Next(corridorStartIndex, currentIndex);
                    }
                }
                usedCorridors.Add(corridorIndex);
                var point = mapMesh.MapBuilder.Corridors[corridorIndex];

                var battleTrigger = objectResolver.Resolve<BattleTrigger, BattleTrigger.Description>(o =>
                {
                    o.MapOffset = mapMesh.PointToVector(point.x, point.y);
                    o.Translation = currentPosition + o.MapOffset;
                    var enemy = biome.RegularEnemies[enemyRandom.Next(biome.RegularEnemies.Count)];
                    o.TriggerEnemy = enemy;
                    o.Zone = index;
                    o.Area = Area;
                    o.Index = enemyIndex++;
                    o.EnemyLevel = enemyLevel;
                    o.BattleSeed = enemyRandom.Next(int.MinValue, int.MaxValue);
                });
                battleTriggers.Add(battleTrigger);
                placeables.Add(battleTrigger);
            }
        }

        private void SetupRooms(FIRandom enemyRandom, out BattleTrigger bossBattleTrigger, out Stack<ITreasure> treasureStack)
        {
            //The order of everything in this function is important to ensure all treasure can be distributed

            bossBattleTrigger = null;
            var treasureChests = new List<TreasureTrigger>();
            treasureStack = new Stack<ITreasure>(this.treasure);

            var rooms = mapMesh.MapBuilder.GetDesiredRooms().ToList();
            var skipRooms = 0;

            int GetRoom()
            {
                int roomIndex;
                do
                {
                    if(skipRooms > rooms.Count)
                    {
                        return rooms[rooms.Count - 1];
                    }
                    roomIndex = rooms[skipRooms];
                    skipRooms++;
                } while (roomIndex == startRoomIndex);

                return roomIndex;
            }

            if (placeKey)
            {
                //This might not be possible, so the key will go in a corridor later if it isn't placed here
                var keyRoomIndex = GetRoom();
                var room = mapMesh.MapBuilder.Rooms[keyRoomIndex];
                var point = new Point(room.Left + room.Width / 2, room.Top + room.Height / 2);
                PlaceKey(point);
            }

            //Philip gets a room always
            if (placePhilip)
            {
                var philipRoom = GetRoom();
                var room = mapMesh.MapBuilder.Rooms[philipRoom];
                var point = new Point(room.Left + room.Width / 2, room.Top + room.Height / 2);
                var mapLoc = mapMesh.PointToVector(point.x, point.y);

                placePhilip = false;
                var philip = objectResolver.Resolve<Philip, Philip.Description>(o =>
                {
                    o.ZoneIndex = index;
                    o.MapOffset = mapLoc;
                    o.Translation = currentPosition + o.MapOffset;
                });
                this.placeables.Add(philip);
            }

            var partyMemberIndex = 0;
            foreach (var partyMember in partyMembers)
            {
                var partyMemberRoom = GetRoom();
                var room = mapMesh.MapBuilder.Rooms[partyMemberRoom];
                Point point;
                switch (partyMemberIndex) //This will only really work with 4 characters
                {
                    case 0:
                        point = new Point(room.Left + room.Width, room.Top + room.Height);
                        break;
                    case 1:
                        point = new Point(room.Left + room.Width, room.Top);
                        break;
                    case 2:
                        point = new Point(room.Left, room.Top + room.Height);
                        break;
                    case 3:
                        point = new Point(room.Left, room.Top);
                        break;
                    default:
                        throw new NotImplementedException("Currently only supports 4 characters in a zone.");
                }
                var mapLoc = mapMesh.PointToVector(point.x, point.y);

                var partyMemberObject = objectResolver.Resolve<PartyMemberTrigger, PartyMemberTrigger.Description>(o =>
                {
                    o.ZoneIndex = index;
                    o.InstanceId = partyMemberIndex++;
                    o.MapOffset = mapLoc;
                    o.Translation = currentPosition + o.MapOffset;
                    o.Sprite = partyMember.PlayerSprite;
                    o.PartyMember = partyMember;
                });
                this.placeables.Add(partyMemberObject);
            }

            //The plot item goes in the exit corridor, not the room
            if (this.plotItem != null)
            {
                var point = mapMesh.MapBuilder.EastConnector.Value;
                var plotItemPlaceable = objectResolver.Resolve<PlotItemPlaceable, PlotItemPlaceable.Description>(o =>
                {
                    o.MapOffset = mapMesh.PointToVector(point.x, point.y);
                    o.Translation = currentPosition + o.MapOffset + new Vector3(2.25f, 0f, 0f);
                    var gateAsset = biome.KeyAsset;
                    o.Sprite = gateAsset.CreateSprite();
                    o.SpriteMaterial = gateAsset.CreateMaterial();
                    o.Scale = new Vector3(2f, 2f, 1f);
                    o.PlotItem = this.plotItem.Value;
                });
                placeables.Add(plotItemPlaceable);
            }

            //The boss goes in the exit corridor, not the room
            if (placeBoss)
            {
                var point = mapMesh.MapBuilder.EastConnector.Value;
                bossBattleTrigger = objectResolver.Resolve<BattleTrigger, BattleTrigger.Description>(o =>
                {
                    o.MapOffset = mapMesh.PointToVector(point.x, point.y);
                    o.Translation = currentPosition + o.MapOffset + new Vector3(1.25f, 0f, 0f);
                    o.TriggerEnemy = biome.BossEnemy;
                    o.Zone = index;
                    o.Area = Area;
                    o.Index = 0; //Only ever 1 boss
                    o.EnemyLevel = enemyLevel;
                    o.BattleSeed = enemyRandom.Next(int.MinValue, int.MaxValue);
                    o.IsBoss = true;
                    o.Scale = new Vector3(2f, 2f, 1f);
                });
                placeables.Add(bossBattleTrigger);
            }

            //The gate goes in the exit corridor, not the room
            if (placeGate)
            {
                var point = mapMesh.MapBuilder.EastConnector.Value;
                var gate = objectResolver.Resolve<Gate, Gate.Description>(o =>
                {
                    o.MapOffset = mapMesh.PointToVector(point.x, point.y);
                    o.Translation = currentPosition + o.MapOffset;
                    var gateAsset = biome.GateAsset;
                    o.Sprite = gateAsset.CreateSprite();
                    o.SpriteMaterial = gateAsset.CreateMaterial();
                    o.Zone = index;
                    o.InstanceId = 0; //Only ever 1 gate
                });
                placeables.Add(gate);
            }

            foreach (var room in rooms.Skip(skipRooms).Select(i => mapMesh.MapBuilder.Rooms[i]))
            {
                PopulateRoom(room, treasureStack, treasureChests);
            }

            //This really should not be able to happen, but track it anyway, if you had philip a key and only 2 rooms this would happen
            if (treasureChests.Count == 0 && treasureStack.Count > 0)
            {
                logger.LogWarning("No treasure chests. All loot for this zone will be converted to stolen treasure.");
                //The treasure stack is not cleared here and passes its items along to become stolen
            }
            else
            {
                //Drop any remaining treasure in the chests that were placed
                var dropIndex = 0;
                var chestCount = treasureChests.Count;
                foreach (var remainingTreasure in treasureStack)
                {
                    var placeable = treasureChests[dropIndex % chestCount];
                    placeable.AddTreasure(remainingTreasure);
                    ++dropIndex;
                }
                treasureStack.Clear(); //Clear the stack since we visited everything in the foreach
            }
        }

        private void PopulateRoom(Rectangle room, Stack<ITreasure> treasureStack, List<TreasureTrigger> treasureChests)
        {
            var point = new Point(room.Left + room.Width / 2, room.Top + room.Height / 2);

            //Special case for first zone, a bit hacky, but the computation should work out the same as the start point
            var mapLoc = mapMesh.PointToVector(point.x, point.y);
            if (goPrevious || mapLoc != startPointLocal)
            {
                //This ensures we place at least 1 chest before placing any rest areas
                if ((placeFirstChest || !placeRestArea) && treasureStack.Count > 0)
                {
                    placeFirstChest = false;
                    var treasureTrigger = objectResolver.Resolve<TreasureTrigger, TreasureTrigger.Description>(o =>
                    {
                        o.InstanceId = treasureIndex++;
                        o.ZoneIndex = index;
                        o.MapOffset = mapLoc;
                        o.Translation = currentPosition + o.MapOffset;
                        var treasure = biome.Treasure;
                        o.Sprite = treasure.Asset.CreateSprite();
                        o.SpriteMaterial = treasure.Asset.CreateMaterial();
                        o.Treasure = treasureStack.Pop();
                    });
                    this.placeables.Add(treasureTrigger);
                    treasureChests.Add(treasureTrigger);
                }
                else if (placeRestArea)
                {
                    placeRestArea = false;
                    var restArea = objectResolver.Resolve<RestArea, RestArea.Description>(o =>
                    {
                        o.InstanceId = restIndex++;
                        o.ZoneIndex = index;
                        o.MapOffset = mapLoc;
                        o.Translation = currentPosition + o.MapOffset;
                        var asset = biome.RestAsset;
                        o.Sprite = asset.CreateSprite();
                        o.SpriteMaterial = asset.CreateMaterial();
                    });
                    this.placeables.Add(restArea);
                }
            }
        }

        private void PlaceKey(Point point)
        {
            var mapLoc = mapMesh.PointToVector(point.x, point.y);
            var key = objectResolver.Resolve<Key, Key.Description>(o =>
            {
                o.InstanceId = 0;
                o.ZoneIndex = index;
                o.MapOffset = mapLoc;
                o.Translation = currentPosition + o.MapOffset;
                var keyAsset = biome.KeyAsset;
                o.Sprite = keyAsset.CreateSprite();
                o.SpriteMaterial = keyAsset.CreateMaterial();
            });
            this.placeables.Add(key);
            placeKey = false;
        }

        private void PlaceKeySafety(FIRandom enemyRandom, HashSet<int> usedCorridors)
        {
            //if we got here without placing the key, place it in a corridor, the player removes its physics
            if (placeKey)
            {
                var corridorTry = 0;
                var corridorCount = mapMesh.MapBuilder.Corridors.Count;
                var corridorIndex = enemyRandom.Next(0, corridorCount);
                while (usedCorridors.Contains(corridorIndex))
                {
                    if (++corridorTry > 50)
                    {
                        //If we generate too many bad random numbers, just get the first index we can from the list
                        for (corridorIndex = 0; corridorIndex < corridorCount && usedCorridors.Contains(corridorIndex); ++corridorIndex) { }
                        if (corridorIndex >= corridorCount)
                        {
                            throw new InvalidOperationException("This should not happen, but ran out of corridors trying to place a key. This is guarded in the constructor.");
                        }
                    }
                    else
                    {
                        corridorIndex = enemyRandom.Next(0, corridorCount);
                    }
                }
                usedCorridors.Add(corridorIndex);
                var point = mapMesh.MapBuilder.Corridors[corridorIndex];
                PlaceKey(point);
            }
        }

        private void AddStolenTreasure(Description description, FIRandom enemyRandom, List<BattleTrigger> battleTriggers, BattleTrigger bossBattleTrigger, Stack<ITreasure> treasureStack)
        {
            var stealTreasure = description.StealTreasure ?? Enumerable.Empty<ITreasure>();
            var bossStealTreasure = description.BossStealTreasure ?? Enumerable.Empty<ITreasure>();
            var uniqueStealTreasure = description.UniqueStealTreasure ?? Enumerable.Empty<ITreasure>();
            var bossUniqueStealTreasure = description.BossUniqueStealTreasure ?? Enumerable.Empty<ITreasure>();
            var battleTriggerDistributor = new EnumerableDistributor<BattleTrigger>(battleTriggers);

            if (battleTriggers.Count > 0)
            {
                foreach (var treasure in stealTreasure)
                {
                    battleTriggerDistributor.GetNext(enemyRandom)
                        .AddStealTreasure(treasure);
                }

                foreach (var treasure in uniqueStealTreasure)
                {
                    battleTriggerDistributor.GetNext(enemyRandom)
                        .AddUniqueStealTreasure(treasure);
                }
            }
            else
            {
                logger.LogWarning($"No battle triggers, cannot place stolen treasure.");
            }

            if (bossBattleTrigger != null)
            {
                foreach (var treasure in bossStealTreasure)
                {
                    bossBattleTrigger.AddStealTreasure(treasure);
                }
                foreach (var treasure in bossUniqueStealTreasure)
                {
                    bossBattleTrigger.AddUniqueStealTreasure(treasure);
                }
            }
            else if (battleTriggers.Count > 0)
            {
                foreach (var treasure in bossStealTreasure)
                {
                    battleTriggerDistributor.GetNext(enemyRandom)
                        .AddStealTreasure(treasure);
                }
                foreach (var treasure in bossUniqueStealTreasure)
                {
                    battleTriggerDistributor.GetNext(enemyRandom)
                        .AddUniqueStealTreasure(treasure);
                }
            }
            else
            {
                logger.LogWarning($"No battle triggers, cannot place boss stolen treasure.");
            }

            if (battleTriggers.Count > 0)
            {
                //Any extra treasures from the zone are added as unique steal treasures
                //This is pretty unlikely to happen
                foreach (var treasure in treasureStack)
                {
                    battleTriggerDistributor.GetNext(enemyRandom)
                        .AddUniqueStealTreasure(treasure);
                }
                treasureStack.Clear(); //Visited everything, clear stack
            }
            else
            {
                logger.LogWarning($"No battle triggers, cannot place overflow chest treasure.");
            }
        }

        private void CreateBackgroundItems(FIRandom bgItemsRandom, IBiome biome)
        {
            //var hitWalkablePath = new bool[mapMesh.MapBuilder.Map_Size.Width];
            for(var x = 0; x < mapMesh.MapBuilder.Map_Size.Width; ++x)
            {
                for (var y = mapMesh.MapBuilder.Map_Size.Height - 1; y > -1; --y)
                {
                    if (mapMesh.MapBuilder.map[x, y] == csMapbuilder.EmptyCell)
                    {
                        BiomeBackgroundItem add = null;
                        var roll = bgItemsRandom.Next(0, biome.MaxBackgroundItemRoll);
                        foreach(var item in biome.BackgroundItems)
                        {
                            if(roll < item.Chance)
                            {
                                add = item;
                                break;
                            }
                        }

                        //if (!hitWalkablePath[x])
                        //{
                        //    add = true;
                        //}
                        if (add != null)
                        {
                            var mapLoc = mapMesh.PointToVector(x, y);
                            var bgItem = objectResolver.Resolve<BackgroundItem, BackgroundItem.Description>(o =>
                            {
                                o.MapOffset = mapLoc;
                                o.Translation = currentPosition + o.MapOffset;
                                var keyAsset = add.Asset;
                                o.Sprite = keyAsset.CreateSprite();
                                o.SpriteMaterial = keyAsset.CreateMaterial();
                            });
                            this.placeables.Add(bgItem);
                        }
                    }
                    else
                    {
                        //hitWalkablePath[x] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Remove physics shapes from scene. Should wait until the zone generation is complete first.
        /// </summary>
        public void DestroyPhysics()
        {
            if (!physicsActive)
            {
                //Do nothing if physics aren't active.
                return;
            }
            physicsActive = false;

            foreach (var placeable in placeables)
            {
                placeable.DestroyPhysics();
            }

            this.previousZoneConnector?.RequestDestruction();
            this.nextZoneConnector?.RequestDestruction();

            this.previousZoneConnector = null;
            this.nextZoneConnector = null;

            var statics = bepuScene.Simulation.Statics;
            foreach (var staticHandle in staticHandles)
            {
                statics.Remove(staticHandle);
            }
            bepuScene.Simulation.Shapes.Remove(boundaryCubeShapeIndex);
            bepuScene.Simulation.Shapes.Remove(floorCubeShapeIndex);
            staticHandles.Clear();
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            floorBlasInstanceData.vertexOffset = mapMesh.FloorMesh.Instance.VertexOffset;
            floorBlasInstanceData.indexOffset = mapMesh.FloorMesh.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &floorBlasInstanceData)
            {
                floorShader.BindSbt(floorInstanceData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
            }
        }
    }
}
