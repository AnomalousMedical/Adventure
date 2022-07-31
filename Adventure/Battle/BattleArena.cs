using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    class BattleArena : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            public IBiome Biome { get; set; }
        }

        private TLASInstanceData floorInstanceData;
        private readonly IDestructionRequest destructionRequest;
        private readonly MeshBLAS floorMesh;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RTInstances rtInstances;
        private PrimaryHitShader floorShader;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();
        private CC0TextureResult floorTexture;
        private CC0TextureResult wallTexture;
        private BlasInstanceData blasInstanceData;

        private const float size = 8.5f;
        private const float farbgSize = 80f;

        public BattleArena
        (
            Description description,
            IScopedCoroutine coroutineRunner,
            IDestructionRequest destructionRequest,
            MeshBLAS floorMesh,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RTInstances<IBattleManager> rtInstances
        )
        {
            this.destructionRequest = destructionRequest;
            this.floorMesh = floorMesh;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.rtInstances = rtInstances;
            coroutineRunner.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction();
                try
                {
                    var floorTextureDesc = new CCOTextureBindingDescription(description.Biome.FloorTexture, reflective: description.Biome.ReflectFloor);
                    var wallTextureDesc = new CCOTextureBindingDescription(description.Biome.WallTexture, reflective: description.Biome.ReflectWall);

                    var floorTextureTask = textureManager.Checkout(floorTextureDesc);
                    var wallTextureTask = textureManager.Checkout(wallTextureDesc);

                    await Task.Run(() =>
                    {
                        floorMesh.Begin(5);

                        floorMesh.AddQuad(new Vector3(-size, 0, size), new Vector3(size, 0, size), new Vector3(size, 0, -size), new Vector3(-size, 0, -size),
                                          Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                                          new Vector2(0, 0),
                                          new Vector2(size, size), 0.5f);

                        var dirOffset = farbgSize + size;

                        //Floor -x
                        floorMesh.AddQuad(
                            new Vector3(-farbgSize - dirOffset, 0, size),
                            new Vector3(farbgSize - dirOffset, 0, size),
                            new Vector3(farbgSize - dirOffset, 0, -size),
                            new Vector3(-farbgSize - dirOffset, 0, -size),
                            Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                            new Vector2(0, 0),
                            new Vector2(farbgSize, size),
                            0.5f);

                        //Floor +x
                        floorMesh.AddQuad(
                            new Vector3(-farbgSize + dirOffset, 0, size),
                            new Vector3(farbgSize + dirOffset, 0, size),
                            new Vector3(farbgSize + dirOffset, 0, -size),
                            new Vector3(-farbgSize + dirOffset, 0, -size),
                            Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                            new Vector2(0, 0),
                            new Vector2(farbgSize, size),
                            0.5f);

                        //Wall +z
                        floorMesh.AddQuad(
                            new Vector3(-farbgSize, 0, farbgSize + dirOffset),
                            new Vector3(farbgSize, 0, farbgSize + dirOffset),
                            new Vector3(farbgSize, 0, -farbgSize + dirOffset),
                            new Vector3(-farbgSize, 0, -farbgSize + dirOffset),
                            Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                            new Vector2(0, 0),
                            new Vector2(farbgSize, farbgSize),
                            1.5f);

                        //Wall -z
                        floorMesh.AddQuad(
                            new Vector3(-farbgSize, 0, farbgSize - dirOffset),
                            new Vector3(farbgSize, 0, farbgSize - dirOffset),
                            new Vector3(farbgSize, 0, -farbgSize - dirOffset),
                            new Vector3(-farbgSize, 0, -farbgSize - dirOffset),
                            Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                            new Vector2(0, 0),
                            new Vector2(farbgSize, farbgSize),
                            1.5f);
                    });

                    await floorMesh.End("BattleArenaFloor");

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

                    this.floorInstanceData = new TLASInstanceData()
                    {
                        InstanceName = RTId.CreateId("BattleArenaFloor"),
                        CustomId = 3, //Texture index
                        pBLAS = floorMesh.Instance.BLAS.Obj,
                        Mask = RtStructures.OPAQUE_GEOM_MASK,
                        Transform = new InstanceMatrix(Vector3.Zero, Quaternion.Identity)
                    };

                    rtInstances.AddTlasBuild(floorInstanceData);
                    rtInstances.AddShaderTableBinder(Bind);
                    blasInstanceData = activeTextures.AddActiveTexture(floorTexture, wallTexture);
                    blasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, description.Biome.ReflectFloor, false, false);

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
            textureManager.TryReturn(wallTexture);
            textureManager.TryReturn(floorTexture);
            rtInstances.RemoveShaderTableBinder(Bind);
            primaryHitShaderFactory.TryReturn(floorShader);
            rtInstances.RemoveTlasBuild(floorInstanceData);
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            blasInstanceData.vertexOffset = floorMesh.Instance.VertexOffset;
            blasInstanceData.indexOffset = floorMesh.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &blasInstanceData)
            {
                floorShader.BindSbt(floorInstanceData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
            }
        }

        public Task WaitForLoad()
        {
            return loadingTask.Task;
        }

        public IEnumerable<Vector3> BgItemLocations()
        {
            var step = 3;
            var xEnd = farbgSize;
            var zStart = -farbgSize + size + farbgSize + 2f;
            var zEnd = farbgSize + size + farbgSize;

            for(var x = -farbgSize; x < xEnd; x += step)
            {
                for (var z = zStart; z < zEnd; z += step)
                {
                    yield return new Vector3(x, 0, z);
                }
            }

            zStart = -farbgSize - size - farbgSize - 5f;
            zEnd = farbgSize - size - farbgSize;

            for (var x = -farbgSize; x < xEnd; x += step)
            {
                for (var z = zStart; z < zEnd; z += step)
                {
                    yield return new Vector3(x, 0, z);
                }
            }
        }
    }
}
