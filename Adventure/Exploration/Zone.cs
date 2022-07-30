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
        }

        private readonly RTInstances<IZoneManager> rtInstances;
        private readonly RayTracingRenderer renderer;
        private readonly Persistence persistence;
        private readonly IDestructionRequest destructionRequest;
        private readonly IBepuScene bepuScene;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly ILogger<Zone> logger;
        private PrimaryHitShader floorShader;
        private CC0TextureResult floorTexture;
        private CC0TextureResult wallTexture;
        private readonly TLASBuildInstanceData floorInstanceData;
        private List<StaticHandle> staticHandles = new List<StaticHandle>();
        private TypedIndex boundaryCubeShapeIndex;
        private TypedIndex floorCubeShapeIndex;
        private MapMesh mapMesh;
        private bool physicsActive = false;
        private IObjectResolver objectResolver;
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

        private Task zoneGenerationTask;
        private Vector3 mapUnits;

        private Vector3 endPointLocal;
        private Vector3 startPointLocal;
        private Vector3 currentPosition;

        public Vector3 StartPoint => startPointLocal + currentPosition;
        public Vector3 EndPoint => endPointLocal + currentPosition;

        public Vector3 LocalStartPoint => startPointLocal;
        public Vector3 LocalEndPoint => endPointLocal;

        public int Index => index;

        public Zone
        (
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene bepuScene,
            Description description,
            ILogger<Zone> logger,
            IObjectResolverFactory objectResolverFactory,
            MeshBLAS floorMesh,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RTInstances<IZoneManager> rtInstances,
            RayTracingRenderer renderer,
            Persistence persistence
        )
        {
            this.maxMainCorridorBattles = description.MaxMainCorridorBattles > 0 ? description.MaxMainCorridorBattles : throw new InvalidOperationException("You must have a max main corridor fight count of at least 1.");
            this.enemyLevel = description.EnemyLevel;
            this.index = description.Index;
            this.enemySeed = description.EnemySeed;
            this.makeRestArea = description.MakeRest;
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
            this.goPrevious = description.GoPrevious;
            this.biome = description.Biome;
            this.treasure = description.Treasure ?? Enumerable.Empty<ITreasure>();

            this.currentPosition = description.Translation;

            this.floorInstanceData = new TLASBuildInstanceData()
            {
                InstanceName = RTId.CreateId("ZoneFloor"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, Quaternion.Identity)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                var floorTextureDesc = new CCOTextureBindingDescription(biome.FloorTexture, reflective: biome.ReflectFloor);
                var wallTextureDesc = new CCOTextureBindingDescription(biome.WallTexture, reflective: biome.ReflectWall);

                var floorTextureTask = textureManager.Checkout(floorTextureDesc);
                var wallTextureTask = textureManager.Checkout(wallTextureDesc);

                this.zoneGenerationTask = Task.Run(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var random = new Random(description.LevelSeed);
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
                        foreach (var room in mapBuilder.Rooms)
                        {
                            if (room.Left < startRoom.Left)
                            {
                                startRoom = room;
                            }
                        }

                        startX = startRoom.Left + startRoom.Width / 2;
                        startY = startRoom.Top + startRoom.Height / 2;
                    }

                    mapMesh = new MapMesh(mapBuilder, random, floorMesh, mapUnitX: description.MapUnitX, mapUnitY: description.MapUnitY, mapUnitZ: description.MapUnitZ);

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
                    wallTextureTask,
                    floorShaderSetup
                );

                this.floorShader = floorShaderSetup.Result;
                this.floorTexture = floorTextureTask.Result;
                this.wallTexture = wallTextureTask.Result;

                this.floorInstanceData.pBLAS = mapMesh.FloorMesh.Instance.BLAS.Obj;

                rtInstances.AddShaderTableBinder(Bind);
                floorBlasInstanceData = activeTextures.AddActiveTexture(floorTexture, wallTexture);
                floorBlasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, biome.ReflectFloor, false, false);
                rtInstances.AddTlasBuild(floorInstanceData);

                ResetPlacementData();
                var enemyRandom = new Random(enemySeed);
                var usedCorridors = new HashSet<int>();

                var battleTriggers = new List<BattleTrigger>();
                SetupCorridors(enemyRandom, usedCorridors, battleTriggers);
                SetupRooms(enemyRandom, out var bossBattleTrigger, out var treasureStack);
                PlaceKeySafety(enemyRandom, usedCorridors);

                if (biome.BackgroundItems != null)
                {
                    CreateBackgroundItems(enemyRandom, biome);
                }

                AddLootDrop();
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
            activeTextures.RemoveActiveTexture(floorTexture);
            textureManager.TryReturn(wallTexture);
            textureManager.TryReturn(floorTexture);
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

        public IBiome Biome => biome;

        public int EnemyLevel => enemyLevel;

        public void ResetPlaceables()
        {
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
                });
            }

            this.nextZoneConnector = objectResolver.Resolve<ZoneConnector, ZoneConnector.Description>(o =>
            {
                o.Scale = new Vector3(mapUnits.x, 50f, mapUnits.z);
                o.Translation = EndPoint + new Vector3(mapUnits.x * 2f, 0f, 0f);
                o.GoPrevious = false;
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
        private ushort philipRoom = csMapbuilder.NullCell;

        private void ResetPlacementData()
        {
            restIndex = 0;
            treasureIndex = 0;
            enemyIndex = 0;
            placeRestArea = this.makeRestArea;
            placePhilip = this.makePhilip;
            placeBoss = this.makeBoss;
            placeKey = placeGate = makeGate;
            philipRoom = csMapbuilder.NullCell;
            placeFirstChest = true;
        }

        private void AddLootDrop()
        {
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
            }
        }

        private void SetupCorridors(Random enemyRandom, HashSet<int> usedCorridors, List<BattleTrigger> battleTriggers)
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

        private void PopulateCorridor(Random enemyRandom, HashSet<int> usedCorridors, int corridorStartIndex, int currentIndex, int maxPossibleFights, List<BattleTrigger> battleTriggers)
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
                    o.Index = enemyIndex++;
                    o.EnemyLevel = enemyLevel;
                    o.BattleSeed = enemyRandom.Next(int.MinValue, int.MaxValue);
                });
                battleTriggers.Add(battleTrigger);
                placeables.Add(battleTrigger);
            }
        }

        private void SetupRooms(Random enemyRandom, out BattleTrigger bossBattleTrigger, out Stack<ITreasure> treasureStack)
        {
            //The order of everything in this function is important to ensure all treasure can be distributed

            bossBattleTrigger = null;
            var treasureChests = new List<TreasureTrigger>();
            treasureStack = new Stack<ITreasure>(this.treasure);

            //Philip gets a room always
            if (placePhilip)
            {
                philipRoom = csMapbuilder.IsRoomCell(mapMesh.MapBuilder.WestConnectorRoom) ? mapMesh.MapBuilder.WestConnectorRoom : csMapbuilder.RoomCell;
                var room = mapMesh.MapBuilder.Rooms[philipRoom - csMapbuilder.RoomCell];
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

            int keyRoom = csMapbuilder.NullCell;

            if (placeKey)
            {
                //This might not be possible, so the key will go in a corridor later if it isn't placed here

                var tries = 0;
                var triedRooms = new HashSet<int>();
                var numRooms = mapMesh.MapBuilder.Rooms.Count;
                triedRooms.Add(philipRoom);
                triedRooms.Add(mapMesh.MapBuilder.WestConnectorRoom);
                triedRooms.Add(mapMesh.MapBuilder.EastConnectorRoom);
                triedRooms.Add(mapMesh.MapBuilder.NorthConnectorRoom);
                triedRooms.Add(mapMesh.MapBuilder.SouthConnectorRoom);
                triedRooms.Add(csMapbuilder.RoomCell); //Not start room
                triedRooms.Add(csMapbuilder.RoomCell + 1); //Not end room
                var keyRoomIndex = enemyRandom.Next(0, numRooms);

                if (triedRooms.Count < numRooms)
                {
                    while (triedRooms.Contains(keyRoomIndex))
                    {
                        if (++tries > 50)
                        {
                            //If we generate too many bad random numbers, just get the first index we can from the list
                            for (keyRoomIndex = 0; keyRoomIndex < numRooms && triedRooms.Contains(keyRoomIndex); ++keyRoomIndex) { }
                        }
                        else
                        {
                            keyRoomIndex = enemyRandom.Next(0, numRooms);
                        }
                    }
                    var room = mapMesh.MapBuilder.Rooms[keyRoomIndex];
                    keyRoom = mapMesh.MapBuilder.map[room.Left, room.Top];
                    var point = new Point(room.Left + room.Width / 2, room.Top + room.Height / 2);
                    PlaceKey(point);
                }
            }

            //Since keys can't go in the connector rooms there will always be at least 1 left when this is called.
            foreach (var room in mapMesh.MapBuilder.Rooms.Where(i =>
            {
                var ri = mapMesh.MapBuilder.map[i.Left, i.Top];
                return ri != philipRoom && ri != keyRoom;
            }))
            {
                PopulateRoom(room, treasureStack, treasureChests);
            }

            //This really should not be able to happen, but track it anyway
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

        private void PlaceKeySafety(Random enemyRandom, HashSet<int> usedCorridors)
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

        private void AddStolenTreasure(Description description, Random enemyRandom, List<BattleTrigger> battleTriggers, BattleTrigger bossBattleTrigger, Stack<ITreasure> treasureStack)
        {
            var stealTreasure = description.StealTreasure ?? Enumerable.Empty<ITreasure>();
            var bossStealTreasure = description.BossStealTreasure ?? Enumerable.Empty<ITreasure>();
            var uniqueStealTreasure = description.UniqueStealTreasure ?? Enumerable.Empty<ITreasure>();
            var bossUniqueStealTreasure = description.BossUniqueStealTreasure ?? Enumerable.Empty<ITreasure>();

            if (battleTriggers.Count > 0)
            {
                foreach (var treasure in stealTreasure)
                {
                    var index = enemyRandom.Next(battleTriggers.Count);
                    battleTriggers[index].AddStealTreasure(treasure);
                }

                foreach (var treasure in uniqueStealTreasure)
                {
                    var index = enemyRandom.Next(battleTriggers.Count);
                    battleTriggers[index].AddUniqueStealTreasure(treasure);
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
                    var index = enemyRandom.Next(battleTriggers.Count);
                    battleTriggers[index].AddStealTreasure(treasure);
                }
                foreach (var treasure in bossUniqueStealTreasure)
                {
                    var index = enemyRandom.Next(battleTriggers.Count);
                    battleTriggers[index].AddUniqueStealTreasure(treasure);
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
                    var index = enemyRandom.Next(battleTriggers.Count);
                    battleTriggers[index].AddUniqueStealTreasure(treasure);
                }
                treasureStack.Clear(); //Visited everything, clear stack
            }
            else
            {
                logger.LogWarning($"No battle triggers, cannot place overflow chest treasure.");
            }
        }

        private void CreateBackgroundItems(Random bgItemsRandom, IBiome biome)
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
