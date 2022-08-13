﻿using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using DungeonGenerator;
using Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class WorldMapInstance : IDisposable
    {
        public class Description
        {
            public csIslandMaze csIslandMaze { get; set; }

            public List<IAreaBuilder> Areas { get; set; }

            public List<IntVector2> PortalLocations { get; set; }

            public IntVector2 AirshipSquare { get; set; }

            public IntVector2 AirshipPortalSquare { get; set; }

            public float MapScale { get; set; } = 1.0f;
        }

        private readonly TLASInstanceData[] floorInstanceData;
        private readonly IDestructionRequest destructionRequest;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RTInstances<IWorldMapGameState> rtInstances;
        private readonly RayTracingRenderer renderer;
        private readonly IBepuScene<IWorldMapGameState> bepuScene;
        private readonly IBiomeManager biomeManager;
        private readonly csIslandMaze map;
        private readonly IObjectResolver objectResolver;
        private PrimaryHitShader shader;
        private IslandMazeMesh mapMesh;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();
        private BlasInstanceData floorBlasInstanceData;
        private bool physicsActive = false;
        private TypedIndex boundaryCubeShapeIndex;
        private TypedIndex floorCubeShapeIndex;
        private List<StaticHandle> staticHandles = new List<StaticHandle>();
        private Vector3 currentPosition = Vector3.Zero;
        private List<IWorldMapPlaceable> placeables = new List<IWorldMapPlaceable>();
        private IntVector2[] areaLocations;
        private List<IntVector2> portalLocations = new List<IntVector2>();
        private float mapScale;
        private Vector2 mapSize;
        private Vector3[] transforms;
        private Vector3 airshipStartPoint;
        private IntVector2 airshipPortal;

        public bool PhysicsActive => physicsActive;

        public Vector3[] Transforms => transforms;

        public Vector3 AirshipStartPoint => airshipStartPoint;

        public Vector2 MapSize => mapSize;

        CC0TextureResult countrysideTexture;
        CC0TextureResult desertTexture;
        CC0TextureResult snowyTexture;
        CC0TextureResult forestTexture;
        CC0TextureResult cliffTexture;
        CC0TextureResult oceanFloorTexture;

        public WorldMapInstance
        (
            Description description,
            IScopedCoroutine coroutineRunner,
            IDestructionRequest destructionRequest,
            MeshBLAS floorMesh,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RTInstances<IWorldMapGameState> rtInstances,
            RayTracingRenderer renderer,
            IObjectResolverFactory objectResolverFactory,
            IBepuScene<IWorldMapGameState> bepuScene,
            IBiomeManager biomeManager
        )
        {
            this.mapScale = description.MapScale;
            var mapWidth = description.csIslandMaze.MapX * this.mapScale;
            var mapHeight = description.csIslandMaze.MapY * this.mapScale;
            this.mapSize = new Vector2(mapWidth, mapHeight);

            this.objectResolver = objectResolverFactory.Create();
            this.destructionRequest = destructionRequest;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.rtInstances = rtInstances;
            this.renderer = renderer;
            this.bepuScene = bepuScene;
            this.biomeManager = biomeManager;
            this.map = description.csIslandMaze;

            transforms = new[]
            {
                Vector3.Zero,
                new Vector3(0, 0, mapHeight),
                new Vector3(0, 0, -mapHeight),
                new Vector3(mapWidth, 0, 0),
                new Vector3(-mapWidth, 0, 0),
                new Vector3(mapWidth, 0, mapHeight),
                new Vector3(-mapWidth, 0, mapHeight),
                new Vector3(mapWidth, 0, -mapHeight),
                new Vector3(-mapWidth, 0, -mapHeight),
            };
            this.floorInstanceData = new TLASInstanceData[transforms.Length];
            for(var i = 0; i < floorInstanceData.Length; i++)
            {
                this.floorInstanceData[i] = new TLASInstanceData()
                {
                    InstanceName = RTId.CreateId("SceneDungeonFloor"),
                    Mask = RtStructures.OPAQUE_GEOM_MASK,
                    Transform = new InstanceMatrix(transforms[i], Quaternion.Identity)
                };
            }

            coroutineRunner.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction();
                try
                {
                    var countrysideTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Ground037_1K");
                    var desertTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Ground025_1K");
                    var snowyTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Snow006_1K");
                    var forestTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Ground042_1K");
                    var cliffTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Rock029_1K");
                    var oceanFloorTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Rock022_1K");

                    var countrysideTextureTask = textureManager.Checkout(countrysideTextureDesc);
                    var desertTextureTask = textureManager.Checkout(desertTextureDesc);
                    var snowyTextureTask = textureManager.Checkout(snowyTextureDesc);
                    var forestTextureTask = textureManager.Checkout(forestTextureDesc);
                    var cliffTextureTask = textureManager.Checkout(cliffTextureDesc);
                    var oceanFloorTextureTask = textureManager.Checkout(oceanFloorTextureDesc);

                    var shaderSetup = primaryHitShaderFactory.Checkout();

                    await Task.Run(() =>
                    {
                        mapMesh = new IslandMazeMesh(description.csIslandMaze, floorMesh, mapUnitX: this.mapScale, mapUnitY: this.mapScale, mapUnitZ: this.mapScale)
                        {
                            WallTextureIndex = 4,
                            LowerGroundTextureIndex = 5,
                        };
                        mapMesh.Build();
                    });

                    await Task.WhenAll
                    (
                        countrysideTextureTask,
                        desertTextureTask,
                        snowyTextureTask,
                        forestTextureTask,
                        cliffTextureTask,
                        oceanFloorTextureTask,
                        floorMesh.End("SceneDungeonFloor"),
                        shaderSetup
                    );

                    this.shader = shaderSetup.Result;
                    this.countrysideTexture = countrysideTextureTask.Result;
                    this.desertTexture = desertTextureTask.Result;
                    this.snowyTexture = snowyTextureTask.Result;
                    this.forestTexture = forestTextureTask.Result;
                    this.cliffTexture = cliffTextureTask.Result;
                    this.oceanFloorTexture = oceanFloorTextureTask.Result;

                    foreach (var data in floorInstanceData)
                    {
                        data.pBLAS = mapMesh.FloorMesh.Instance.BLAS.Obj;
                        rtInstances.AddTlasBuild(data);
                    }

                    floorBlasInstanceData = this.activeTextures.AddActiveTexture(this.countrysideTexture, this.desertTexture, this.snowyTexture, this.forestTexture, this.cliffTexture, this.oceanFloorTexture);
                    floorBlasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, false, false, false);
                    rtInstances.AddShaderTableBinder(Bind);

                    SetupAreas(description.Areas, description.AirshipSquare, description.AirshipPortalSquare, description.PortalLocations);

                    loadingTask.SetResult();
                }
                catch (Exception ex)
                {
                    loadingTask.SetException(ex);
                }
            });
        }

        internal Vector3 GetAirshipPortalLocation()
        {
            return mapMesh.PointToVector(airshipPortal.x, airshipPortal.y);
        }

        /// <summary>
        /// Get the location of the portal indicated by index. The portal will be normalized, so even if you request something outside the range you will
        /// get that portal within the range of available portals.
        /// </summary>
        /// <param name="portalIndex"></param>
        public Vector3 GetPortalLocation(int portalIndex)
        {
            portalIndex %= portalLocations.Count;
            var square = portalLocations[portalIndex];

            return mapMesh.PointToVector(square.x, square.y);
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        public void Dispose()
        {
            foreach (var placeable in placeables)
            {
                placeable.RequestDestruction();
            }
            objectResolver.Dispose();
            DestroyPhysics();
            activeTextures.RemoveActiveTexture(cliffTexture);
            activeTextures.RemoveActiveTexture(countrysideTexture);
            activeTextures.RemoveActiveTexture(desertTexture);
            activeTextures.RemoveActiveTexture(snowyTexture);
            activeTextures.RemoveActiveTexture(forestTexture);
            activeTextures.RemoveActiveTexture(oceanFloorTexture);
            textureManager.TryReturn(countrysideTexture);
            textureManager.TryReturn(desertTexture);
            textureManager.TryReturn(snowyTexture);
            textureManager.TryReturn(forestTexture);
            textureManager.TryReturn(cliffTexture);
            textureManager.TryReturn(oceanFloorTexture);
            rtInstances.RemoveShaderTableBinder(Bind);
            primaryHitShaderFactory.TryReturn(shader);
            foreach(var data in floorInstanceData)
            {
                rtInstances.RemoveTlasBuild(data);
            }
        }

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

            foreach (var placeable in placeables)
            {
                placeable.CreatePhysics();
            }
        }

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

            var statics = bepuScene.Simulation.Statics;
            foreach (var staticHandle in staticHandles)
            {
                statics.Remove(staticHandle);
            }
            bepuScene.Simulation.Shapes.Remove(boundaryCubeShapeIndex);
            bepuScene.Simulation.Shapes.Remove(floorCubeShapeIndex);
            staticHandles.Clear();
        }

        public Vector3 GetAreaLocation(int area)
        {
            var square = areaLocations[area];
            return mapMesh.PointToVector(square.x, square.y);
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            floorBlasInstanceData.vertexOffset = mapMesh.FloorMesh.Instance.VertexOffset;
            floorBlasInstanceData.indexOffset = mapMesh.FloorMesh.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &floorBlasInstanceData)
            {
                foreach (var data in floorInstanceData)
                {
                    shader.BindSbt(data.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
                }
            }
        }

        public Task WaitForLoad()
        {
            return loadingTask.Task;
        }

        private void DumpDungeon(csIslandMaze mapBuilder, int seed, long creationTime)
        {
            var map = mapBuilder.Map;
            var mapWidth = mapBuilder.MapX;
            var mapHeight = mapBuilder.MapY;

            for (int mapY = mapHeight - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (map[mapX, mapY] == csIslandMaze.EmptyCell)
                    {
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.Write('X');
                    }
                }
                Console.WriteLine();
            }

            for (int mapY = mapHeight - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (map[mapX, mapY] == csIslandMaze.EmptyCell)
                    {
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.Write(map[mapX, mapY]);
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Level seed {seed}");
            Console.WriteLine($"Created in {creationTime}");
            Console.WriteLine($"Number of islands {mapBuilder.NumIslands}");
            Console.WriteLine("--------------------------------------------------");
        }

        private void SetupAreas(List<IAreaBuilder> areaBuilders, in IntVector2 airshipSquare, in IntVector2 airshipPortalSquare, List<IntVector2> portalLocations)
        {
            areaLocations = new IntVector2[areaBuilders.Count];

            {
                this.airshipStartPoint = mapMesh.PointToVector(airshipSquare.x, airshipSquare.y);     
                this.airshipPortal = airshipPortalSquare;
                var loc = mapMesh.PointToVector(airshipPortalSquare.x, airshipPortalSquare.y);

                var portal = objectResolver.Resolve<AirshipPortal, IslandPortal.Description>(o =>
                {
                    o.PortalIndex = -1;
                    o.MapOffset = loc;
                    o.Transforms = transforms;
                    o.Translation = currentPosition + o.MapOffset;
                    var entrance = new Assets.World.Portal();
                    o.Sprite = entrance.CreateSprite();
                    o.SpriteMaterial = entrance.CreateMaterial();
                    o.Scale = new Vector3(0.3f, 0.3f, 1.0f);
                });

                placeables.Add(portal);
            }

            int portalIndex = 0;
            this.portalLocations = portalLocations;
            foreach(var square in portalLocations)
            {
                var loc = mapMesh.PointToVector(square.x, square.y);

                var portal = objectResolver.Resolve<IslandPortal, IslandPortal.Description>(o =>
                {
                    o.PortalIndex = portalIndex;
                    o.MapOffset = loc;
                    o.Transforms = transforms;
                    o.Translation = currentPosition + o.MapOffset;
                    var entrance = new Assets.World.Portal();
                    o.Sprite = entrance.CreateSprite();
                    o.SpriteMaterial = entrance.CreateMaterial();
                    o.Scale = new Vector3(0.3f, 0.3f, 1.0f);
                });

                placeables.Add(portal);

                ++portalIndex;
            }

            foreach (var area in areaBuilders)
            {
                var square = area.Location;
                var loc = mapMesh.PointToVector(square.x, square.y);
                var biome = biomeManager.GetBiome(area.Biome);
                areaLocations[area.Index] = square;

                var entrance = objectResolver.Resolve<ZoneEntrance, ZoneEntrance.Description>(o =>
                {
                    o.ZoneIndex = area.Index == 0 ? area.EndZone : area.StartZone; //The first area is a special case, since the start is an empty square inside of it
                    o.MapOffset = loc;
                    o.Translation = currentPosition + o.MapOffset;
                    o.Transforms = transforms;
                    var entrance = biome.BackgroundItems[0];
                    o.Sprite = entrance.Asset.CreateSprite();
                    o.SpriteMaterial = entrance.Asset.CreateMaterial();
                    o.Scale = new Vector3(0.3f, 0.3f, 1.0f);
                });

                placeables.Add(entrance);
            }

        }

        public IntVector2 GetCellForLocation(Vector3 currentPosition)
        {
            var square = new IntVector2
            (
                Math.Max(0, (int)(currentPosition.x / mapMesh.MapUnitX) % map.MapX), 
                Math.Max(0, (int)(currentPosition.z / mapMesh.MapUnitZ) % map.MapY)
            );
            return square;
        }

        public int GetCellType(in IntVector2 cell)
        {
            return map.Map[cell.x, cell.y];
        }

        public Vector3 GetCellCenterpoint(in IntVector2 cell)
        {
            return mapMesh.PointToVector(cell.x, cell.y);
        }

        public bool CanLand(in IntVector2 cell)
        {
            return map.Map[cell.x, cell.y] != csIslandMaze.EmptyCell && !areaLocations.Contains(cell) && !portalLocations.Contains(cell) && cell != airshipPortal;
        }
    }
}
