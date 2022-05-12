﻿using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using DungeonGenerator;
using Engine;
using RogueLikeMapBuilder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTDungeonGeneratorTest
{
    internal class SceneDungeon : IDisposable
    {
        public class Desc
        {
            public string InstanceName { get; set; } = RTId.CreateId("SceneDungeon");

            public InstanceMatrix Transform = InstanceMatrix.Identity;

            public int Seed { get; set; }
        }

        private readonly TLASBuildInstanceData wallInstanceData;
        private readonly TLASBuildInstanceData floorInstanceData;
        private readonly IDestructionRequest destructionRequest;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RTInstances rtInstances;
        private readonly RayTracingRenderer renderer;
        private PrimaryHitShader floorShader;
        private PrimaryHitShader wallShader;
        private MapMesh mapMesh;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();
        private BlasInstanceData floorBlasInstanceData;
        private BlasInstanceData wallBlasInstanceData;


        CC0TextureResult floorTexture;
        CC0TextureResult wallTexture;

        public SceneDungeon
        (
            Desc description,
            IScopedCoroutine coroutineRunner,
            IDestructionRequest destructionRequest,
            MeshBLAS floorMesh,
            MeshBLAS wallMesh,
            TextureManager textureManager, 
            ActiveTextures activeTextures,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RTInstances rtInstances,
            RayTracingRenderer renderer
        )
        {
            this.destructionRequest = destructionRequest;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.rtInstances = rtInstances;
            this.renderer = renderer;

            this.floorInstanceData = new TLASBuildInstanceData()
            {
                InstanceName = RTId.CreateId("SceneDungeonFloor"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(Vector3.Zero, Quaternion.Identity)
            };
            this.wallInstanceData = new TLASBuildInstanceData()
            {
                InstanceName = RTId.CreateId("SceneDungeonWall"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(Vector3.Zero, Quaternion.Identity)
            };

            coroutineRunner.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction();
                try
                {
                    var floorTextureDesc = new CCOTextureBindingDescription("cc0Textures/Ground025_1K");
                    var wallTextureDesc = new CCOTextureBindingDescription("cc0Textures/Ground042_1K");

                    var floorTextureTask = textureManager.Checkout(floorTextureDesc);
                    var wallTextureTask = textureManager.Checkout(wallTextureDesc);

                    var shaderDesc = new PrimaryHitShader.Desc();
                    var floorShaderSetup = primaryHitShaderFactory.Checkout(shaderDesc);
                    var wallShaderSetup = primaryHitShaderFactory.Checkout(shaderDesc);

                    await Task.Run(() =>
                    {
                        var sw = new Stopwatch();
                        sw.Start();
                        var random = new Random(description.Seed);
                        var mapBuilder = new csMapbuilder(random, 50, 50);
                        mapBuilder.CorridorSpace = 10;
                        mapBuilder.RoomDistance = 3;
                        mapBuilder.Room_Min = new IntSize2(2, 2);
                        mapBuilder.Room_Max = new IntSize2(6, 6); //Between 3-6 is good here, 3 for more cityish with small rooms, 6 for more open with more big rooms, sometimes connected
                        mapBuilder.Corridor_Max = 4;
                        mapBuilder.Horizontal = false;
                        mapBuilder.Build_ConnectedStartRooms();
                        mapBuilder.AddNorthConnector();
                        mapBuilder.AddSouthConnector();
                        mapBuilder.AddWestConnector();
                        mapBuilder.AddEastConnector();
                        sw.Stop();

                        //DumpDungeon(mapBuilder, description.Seed, sw.ElapsedMilliseconds);

                        mapMesh = new MapMesh(mapBuilder, random, floorMesh, wallMesh, mapUnitX: 3.0f, mapUnitY: 0.1f, mapUnitZ: 1.5f);
                    });

                    await Task.WhenAll
                    (
                        floorTextureTask,
                        wallTextureTask,
                        floorMesh.End("SceneDungeonFloor"), 
                        wallMesh.End("SceneDungeonWall"),
                        floorShaderSetup,
                        wallShaderSetup
                    );

                    this.floorShader = floorShaderSetup.Result;
                    this.wallShader = wallShaderSetup.Result;
                    this.floorTexture = floorTextureTask.Result;
                    this.wallTexture = wallTextureTask.Result;

                    this.floorInstanceData.pBLAS = mapMesh.FloorMesh.Instance.BLAS.Obj;
                    this.wallInstanceData.pBLAS = mapMesh.WallMesh.Instance.BLAS.Obj;

                    floorBlasInstanceData = this.activeTextures.AddActiveTexture(this.floorTexture);
                    floorBlasInstanceData.lightingType = BlasInstanceDataConstants.GetShaderForDescription(true, true, false, false);
                    wallBlasInstanceData = this.activeTextures.AddActiveTexture(this.wallTexture);
                    wallBlasInstanceData.lightingType = BlasInstanceDataConstants.GetShaderForDescription(true, true, false, false);
                    rtInstances.AddTlasBuild(floorInstanceData);
                    rtInstances.AddTlasBuild(wallInstanceData);
                    rtInstances.AddShaderTableBinder(Bind);

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
            textureManager.TryReturn(wallTexture);
            textureManager.TryReturn(floorTexture);
            rtInstances.RemoveShaderTableBinder(Bind);
            primaryHitShaderFactory.TryReturn(wallShader);
            primaryHitShaderFactory.TryReturn(floorShader);
            rtInstances.RemoveTlasBuild(floorInstanceData);
            rtInstances.RemoveTlasBuild(wallInstanceData);
        }

        public void SetTransform(InstanceMatrix matrix)
        {
            this.floorInstanceData.Transform = matrix;
            this.wallInstanceData.Transform = matrix;
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            floorBlasInstanceData.vertexOffset = mapMesh.FloorMesh.Instance.VertexOffset;
            floorBlasInstanceData.indexOffset = mapMesh.FloorMesh.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &floorBlasInstanceData)
            {
                floorShader.BindSbt(floorInstanceData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
            }

            wallBlasInstanceData.vertexOffset = mapMesh.WallMesh.Instance.VertexOffset;
            wallBlasInstanceData.indexOffset = mapMesh.WallMesh.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &wallBlasInstanceData)
            {
                wallShader.BindSbt(wallInstanceData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
            }
        }

        public Task WaitForLoad()
        {
            return loadingTask.Task;
        }

        private void DumpDungeon(csMapbuilder mapBuilder, int seed, long creationTime)
        {
            var map = mapBuilder.map;
            var mapWidth = mapBuilder.Map_Size.Width;
            var mapHeight = mapBuilder.Map_Size.Height;

            for (int mapY = mapBuilder.Map_Size.Height - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    switch (map[mapX, mapY])
                    {
                        case csMapbuilder.EmptyCell:
                            Console.Write(' ');
                            break;
                        case csMapbuilder.MainCorridorCell:
                            Console.Write('M');
                            break;
                        case csMapbuilder.RoomCell:
                            Console.Write('S');
                            break;
                        case csMapbuilder.RoomCell + 1:
                            Console.Write('E');
                            break;
                        default:
                            Console.Write('X');
                            break;
                    }
                }
                Console.WriteLine();
            }

            for (int mapY = mapBuilder.Map_Size.Height - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (map[mapX, mapY] == csMapbuilder.EmptyCell)
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
            Console.WriteLine(mapBuilder.StartRoom);
            Console.WriteLine(mapBuilder.EndRoom);
            Console.WriteLine("--------------------------------------------------");
        }
    }
}
