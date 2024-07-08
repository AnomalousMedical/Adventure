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
using BepuUtilities.Collections;

namespace Adventure
{
    class Zone : IDisposable
    {
        public enum Alignment
        {
            WestEast,
            EastWest,
            NorthSouth,
            SouthNorth,
        }

        public class Description
        {
            /// <summary>
            /// Set this task to wait for it before doing main thread work.
            /// </summary>
            public Task MainThreadSyncTask { get; set; }

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

            public bool MakeBoss { get; set; }

            public bool MakeGate { get; set; }

            public bool MakeTorch { get; set; }

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

            public PlotItems? PlotItem { get; set; }

            public PlotItems? HelpBookPlotItem { get; set; }

            public IEnumerable<PartyMember> PartyMembers { get; set; }

            public int Area { get; set; }
            public int PadTop { get; set; } = 75;
            public int PadBottom { get; set; } = 75;
            public int PadLeft { get; set; } = 35;
            public int PadRight { get; set; } = 35;

            public Alignment Alignment { get; set; } = Alignment.WestEast;

            public bool IsFinalZone { get; set; }
        }

        private readonly RTInstances<ZoneScene> rtInstances;
        private readonly RayTracingRenderer renderer;
        private readonly Persistence persistence;
        private readonly NoiseTextureManager noiseTextureManager;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
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
        private StaticHandle floorStaticHandle;
        private TypedIndex boundaryCubeShapeIndex;
        private TypedIndex floorShapeIndex;
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
        private bool makeBoss;
        private bool makeGate;
        private bool makeTorch;
        private int enemyLevel;
        private int maxMainCorridorBattles;
        private IEnumerable<ITreasure> treasure;
        private IEnumerable<PartyMember> partyMembers;
        private PlotItems? plotItem;
        private PlotItems? helpBookPlotItem;
        private LootDropTrigger lootDropTrigger;
        private ushort startRoomIndex = ushort.MaxValue;
        private bool isFinalZone;

        private Task zoneGenerationTask;
        private TaskCompletionSource zoneFullyLoadedTask = new TaskCompletionSource();
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

        public int Area { get; init; }

        public Size2 Size { get; private set; }

        private Alignment alignment;

        public Alignment ZoneAlignment => alignment;

        public static Alignment GetEndAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.SouthNorth:
                    return Alignment.NorthSouth;

                case Alignment.NorthSouth:
                    return Alignment.SouthNorth;

                case Alignment.WestEast:
                    return Alignment.EastWest;

                default:
                case Alignment.EastWest:
                    return Alignment.WestEast;
            }
        }

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
            TerrainNoise terrainNoise,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier
        )
        {
            this.isFinalZone = description.IsFinalZone;
            this.plotItem = description.PlotItem;
            this.helpBookPlotItem = description.HelpBookPlotItem;
            this.StartEnd = description.StartEnd;
            this.maxMainCorridorBattles = description.MaxMainCorridorBattles > 0 ? description.MaxMainCorridorBattles : throw new InvalidOperationException("You must have a max main corridor fight count of at least 1.");
            this.enemyLevel = description.EnemyLevel;
            this.index = description.Index;
            this.enemySeed = description.EnemySeed;
            this.makeRestArea = description.MakeRest;
            this.Area = description.Area;
            this.makeBoss = description.MakeBoss;
            this.makeGate = description.MakeGate;
            this.makeTorch = description.MakeTorch;
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
            this.collidableIdentifier = collidableIdentifier;
            this.goPrevious = description.GoPrevious;
            this.alignment = description.Alignment;
            this.biome = description.Biome;
            this.treasure = description.Treasure ?? Enumerable.Empty<ITreasure>();
            this.partyMembers = description.PartyMembers ?? Enumerable.Empty<PartyMember>();
            this.Size = new Size2(description.MapUnitX * (description.Width + description.PadLeft + description.PadRight),
                                  description.MapUnitZ * (description.Height + description.PadTop + description.PadBottom));

            //Set current position and shift if requested
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

                var floorTextureDesc = new CCOTextureBindingDescription(biome.FloorTexture, Reflective: biome.ReflectFloor);
                var floorTextureDesc2 = new CCOTextureBindingDescription(biome.FloorTexture2 ?? biome.FloorTexture, Reflective: biome.ReflectFloor);
                var wallTextureDesc = new CCOTextureBindingDescription(biome.WallTexture, Reflective: biome.ReflectWall);
                var wallTextureDesc2 = new CCOTextureBindingDescription(biome.WallTexture2 ?? biome.WallTexture, Reflective: biome.ReflectWall);

                var floorTextureTask = textureManager.Checkout(floorTextureDesc);
                var floorTexture2Task = textureManager.Checkout(floorTextureDesc2);
                var wallTextureTask = textureManager.Checkout(wallTextureDesc);
                var wallTexture2Task = textureManager.Checkout(wallTextureDesc2);

                var noise = biome.CreateNoise?.Invoke(description.LevelSeed) ?? terrainNoise.CreateBlendTerrainNoise(description.LevelSeed);

                var noiseTask = noiseTextureManager.GenerateTexture(noise, 4096, 4096);

                Point startPoint = new Point();
                Point endPoint = new Point();

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

                    switch (alignment)
                    {
                        case Alignment.EastWest:
                            mapBuilder.FindEastConnector();
                            mapBuilder.FindWestConnector();
                            break;
                        case Alignment.WestEast:
                            mapBuilder.FindEastConnector();
                            if (description.GoPrevious)
                            {
                                mapBuilder.FindWestConnector();
                            }
                            break;
                        case Alignment.SouthNorth:
                            mapBuilder.FindSouthConnector();
                            mapBuilder.FindNorthConnector();
                            break;
                        case Alignment.NorthSouth:
                            mapBuilder.FindSouthConnector();
                            mapBuilder.FindNorthConnector();
                            break;
                    }

                    mapBuilder.AddPadding(description.PadTop, description.PadBottom, description.PadLeft, description.PadRight);

                    if (mapBuilder.EastConnector.HasValue)
                    {
                        mapBuilder.BuildEastConnector();
                    }

                    if (mapBuilder.WestConnector.HasValue)
                    {
                        mapBuilder.BuildWestConnector();
                    }

                    if (mapBuilder.NorthConnector.HasValue)
                    {
                        mapBuilder.BuildNorthConnector();
                    }

                    if (mapBuilder.SouthConnector.HasValue)
                    {
                        mapBuilder.BuildSouthConnector();
                    }

                    switch (alignment)
                    {
                        case Alignment.EastWest:
                            startPoint = mapBuilder.EastConnector.Value;
                            endPoint = mapBuilder.WestConnector.Value;
                            break;
                        case Alignment.WestEast:
                            if (description.GoPrevious)
                            {
                                startPoint = mapBuilder.WestConnector.Value;
                            }
                            else
                            {
                                Rectangle startRoom = new Rectangle(int.MaxValue, 0, 0, 0);
                                var numRooms = mapBuilder.Rooms.Count;
                                for (ushort i = 0; i < numRooms; i++)
                                {
                                    var room = mapBuilder.Rooms[i];
                                    if (room.Left < startRoom.Left)
                                    {
                                        startRoom = room;
                                        this.startRoomIndex = i;
                                    }
                                }

                                startPoint = new Point
                                (
                                    startRoom.Left + startRoom.Width / 2,
                                    startRoom.Top + startRoom.Height / 2
                                );
                            }
                            endPoint = mapBuilder.EastConnector.Value;
                            break;
                        case Alignment.SouthNorth:
                            startPoint = mapBuilder.SouthConnector.Value;
                            endPoint = mapBuilder.NorthConnector.Value;
                            break;
                        case Alignment.NorthSouth:
                            startPoint = mapBuilder.NorthConnector.Value;
                            endPoint = mapBuilder.SouthConnector.Value;
                            break;
                    }

                    mapMesh = new MapMesh(mapBuilder, floorMesh, mapUnitX: description.MapUnitX, mapUnitY: description.MapUnitY, mapUnitZ: description.MapUnitZ, corridorSlopeMultiple: description.CorridorSlopeMultiple);

                    startPointLocal = mapMesh.PointToVector(startPoint.x, startPoint.y);
                    endPointLocal = mapMesh.PointToVector(endPoint.x, endPoint.y);

                    var triangles = new QuickList<Triangle>(mapMesh.CollisionMeshPositions.Count() * 4, bepuScene.BufferPool);
                    foreach (var centerPt in mapMesh.CollisionMeshPositions)
                    {
                        //Clockwise order for camera collision
                        triangles.AllocateUnsafely() = new Triangle
                        (
                            centerPt.TopLeft.ToSystemNumerics(),
                            centerPt.TopRight.ToSystemNumerics(),
                            centerPt.BottomRight.ToSystemNumerics()
                        );

                        triangles.AllocateUnsafely() = new Triangle
                        (
                            centerPt.BottomRight.ToSystemNumerics(),
                            centerPt.BottomLeft.ToSystemNumerics(),
                            centerPt.TopLeft.ToSystemNumerics()
                        );

                        //Counter clockwise for the actual physics objects
                        triangles.AllocateUnsafely() = new Triangle
                        (
                           centerPt.TopRight.ToSystemNumerics(),
                           centerPt.TopLeft.ToSystemNumerics(),
                           centerPt.BottomLeft.ToSystemNumerics()
                        );

                        triangles.AllocateUnsafely() = new Triangle
                        (
                           centerPt.BottomLeft.ToSystemNumerics(),
                           centerPt.BottomRight.ToSystemNumerics(),
                           centerPt.TopRight.ToSystemNumerics()
                        );
                    }

                    var meshShape = new Mesh(triangles, new System.Numerics.Vector3(1.0f, 1.0f, 1.0f), bepuScene.BufferPool);
                    floorShapeIndex = bepuScene.Simulation.Shapes.Add(meshShape);

                    sw.Stop();
                    logger.LogInformation($"Generated zone {description.Index} seed {description.LevelSeed} in {sw.ElapsedMilliseconds} ms.");
                });

                await zoneGenerationTask; //Need the zone before kicking off the calls to End() below.

                if(description.MainThreadSyncTask != null)
                {
                    await description.MainThreadSyncTask;
                    description.MainThreadSyncTask = null; //Null this out so we don't hold onto any closures
                }

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
                floorBlasInstanceData.raycastSmallOffset = 0.1f;
                rtInstances.AddTlasBuild(floorInstanceData);

                ResetPlacementData();
                var enemyRandom = new FIRandom(enemySeed);
                var usedCorridors = new HashSet<int>();
                var noBgSquares = new bool[mapMesh.MapBuilder.map.GetLength(0), mapMesh.MapBuilder.map.GetLength(1)];

                var battleTriggers = new List<BattleTrigger>();
                SetupCorridors(enemyRandom, usedCorridors, battleTriggers);
                SetupRooms(enemyRandom, out var bossBattleTrigger, out var treasureStack, noBgSquares);
                PlaceKeySafety(enemyRandom, usedCorridors);
                PlaceSignpost(description, startPoint, endPoint);
                ReserveBgSquares(noBgSquares, startPoint, 7);
                ReserveBgSquares(noBgSquares, new Point(startPoint.x + 1, startPoint.y), 7);
                ReserveBgSquares(noBgSquares, new Point(startPoint.x - 1, startPoint.y), 7);
                ReserveBgSquares(noBgSquares, endPoint, 7);
                ReserveBgSquares(noBgSquares, new Point(endPoint.x + 1, endPoint.y), 7);
                ReserveBgSquares(noBgSquares, new Point(endPoint.x - 1, endPoint.y), 7);
                CreateHelpBook();

                if (biome.BackgroundItems != null)
                {
                    CreateBackgroundItems(enemyRandom, biome, noBgSquares);
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

                zoneFullyLoadedTask.SetResult();
            });
        }

        private void PlaceSignpost(Description description, Point startPoint, Point endPoint)
        {
            Vector3 startSignpostOffset;
            Vector3 endSignpostOffset;
            var zOffset = mapMesh.MapUnitZ / -2.0f;
            switch (alignment)
            {
                case Alignment.EastWest:
                case Alignment.WestEast:
                default:
                    var offset = new Vector3(0f, 0f, zOffset);
                    startSignpostOffset = mapMesh.PointToVector(startPoint.x, startPoint.y + 1) + offset;
                    endSignpostOffset = mapMesh.PointToVector(endPoint.x, endPoint.y + 1) + offset;
                    break;

                case Alignment.NorthSouth:
                    startSignpostOffset = mapMesh.PointToVector(startPoint.x, startPoint.y + 1) + new Vector3(mapMesh.MapUnitX / -2.0f, 0f, zOffset);
                    endSignpostOffset = mapMesh.PointToVector(endPoint.x, endPoint.y) + new Vector3(mapMesh.MapUnitX / 2.0f, 0f, zOffset);
                    break;
                case Alignment.SouthNorth:
                    startSignpostOffset = mapMesh.PointToVector(startPoint.x, startPoint.y) + new Vector3(mapMesh.MapUnitX / -2.0f, 0f, zOffset);
                    endSignpostOffset = mapMesh.PointToVector(endPoint.x, endPoint.y + 1) + new Vector3(mapMesh.MapUnitX / 2.0f, 0f, zOffset);
                    break;
            }

            var signpostAsset = new Signpost();
            if (description.GoPrevious)
            {
                var bgItem = objectResolver.Resolve<BackgroundItem, BackgroundItem.Description>(o =>
                {
                    o.MapOffset = startSignpostOffset;
                    o.Translation = currentPosition + o.MapOffset;
                    o.Sprite = signpostAsset.CreateSprite();
                    o.SpriteMaterial = signpostAsset.CreateMaterial();
                    o.Scale = new Vector3(1.5f, 1.5f, 1.0f);
                });
                this.placeables.Add(bgItem);
            }

            {
                var bgItem = objectResolver.Resolve<BackgroundItem, BackgroundItem.Description>(o =>
                {
                    o.MapOffset = endSignpostOffset;
                    o.Translation = currentPosition + o.MapOffset;
                    o.Sprite = signpostAsset.CreateSprite();
                    o.SpriteMaterial = signpostAsset.CreateMaterial();
                    o.Scale = new Vector3(1.5f, 1.5f, 1.0f);
                });
                this.placeables.Add(bgItem);
            }
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

            //This is made in the constructor, so remove it here
            bepuScene.Simulation.Shapes.Remove(floorShapeIndex);
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

        public async Task WaitForFullLoad()
        {
            if (zoneFullyLoadedTask != null)
            {
                await zoneFullyLoadedTask.Task;
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

            var boundaryOrientation = System.Numerics.Quaternion.Identity;

            //Floor
            {
                floorStaticHandle = bepuScene.Simulation.Statics.Add(
                       new StaticDescription(
                           currentPosition.ToSystemNumerics(),
                           System.Numerics.Quaternion.Identity,
                           floorShapeIndex));

                collidableIdentifier.AddIdentifier(new CollidableReference(floorStaticHandle), this);
            }

            //Boundary cubes
            foreach (var boundary in mapMesh.BoundaryCubeCenterPoints)
            {
                var staticHandle = bepuScene.Simulation.Statics.Add(
                    new StaticDescription(
                        (boundary + currentPosition).ToSystemNumerics(),
                        boundaryOrientation,
                        boundaryCubeShapeIndex));

                staticHandles.Add(staticHandle);
            }

            //Zone connectors
            Vector3 nextZoneConnectorOffset;
            Vector3 previousZoneConnectorOffset;
            switch (alignment)
            {
                default:
                case Alignment.WestEast:
                    nextZoneConnectorOffset = new Vector3(1f, 0f, 0f);
                    previousZoneConnectorOffset = new Vector3(-1f, 0f, 0f);
                    break;
                case Alignment.EastWest:
                    nextZoneConnectorOffset = new Vector3(-1f, 0f, 0f);
                    previousZoneConnectorOffset = new Vector3(1f, 0f, 0f);
                    break;
                case Alignment.SouthNorth:
                    nextZoneConnectorOffset = new Vector3(0f, 0f, 1f);
                    previousZoneConnectorOffset = new Vector3(0f, 0f, -1f);
                    break;
                case Alignment.NorthSouth:
                    nextZoneConnectorOffset = new Vector3(0f, 0f, -1f);
                    previousZoneConnectorOffset = new Vector3(0f, 0f, 1f);
                    break;
            }

            if (goPrevious)
            {
                this.previousZoneConnector = objectResolver.Resolve<ZoneConnector, ZoneConnector.Description>(o =>
                {
                    o.Scale = new Vector3(mapUnits.x, 50f, mapUnits.z);
                    o.Translation = StartPoint + mapUnits.x * 2f * previousZoneConnectorOffset;
                });
            }

            this.nextZoneConnector = objectResolver.Resolve<ZoneConnector, ZoneConnector.Description>(o =>
            {
                o.Scale = new Vector3(mapUnits.x, 50f, mapUnits.z);
                o.Translation = EndPoint + mapUnits.x * 2f * nextZoneConnectorOffset;
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
        private bool placeBoss;
        private bool placeGate;
        private bool placeKey;
        private bool placeTorch;
        private bool placeFirstChest;
        private Point? helpBookPoint;

        private void ResetPlacementData()
        {
            restIndex = 0;
            treasureIndex = 0;
            enemyIndex = 0;
            placeRestArea = this.makeRestArea;
            placeBoss = this.makeBoss;
            placeKey = placeGate = makeGate;
            placeTorch = this.makeTorch;
            placeFirstChest = true;
            helpBookPoint = null;
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
            var numEnemies = maxFights;
            if (minFights < maxFights)
            {
                numEnemies = enemyRandom.Next(minFights, maxFights);
            }
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

        private void SetupRooms(FIRandom enemyRandom, out BattleTrigger bossBattleTrigger, out Stack<ITreasure> treasureStack, bool[,] noBgSquares)
        {
            //The order of everything in this function is important to ensure all treasure can be distributed

            bossBattleTrigger = null;
            var treasureChests = new List<TreasureTrigger>();
            treasureStack = new Stack<ITreasure>(this.treasure.Reverse());

            var rooms = mapMesh.MapBuilder.GetDesiredRooms().ToList();
            var skipRooms = 0;

            int GetRoom()
            {
                int roomIndex;
                do
                {
                    if (skipRooms >= rooms.Count)
                    {
                        return rooms[rooms.Count - 1];
                    }
                    roomIndex = rooms[skipRooms];
                    skipRooms++;
                } while (roomIndex == startRoomIndex);

                return roomIndex;
            }

            var partyMemberIndex = 0;
            foreach (var partyMember in partyMembers)
            {
                var partyMemberRoom = startRoomIndex;
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
                ReserveBgSquares(noBgSquares, point);

                var partyMemberObject = objectResolver.Resolve<PartyMemberTrigger, PartyMemberTrigger.Description>(o =>
                {
                    o.ZoneIndex = index;
                    o.InstanceId = partyMemberIndex++;
                    o.MapOffset = mapLoc;
                    o.Translation = currentPosition + o.MapOffset;
                    o.Sprite = partyMember.CharacterData.PlayerSprite;
                    o.PartyMember = partyMember;
                });
                this.placeables.Add(partyMemberObject);
            }

            //Reset skip rooms, the players go in the corners so these other things can go in too
            skipRooms = 0;

            var shareKeyAndTorchRoom = rooms.Count < 3;
            int? keyRoomIndex = null;
            if (placeKey)
            {
                keyRoomIndex = GetRoom();
                var room = mapMesh.MapBuilder.Rooms[keyRoomIndex.Value];
                var point = new Point(room.Left + room.Width / 2, room.Top + room.Height / 2);
                if (placeTorch && shareKeyAndTorchRoom)
                {
                    //Move the key to the left side if the torch and key are in the same room.
                    point.x = room.Left;
                }
                PlaceKey(point);
                ReserveBgSquares(noBgSquares, point);
            }

            if (placeTorch)
            {
                placeTorch = false;
                int roomIndex;
                if (shareKeyAndTorchRoom)
                {
                    roomIndex = keyRoomIndex ?? GetRoom();
                }
                //Otherwise get a unique room for the torch.
                else
                {
                    roomIndex = GetRoom();
                }
                var room = mapMesh.MapBuilder.Rooms[roomIndex];
                var point = new Point(room.Left + room.Width / 2, room.Top + room.Height / 2);
                var mapLoc = mapMesh.PointToVector(point.x, point.y);
                var torch = objectResolver.Resolve<Torch, Torch.Description>(o =>
                {
                    o.InstanceId = 0;
                    o.ZoneIndex = index;
                    o.MapOffset = mapLoc;
                    o.Translation = currentPosition + o.MapOffset;
                    var asset = biome.TorchAsset;
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                });
                this.placeables.Add(torch);
                ReserveBgSquares(noBgSquares, point);
            }

            Vector3 endZoneItemOffset;
            switch (alignment)
            {
                default:
                case Alignment.WestEast:
                    endZoneItemOffset = new Vector3(1f, 0f, 0f);
                    break;
                case Alignment.EastWest:
                    endZoneItemOffset = new Vector3(-1f, 0f, 0f);
                    break;
                case Alignment.SouthNorth:
                    endZoneItemOffset = new Vector3(0f, 0f, 1f);
                    break;
                case Alignment.NorthSouth:
                    endZoneItemOffset = new Vector3(0f, 0f, -1f);
                    break;
            }

            //The plot item goes in the exit corridor, not the room
            if (this.plotItem != null)
            {
                var plotItemPlaceable = objectResolver.Resolve<PlotItemPlaceable, PlotItemPlaceable.Description>(o =>
                {
                    o.MapOffset = endPointLocal;
                    o.Translation = currentPosition + o.MapOffset + 2.25f * endZoneItemOffset;
                    o.Scale = new Vector3(2f, 2f, 1f);
                    o.PlotItem = this.plotItem.Value;
                });
                placeables.Add(plotItemPlaceable);
            }

            //The boss goes in the exit corridor, not the room
            if (placeBoss)
            {
                bossBattleTrigger = objectResolver.Resolve<BattleTrigger, BattleTrigger.Description>(o =>
                {
                    o.MapOffset = endPointLocal;
                    o.Translation = currentPosition + o.MapOffset + 1.25f * endZoneItemOffset;
                    o.TriggerEnemy = biome.BossEnemy;
                    o.Zone = index;
                    o.Area = Area;
                    o.Index = 0; //Only ever 1 boss
                    o.EnemyLevel = enemyLevel;
                    o.BattleSeed = enemyRandom.Next(int.MinValue, int.MaxValue);
                    o.IsBoss = true;
                    o.IsFinalBoss = isFinalZone;
                    o.Scale = new Vector3(2f, 2f, 1f);
                });
                placeables.Add(bossBattleTrigger);
            }

            //The gate goes in the exit corridor, not the room
            if (placeGate)
            {
                var gate = objectResolver.Resolve<Gate, Gate.Description>(o =>
                {
                    o.MapOffset = endPointLocal;
                    o.Translation = currentPosition + o.MapOffset;
                    var gateAsset = biome.GateAsset;
                    o.Sprite = gateAsset.CreateSprite();
                    o.SpriteMaterial = gateAsset.CreateMaterial();
                    o.Zone = index;
                    o.ZoneAlignment = alignment;
                    o.InstanceId = 0; //Only ever 1 gate
                });
                placeables.Add(gate);
            }

            foreach (var room in rooms.Skip(skipRooms).Select(i => mapMesh.MapBuilder.Rooms[i]))
            {
                PopulateRoom(room, treasureStack, treasureChests, noBgSquares);
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

        private static void ReserveBgSquares(bool[,] noBgSquares, Point point, int numSquares = 4)
        {
            int max = 1 + numSquares;
            int noY;
            for (int i = 1; (noY = point.y - i) > 0 && i < max; ++i)
            {
                noBgSquares[point.x, noY] = true;
            }
        }

        private void PopulateRoom(Rectangle room, Stack<ITreasure> treasureStack, List<TreasureTrigger> treasureChests, bool[,] noBgSquares)
        {
            var point = new Point(room.Left + room.Width / 2, room.Top + room.Height / 2);
            ReserveBgSquares(noBgSquares, point);

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
                        o.Treasure = treasureStack.Pop();
                        var treasureAsset = biome.Treasure.GetTreasureAsset(o.Treasure.TreasureType);
                        o.Sprite = treasureAsset.CreateSprite();
                        o.SpriteMaterial = treasureAsset.CreateMaterial();
                    });
                    this.placeables.Add(treasureTrigger);
                    treasureChests.Add(treasureTrigger);
                    SetHelpBookPoint(new Point(point.x + 1, point.y));
                }
                else if (goPrevious && placeRestArea) //Only place this way if you can go to a previous level, otherwise it goes in the start area
                {
                    placeRestArea = false;
                    CreateRestArea(mapLoc);
                }
            }
            else if(!goPrevious && mapLoc == startPointLocal && placeRestArea)
            {
                //This is the start of the game, so place a rest area and the help book
                placeRestArea = false;
                CreateRestArea(mapLoc);
                SetHelpBookPoint(new Point(point.x + 1, point.y), true);
            }
        }

        private void CreateRestArea(Vector3 mapLoc)
        {
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

        private void SetHelpBookPoint(in Point point, bool force = false)
        {
            if(helpBookPoint == null || force)
            {
                helpBookPoint = point;
            }
        }

        private void CreateHelpBook()
        {
            if (helpBookPlotItem != null && helpBookPoint != null)
            {
                var mapLoc = mapMesh.PointToVector(helpBookPoint.Value.x, helpBookPoint.Value.y);
                var helpBook = objectResolver.Resolve<HelpBook, HelpBook.Description>(o =>
                {
                    o.MapOffset = mapLoc;
                    o.Translation = currentPosition + o.MapOffset;
                    o.PlotItem = helpBookPlotItem.Value;
                });
                this.placeables.Add(helpBook);
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
                o.Scale = new Vector3(1.5f, 1.5f, 1f);
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

        private void CreateBackgroundItems(FIRandom bgItemsRandom, IBiome biome, bool[,] noBgSquares)
        {
            bool mustBeEven = false;
            for (var x = 0; x < mapMesh.MapBuilder.Map_Size.Width; ++x)
            {
                mustBeEven = !mustBeEven;
                for (var y = mapMesh.MapBuilder.Map_Size.Height - 1; y > -1; --y)
                {
                    if (mapMesh.MapBuilder.map[x, y] == csMapbuilder.EmptyCell && !noBgSquares[x,y])
                    {
                        BiomeBackgroundItem add = null;
                        var roll = bgItemsRandom.Next(0, biome.MaxBackgroundItemRoll);
                        foreach (var item in biome.BackgroundItems)
                        {
                            if (roll < item.Chance)
                            {
                                add = item;
                                break;
                            }
                        }

                        if (add != null)
                        {
                            var bgItem = objectResolver.Resolve<BackgroundItem, BackgroundItem.Description>(o =>
                            {
                                var mapUnitX = mapMesh.MapUnitX * add.XPlacementRange;
                                var halfUnitX = mapUnitX * 0.5f;
                                var mapUnitZ = mapMesh.MapUnitZ * add.ZPlacementRange;
                                var halfUnitZ = mapUnitZ * 0.5f;

                                var scale = Vector3.ScaleIdentity * (bgItemsRandom.NextSingle() * add.ScaleRange + add.ScaleMin);
                                var mapLoc = mapMesh.PointToVector(x, y);
                                var keyAsset = add.Asset;
                                var sprite = keyAsset.CreateSprite();
                                mapLoc.x += bgItemsRandom.NextSingle() * mapUnitX - halfUnitX;
                                if (keyAsset.GroundAttachmentChannel.HasValue)
                                {
                                    var groundOffset = sprite.GetCurrentFrame().Attachments[keyAsset.GroundAttachmentChannel.Value].translate;
                                    mapLoc += groundOffset * scale * sprite.BaseScale;
                                }
                                var zOffsetBucket = bgItemsRandom.Next(9);
                                if (mustBeEven)
                                {
                                    if (zOffsetBucket % 2 != 0)
                                    {
                                        zOffsetBucket += 1;
                                    }
                                }
                                else //Odd
                                {
                                    if (zOffsetBucket % 2 != 1)
                                    {
                                        zOffsetBucket += 1;
                                    }
                                }
                                mapLoc.z += zOffsetBucket * 0.1f * mapUnitZ - halfUnitZ;

                                o.MapOffset = mapLoc;
                                o.Translation = currentPosition + o.MapOffset;
                                o.Sprite = sprite;
                                o.SpriteMaterial = keyAsset.CreateMaterial();
                                o.Scale = scale;
                            });
                            this.placeables.Add(bgItem);
                        }
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

            collidableIdentifier.RemoveIdentifier(new CollidableReference(floorStaticHandle));
            statics.Remove(floorStaticHandle);

            bepuScene.Simulation.Shapes.Remove(boundaryCubeShapeIndex);
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

        internal async Task<BattleTrigger> FindTrigger(int index, bool isBoss)
        {
            await zoneFullyLoadedTask.Task;
            return placeables
                .Select(i => i as BattleTrigger)
                .Where(i => i != null)
                .Where(i => i.Index == index && i.IsBoss == isBoss).First();
        }
    }
}
