﻿using Adventure.Assets.World;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using BepuUtilities.Collections;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using DungeonGenerator;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class WorldMapInstance : IDisposable
    {
        public class Description
        {
            public csIslandMaze csIslandMaze { get; set; }

            public List<IAreaBuilder> Areas { get; set; }

            public IntVector2 AirshipSquare { get; set; }

            public IEnumerable<IntVector2> BiomePropLocations { get; set; }

            public float MapScale { get; set; } = 1.0f;
        }

        private readonly TLASInstanceData[] floorInstanceData;
        private readonly IDestructionRequest destructionRequest;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RTInstances<WorldMapScene> rtInstances;
        private readonly RayTracingRenderer renderer;
        private readonly IBepuScene<WorldMapScene> bepuScene;
        private readonly IBiomeManager biomeManager;
        private readonly NoiseTextureManager noiseTextureManager;
        private readonly IWorldDatabase worldDatabase;
        private readonly csIslandMaze map;
        private readonly IObjectResolver objectResolver;
        private PrimaryHitShader shader;
        private IslandMazeMesh mapMesh;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();
        private BlasInstanceData floorBlasInstanceData;
        private bool physicsActive = false;
        private TypedIndex boundaryCubeShapeIndex;
        private TypedIndex floorShapeIndex;
        private bool madeCollisionShapes = false;
        private StaticHandle floorStaticHandle;
        private List<StaticHandle> staticHandles = new List<StaticHandle>();
        private Vector3 currentPosition = Vector3.Zero;
        private List<IWorldMapPlaceable> placeables = new List<IWorldMapPlaceable>();
        private IntVector2[] areaLocations;
        private float mapScale;
        private Vector2 mapSize;
        private Vector3[] transforms;
        private Vector3 airshipStartPoint;

        public bool PhysicsActive => physicsActive;

        public Vector3[] Transforms => transforms;

        public Vector3 AirshipStartPoint => airshipStartPoint;

        public Vector2 MapSize => mapSize;

        List<CC0TextureResult> loadedTextures = new List<CC0TextureResult>();

        public WorldMapInstance
        (
            Description description,
            IScopedCoroutine coroutineRunner,
            IDestructionRequest destructionRequest,
            MeshBLAS floorMesh,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RTInstances<WorldMapScene> rtInstances,
            RayTracingRenderer renderer,
            IObjectResolverFactory objectResolverFactory,
            IBepuScene<WorldMapScene> bepuScene,
            IBiomeManager biomeManager,
            NoiseTextureManager noiseTextureManager,
            TerrainNoise terrainNoise,
            IWorldDatabase worldDatabase
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
            this.noiseTextureManager = noiseTextureManager;
            this.worldDatabase = worldDatabase;
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
            for (var i = 0; i < floorInstanceData.Length; i++)
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
                    var mountainTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Rock026_1K");
                    var snowyTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Snow006_1K");
                    var forestTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Ground042_1K");
                    var beachTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Ground060_1K");
                    var swampTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Moss001_1K");
                    var volcanoTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Rock037_1K");
                    var cliffTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Rock029_1K");

                    var countrysideTextureTask = textureManager.Checkout(countrysideTextureDesc);
                    var mountainTextureTask = textureManager.Checkout(mountainTextureDesc);
                    var snowyTextureTask = textureManager.Checkout(snowyTextureDesc);
                    var forestTextureTask = textureManager.Checkout(forestTextureDesc);
                    var beachTextureTask = textureManager.Checkout(beachTextureDesc);
                    var swampTextureTask = textureManager.Checkout(swampTextureDesc);
                    var volcanoTextureTask = textureManager.Checkout(volcanoTextureDesc);
                    var cliffTextureTask = textureManager.Checkout(cliffTextureDesc);

                    var shaderSetup = primaryHitShaderFactory.Checkout();

                    await Task.Run(() =>
                    {
                        mapMesh = new IslandMazeMesh(description.csIslandMaze, floorMesh, mapUnitX: this.mapScale, mapUnitY: this.mapScale, mapUnitZ: this.mapScale)
                        {
                            WallTextureIndex = 7,
                            LowerGroundTextureIndex = 7,
                        };
                        mapMesh.Build();
                    });

                    await Task.WhenAll
                    (
                        countrysideTextureTask,
                        mountainTextureTask,
                        snowyTextureTask,
                        forestTextureTask,
                        beachTextureTask,
                        swampTextureTask,
                        volcanoTextureTask,
                        cliffTextureTask,
                        floorMesh.End("SceneDungeonFloor"),
                        shaderSetup
                    );

                    this.shader = shaderSetup.Result;

                    var countrysideTexture = countrysideTextureTask.Result;
                    var mountainTexture = mountainTextureTask.Result;
                    var snowyTexture = snowyTextureTask.Result;
                    var forestTexture = forestTextureTask.Result;
                    var beachTexture = beachTextureTask.Result;
                    var swampTexture = swampTextureTask.Result;
                    var volcanoTexture = volcanoTextureTask.Result;
                    var cliffTexture = cliffTextureTask.Result;

                    loadedTextures.Add(countrysideTexture);
                    loadedTextures.Add(mountainTexture);
                    loadedTextures.Add(snowyTexture);
                    loadedTextures.Add(forestTexture);
                    loadedTextures.Add(beachTexture);
                    loadedTextures.Add(swampTexture);
                    loadedTextures.Add(volcanoTexture);
                    loadedTextures.Add(cliffTexture);

                    foreach (var data in floorInstanceData)
                    {
                        data.pBLAS = mapMesh.FloorMesh.Instance.BLAS.Obj;
                        rtInstances.AddTlasBuild(data);
                    }

                    floorBlasInstanceData = this.activeTextures.AddActiveTexture(
                        countrysideTexture,
                        mountainTexture,
                        snowyTexture,
                        forestTexture,
                        beachTexture,
                        swampTexture,
                        volcanoTexture,
                        cliffTexture);

                    floorBlasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, false, false);
                    rtInstances.AddShaderTableBinder(Bind);

                    SetupAreas(description.Areas, description.AirshipSquare, description.BiomePropLocations);

                    loadingTask.SetResult();
                }
                catch (Exception ex)
                {
                    loadingTask.SetException(ex);
                }
            });
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
            foreach (var texture in loadedTextures)
            {
                activeTextures.RemoveActiveTexture(texture);
            }
            foreach (var texture in loadedTextures)
            {
                textureManager.TryReturn(texture);
            }
            rtInstances.RemoveShaderTableBinder(Bind);
            primaryHitShaderFactory.TryReturn(shader);
            foreach (var data in floorInstanceData)
            {
                rtInstances.RemoveTlasBuild(data);
            }

            if (madeCollisionShapes)
            {
                bepuScene.Simulation.Shapes.Remove(floorShapeIndex);
                bepuScene.Simulation.Shapes.Remove(boundaryCubeShapeIndex);
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

            if (!madeCollisionShapes)
            {
                madeCollisionShapes = true;

                var boundaryCubeShape = new Box(mapMesh.MapUnitX, mapMesh.MapUnitY * yBoundaryScale, mapMesh.MapUnitZ); //Each one creates its own, try to load from resources
                boundaryCubeShapeIndex = bepuScene.Simulation.Shapes.Add(boundaryCubeShape);

                var triangles = new QuickList<Triangle>(mapMesh.CollisionMeshPositions.Count() * 2, bepuScene.BufferPool);
                foreach (var centerPt in mapMesh.CollisionMeshPositions)
                {
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
            }

            var boundaryOrientation = System.Numerics.Quaternion.Identity;

            //Floor
            floorStaticHandle = bepuScene.Simulation.Statics.Add(
                      new StaticDescription(
                          currentPosition.ToSystemNumerics(),
                          System.Numerics.Quaternion.Identity,
                          floorShapeIndex));

            staticHandles.Add(floorStaticHandle);

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

        private void SetupAreas(List<IAreaBuilder> areaBuilders, in IntVector2 airshipSquare, IEnumerable<IntVector2> biomeProps)
        {
            areaLocations = new IntVector2[areaBuilders.Count];

            {
                var blacksmithUpgrade = objectResolver.Resolve<BlacksmithUpgrade, BlacksmithUpgrade.Description>(o =>
                {
                    o.Transforms = transforms;
                    var entrance = new Gargoyle();
                    o.Sprite = entrance.CreateSprite();
                    o.SpriteMaterial = entrance.CreateMaterial();
                    o.Scale = new Vector3(0.3f, 0.3f, 1.0f);
                });

                placeables.Add(blacksmithUpgrade);
            }

            {
                var alchemistUpgrade = objectResolver.Resolve<AlchemistUpgrade, AlchemistUpgrade.Description>(o =>
                {
                    o.Transforms = transforms;
                    var entrance = new Gargoyle();
                    o.Sprite = entrance.CreateSprite();
                    o.SpriteMaterial = entrance.CreateMaterial();
                    o.Scale = new Vector3(0.3f, 0.3f, 1.0f);
                });

                placeables.Add(alchemistUpgrade);
            }

            {
                var innkeeper = objectResolver.Resolve<Innkeeper, Innkeeper.Description>(o =>
                {
                    o.Transforms = transforms;
                    var sprite = new Assets.NPC.Innkeeper();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                    o.Scale = new Vector3(0.35f, 0.35f, 1.0f);
                });

                placeables.Add(innkeeper);

                var inn = objectResolver.Resolve<WorldMapProp, WorldMapProp.Description>(o =>
                {
                    o.Translation = GetCellCenterpoint(worldDatabase.InnkeeperPosition) + new Vector3(0f, 0f, 0.2f);
                    o.Transforms = transforms;
                    var sprite = new Assets.World.Inn();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                });

                placeables.Add(inn);
            }

            {
                var blacksmith = objectResolver.Resolve<Blacksmith, Blacksmith.Description>(o =>
                {
                    o.Transforms = transforms;
                    var sprite = new Assets.NPC.Blacksmith();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                    o.Scale = new Vector3(0.35f, 0.35f, 1.0f);
                });

                placeables.Add(blacksmith);

                var blacksmithShop = objectResolver.Resolve<WorldMapProp, WorldMapProp.Description>(o =>
                {
                    o.Translation = GetCellCenterpoint(worldDatabase.BlacksmithPosition) + new Vector3(0f, 0f, 0.2f);
                    o.Transforms = transforms;
                    var sprite = new Assets.World.BlacksmithShop();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                });

                placeables.Add(blacksmithShop);
            }

            {
                var airshipEngineer = objectResolver.Resolve<AirshipEngineer, AirshipEngineer.Description>(o =>
                {
                    o.Transforms = transforms;
                    var sprite = new Assets.NPC.Engineer();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                    o.Scale = new Vector3(0.35f, 0.35f, 1.0f);
                });

                placeables.Add(airshipEngineer);
            }

            {
                var alchemist = objectResolver.Resolve<Alchemist, Alchemist.Description>(o =>
                {
                    o.Transforms = transforms;
                    var sprite = new Assets.NPC.Alchemist();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                    o.Scale = new Vector3(0.35f, 0.35f, 1.0f);
                });

                placeables.Add(alchemist);

                var alchemistShop = objectResolver.Resolve<WorldMapProp, WorldMapProp.Description>(o =>
                {
                    o.Translation = GetCellCenterpoint(worldDatabase.AlchemistPosition) + new Vector3(0f, 0f, 0.2f);
                    o.Transforms = transforms;
                    var sprite = new Assets.World.AlchemistShop();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                });

                placeables.Add(alchemistShop);
            }

            {
                var fortuneTeller = objectResolver.Resolve<FortuneTeller, FortuneTeller.Description>(o =>
                {
                    o.Transforms = transforms;
                    var sprite = new Assets.NPC.FortuneTeller();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                    o.Scale = new Vector3(0.35f, 0.35f, 1.0f);
                });

                placeables.Add(fortuneTeller);

                var fortuneTellerTent = objectResolver.Resolve<WorldMapProp, WorldMapProp.Description>(o =>
                {
                    o.Translation = GetCellCenterpoint(worldDatabase.FortuneTellerPosition) + new Vector3(0f, 0f, 0.2f);
                    o.Transforms = transforms;
                    var sprite = new Assets.World.FortuneTellerTent();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                });

                placeables.Add(fortuneTellerTent);
            }

            {
                var itemStorage = objectResolver.Resolve<ItemStorage, ItemStorage.Description>(o =>
                {
                    o.Transforms = transforms;
                    var sprite = new Assets.World.Gargoyle();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                    o.Scale = new Vector3(0.3f, 0.3f, 1.0f);
                });

                placeables.Add(itemStorage);
            }

            {
                var elementalStone = objectResolver.Resolve<ElementalStone, ElementalStone.Description>(o =>
                {
                    o.Transforms = transforms;
                    var sprite = new Assets.World.ElementalStone();
                    o.Sprite = sprite.CreateSprite();
                    o.SpriteMaterial = sprite.CreateMaterial();
                    o.Scale = new Vector3(0.7f, 0.7f, 1.0f);
                });

                placeables.Add(elementalStone);
            }

            {
                var water = objectResolver.Resolve<WorldWater, WorldWater.Description>(o =>
                {
                    o.Translation = new Vector3(0f, 0.69f, 0f);
                });

                placeables.Add(water);
            }

            this.airshipStartPoint = mapMesh.PointToVector(airshipSquare.x, airshipSquare.y);

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
                    var entrance = biome.EntranceAsset;
                    o.Sprite = entrance.CreateSprite();
                    o.SpriteMaterial = entrance.CreateMaterial();
                    o.Scale = new Vector3(0.3f, 0.3f, 1.0f);
                });

                placeables.Add(entrance);
            }

            var bgItemsRandom = new FIRandom(worldDatabase.CurrentSeed);

            foreach (var prop in biomeProps)
            {
                var textureIndex = map.TextureOffsets[prop.x, prop.y];
                var biomeType = (BiomeType)textureIndex;
                var biome = biomeManager.GetBiome(biomeType);
                var add = biome.BackgroundItems.FirstOrDefault();

                if (add != null)
                {
                    var biomeProp = objectResolver.Resolve<WorldMapProp, WorldMapProp.Description>(o =>
                    {
                        var mustBeEven = prop.x % 2 == 0;

                        var mapUnitX = mapMesh.MapUnitX * add.XPlacementRange;
                        var halfUnitX = mapUnitX * 0.5f;
                        var mapUnitZ = mapMesh.MapUnitZ * add.ZPlacementRange;
                        var halfUnitZ = mapUnitZ * 0.5f;

                        var scale = new Vector3(add.WorldScale, add.WorldScale, 1.0f) * (bgItemsRandom.NextSingle() * add.ScaleRange + add.ScaleMin);
                        var mapLoc = mapMesh.PointToVector(prop.x, prop.y);
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

                        o.Translation = currentPosition + mapLoc;
                        o.Sprite = sprite;
                        o.SpriteMaterial = keyAsset.CreateMaterial();
                        o.Scale = scale;
                        o.Transforms = transforms;
                    });

                    placeables.Add(biomeProp);
                }
            }
        }

        public IntVector2 GetCellForLocation(Vector3 currentPosition)
        {
            var square = new IntVector2
            (
                Math.Max(0, (int)(currentPosition.x / mapMesh.MapUnitX) % map.MapX),
                Math.Max(0, (int)(currentPosition.z / mapMesh.MapUnitZ + 1) % map.MapY)
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
            return map.Map[cell.x, cell.y] != csIslandMaze.EmptyCell && !areaLocations.Contains(cell);
        }
    }
}
