using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using Engine;
using System;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    internal class WorldWater : IDisposable, IWorldMapPlaceable
    {
        public class Description
        {
            public string InstanceName { get; set; } = RTId.CreateId("WorldWater");

            public byte Mask { get; set; } = RtStructures.TRANSPARENT_GEOM_MASK;

            public RAYTRACING_INSTANCE_FLAGS Flags { get; set; } = RAYTRACING_INSTANCE_FLAGS.RAYTRACING_INSTANCE_NONE;


            public InstanceMatrix Transform = InstanceMatrix.Identity;
        }

        private readonly TLASInstanceData instanceData;
        private readonly MeshBLAS meshBlas;
        private readonly RTInstances rtInstances;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RayTracingRenderer renderer;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private readonly IDestructionRequest destructionRequest;
        private PrimaryHitShader primaryHitShader;
        private BlasInstanceData blasInstanceData;

        public WorldWater
        (
            Description description,
            MeshBLAS meshBlas,
            IScopedCoroutine coroutine,
            RTInstances<WorldMapScene> rtInstances,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RayTracingRenderer renderer,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            IDestructionRequest destructionRequest
        )
        {
            this.meshBlas = meshBlas;
            this.rtInstances = rtInstances;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.renderer = renderer;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.destructionRequest = destructionRequest;
            this.instanceData = new TLASInstanceData()
            {
                InstanceName = description.InstanceName,
                Mask = description.Mask,
                Transform = description.Transform,
                Flags = description.Flags,
            };

            coroutine.RunTask(async () =>
            {
                var primaryHitShaderTask = primaryHitShaderFactory.Checkout();

                meshBlas.Begin(1);

                var unit = 1000.0f;

                meshBlas.AddQuad(
                    new Vector3(-unit, 0, -unit),
                    new Vector3(unit, 0, -unit),
                    new Vector3(unit, 0, unit),
                    new Vector3(-unit, 0, unit),
                    Vector3.Up, Vector3.Up, Vector3.Up, Vector3.Up, 
                    new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(0f, 0f), new Vector2(0f, 0f));

                await Task.WhenAll
                (
                    meshBlas.End("WorldWater"),
                    primaryHitShaderTask
                );

                this.instanceData.pBLAS = meshBlas.Instance.BLAS.Obj;
                this.primaryHitShader = primaryHitShaderTask.Result;
                blasInstanceData = GlassInstanceDataCreator.Create(new Vector3(0.5f, 0.5f, 0.5f), 100.95f, new Vector2(1.5f, 1.02f), new Color(0.5f, 0.5f, 0.5f));
                blasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(false, false, false, false, false, isWater: true);

                rtInstances.AddTlasBuild(instanceData);
                rtInstances.AddShaderTableBinder(Bind);
            });
        }

        public void Dispose()
        {
            primaryHitShaderFactory.TryReturn(primaryHitShader);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(instanceData);
        }

        public void SetTransform(in Vector3 trans, in Quaternion rot)
        {
            this.instanceData.Transform = new InstanceMatrix(trans, rot);
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            blasInstanceData.vertexOffset = meshBlas.Instance.VertexOffset;
            blasInstanceData.indexOffset = meshBlas.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &blasInstanceData)
            {
                primaryHitShader.BindSbt(instanceData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
            }
        }

        public void CreatePhysics()
        {
            
        }

        public void DestroyPhysics()
        {
            
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }
    }
}
