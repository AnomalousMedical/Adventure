using Adventure.Services;
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
        }

        private readonly TLASInstanceData floorInstanceData;
        private readonly IDestructionRequest destructionRequest;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RTInstances<IWorldMapGameState> rtInstances;
        private readonly RayTracingRenderer renderer;
        private PrimaryHitShader floorShader;
        private IslandMazeMesh mapMesh;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();
        private BlasInstanceData floorBlasInstanceData;


        CC0TextureResult floorTexture;
        CC0TextureResult wallTexture;
        CC0TextureResult lowerFloorTexture;

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
            RayTracingRenderer renderer
        )
        {
            this.destructionRequest = destructionRequest;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.rtInstances = rtInstances;
            this.renderer = renderer;

            this.floorInstanceData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("SceneDungeonFloor"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(Vector3.Zero, Quaternion.Identity)
            };

            coroutineRunner.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction();
                try
                {
                    var floorTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Ground037_1K");
                    var wallTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Rock029_1K");
                    var lowerFloorTextureDesc = new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Rock022_1K");

                    var floorTextureTask = textureManager.Checkout(floorTextureDesc);
                    var wallTextureTask = textureManager.Checkout(wallTextureDesc);
                    var lowerFloorTextureTask = textureManager.Checkout(lowerFloorTextureDesc);

                    var floorShaderSetup = primaryHitShaderFactory.Checkout();

                    await Task.Run(() =>
                    {
                        mapMesh = new IslandMazeMesh(description.csIslandMaze, floorMesh, mapUnitX: 1.0f, mapUnitY: 1.0f, mapUnitZ: 1.0f);
                    });

                    await Task.WhenAll
                    (
                        floorTextureTask,
                        wallTextureTask,
                        lowerFloorTextureTask,
                        floorMesh.End("SceneDungeonFloor"),
                        floorShaderSetup
                    );

                    this.floorShader = floorShaderSetup.Result;
                    this.floorTexture = floorTextureTask.Result;
                    this.wallTexture = wallTextureTask.Result;
                    this.lowerFloorTexture = lowerFloorTextureTask.Result;

                    this.floorInstanceData.pBLAS = mapMesh.FloorMesh.Instance.BLAS.Obj;

                    floorBlasInstanceData = this.activeTextures.AddActiveTexture(this.floorTexture, this.wallTexture, this.lowerFloorTexture);
                    floorBlasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, false, false, false);
                    rtInstances.AddTlasBuild(floorInstanceData);
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
            activeTextures.RemoveActiveTexture(wallTexture);
            activeTextures.RemoveActiveTexture(floorTexture);
            activeTextures.RemoveActiveTexture(lowerFloorTexture);
            textureManager.TryReturn(floorTexture);
            textureManager.TryReturn(wallTexture);
            textureManager.TryReturn(lowerFloorTexture);
            rtInstances.RemoveShaderTableBinder(Bind);
            primaryHitShaderFactory.TryReturn(floorShader);
            rtInstances.RemoveTlasBuild(floorInstanceData);
        }

        public void SetTransform(InstanceMatrix matrix)
        {
            this.floorInstanceData.Transform = matrix;
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
    }
}
