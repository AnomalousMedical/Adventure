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
        const float FloorTextureIndex = 0.5f;
        const float FloorTexture2Index = 1.5f;
        const float WallTextureIndex = 2.5f;
        const float WallTexture2Index = 3.5f;

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
        private readonly NoiseTextureManager noiseTextureManager;
        private PrimaryHitShader floorShader;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();
        private CC0TextureResult floorTexture;
        private CC0TextureResult floorTexture2;
        private CC0TextureResult wallTexture;
        private CC0TextureResult wallTexture2;
        CC0TextureResult noiseTexture;
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
            RTInstances<BattleScene> rtInstances,
            NoiseTextureManager noiseTextureManager,
            TerrainNoise terrainNoise
        )
        {
            this.destructionRequest = destructionRequest;
            this.floorMesh = floorMesh;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.rtInstances = rtInstances;
            this.noiseTextureManager = noiseTextureManager;
            coroutineRunner.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction();
                try
                {
                    var floorTextureDesc = new CCOTextureBindingDescription(description.Biome.FloorTexture, reflective: description.Biome.ReflectFloor);
                    var floorTextureDesc2 = new CCOTextureBindingDescription(description.Biome.FloorTexture2 ?? description.Biome.FloorTexture, reflective: description.Biome.ReflectFloor);
                    var wallTextureDesc = new CCOTextureBindingDescription(description.Biome.WallTexture, reflective: description.Biome.ReflectWall);
                    var wallTextureDesc2 = new CCOTextureBindingDescription(description.Biome.WallTexture2 ?? description.Biome.WallTexture, reflective: description.Biome.ReflectWall);

                    var floorTextureTask = textureManager.Checkout(floorTextureDesc);
                    var floorTexture2Task = textureManager.Checkout(floorTextureDesc2);
                    var wallTextureTask = textureManager.Checkout(wallTextureDesc);
                    var wallTexture2Task = textureManager.Checkout(wallTextureDesc2);

                    var noise = terrainNoise.CreateBlendTerrainNoise(0);
                    var noiseTask = noiseTextureManager.GenerateTexture(noise, 4096, 4096);

                    await Task.Run(() =>
                    {
                        floorMesh.Begin(5);

                        floorMesh.AddQuad(new Vector3(-size, 0, size), new Vector3(size, 0, size), new Vector3(size, 0, -size), new Vector3(-size, 0, -size),
                                          Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                                          new Vector2(0, 0),
                                          new Vector2(size, size),
                                          new Vector2(0, 0),
                                          new Vector2(1, 1), 
                                          FloorTextureIndex,
                                          FloorTexture2Index);

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
                            new Vector2(0, 0),
                            new Vector2(1, 1),
                            FloorTextureIndex,
                            FloorTexture2Index);

                        //Floor +x
                        floorMesh.AddQuad(
                            new Vector3(-farbgSize + dirOffset, 0, size),
                            new Vector3(farbgSize + dirOffset, 0, size),
                            new Vector3(farbgSize + dirOffset, 0, -size),
                            new Vector3(-farbgSize + dirOffset, 0, -size),
                            Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                            new Vector2(0, 0),
                            new Vector2(farbgSize, size),
                            new Vector2(0, 0),
                            new Vector2(1, 1),
                            FloorTextureIndex,
                            FloorTexture2Index);

                        //Wall +z
                        floorMesh.AddQuad(
                            new Vector3(-farbgSize, 0, farbgSize + dirOffset),
                            new Vector3(farbgSize, 0, farbgSize + dirOffset),
                            new Vector3(farbgSize, 0, -farbgSize + dirOffset),
                            new Vector3(-farbgSize, 0, -farbgSize + dirOffset),
                            Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                            new Vector2(0, 0),
                            new Vector2(farbgSize, farbgSize),
                            new Vector2(0, 0),
                            new Vector2(1, 1),
                            WallTextureIndex,
                            WallTexture2Index);

                        //Wall -z
                        floorMesh.AddQuad(
                            new Vector3(-farbgSize, 0, farbgSize - dirOffset),
                            new Vector3(farbgSize, 0, farbgSize - dirOffset),
                            new Vector3(farbgSize, 0, -farbgSize - dirOffset),
                            new Vector3(-farbgSize, 0, -farbgSize - dirOffset),
                            Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up,
                            new Vector2(0, 0),
                            new Vector2(farbgSize, farbgSize),
                            new Vector2(0, 0),
                            new Vector2(1, 1),
                            WallTextureIndex,
                            WallTexture2Index);
                    });

                    await floorMesh.End("BattleArenaFloor");

                    var floorShaderSetup = primaryHitShaderFactory.Checkout();

                    await Task.WhenAll
                    (
                        floorTextureTask,
                        wallTextureTask,
                        floorShaderSetup,
                        noiseTask
                    );

                    this.floorShader = floorShaderSetup.Result;
                    this.floorTexture = floorTextureTask.Result;
                    this.floorTexture2 = floorTexture2Task.Result;
                    this.wallTexture = wallTextureTask.Result;
                    this.wallTexture2 = wallTexture2Task.Result;
                    noiseTexture = noiseTask.Result;

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
                    blasInstanceData = activeTextures.AddActiveTexture(floorTexture, floorTexture2, wallTexture, wallTexture2, noiseTexture);
                    blasInstanceData.padding = 4; //This is the noise texture index, which is the 5th texture
                    blasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(true, true, description.Biome.ReflectFloor, false, BlasSpecialMaterial.MultiTexture);

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
            activeTextures.RemoveActiveTexture(wallTexture2);
            activeTextures.RemoveActiveTexture(floorTexture);
            activeTextures.RemoveActiveTexture(floorTexture2);
            activeTextures.RemoveActiveTexture(noiseTexture);
            textureManager.TryReturn(wallTexture);
            textureManager.TryReturn(wallTexture2);
            textureManager.TryReturn(floorTexture);
            textureManager.TryReturn(floorTexture2);
            noiseTextureManager.ReturnTexture(noiseTexture);
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
