using Adventure.Exploration.Menu;
using Adventure.Services;
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

namespace Adventure.WorldMap
{
    class Airship : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            
        }

        private readonly TLASInstanceData instanceData;
        private readonly CubeBLAS cubeBLAS;
        private readonly RTInstances<IWorldMapGameState> rtInstances;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private readonly RayTracingRenderer renderer;
        private readonly TextureManager textureManager;
        private readonly ActiveTextures activeTextures;
        private PrimaryHitShader primaryHitShader;
        private CC0TextureResult cubeTexture;
        private BlasInstanceData blasInstanceData;
        private readonly IBepuScene<IWorldMapGameState> bepuScene;
        private readonly IContextMenu contextMenu;
        private readonly ICollidableTypeIdentifier<IWorldMapGameState> collidableIdentifier;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public Airship
        (
            Description description,
            CubeBLAS cubeBLAS,
            IScopedCoroutine coroutine,
            RTInstances<IWorldMapGameState> rtInstances,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            RayTracingRenderer renderer,
            TextureManager textureManager,
            ActiveTextures activeTextures,
            Persistence persistence,
            ICollidableTypeIdentifier<IWorldMapGameState> collidableIdentifier,
            IBepuScene<IWorldMapGameState> bepuScene,
            IContextMenu contextMenu
        )
        {
            var scale = description.Scale;
            var halfScale = scale.y / 2f;
            var startPos = persistence.Current.Player.WorldPosition ?? description.Translation + new Vector3(0f, halfScale, 0f);

            this.currentPosition = startPos;
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

            this.cubeBLAS = cubeBLAS;
            this.rtInstances = rtInstances;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.renderer = renderer;
            this.textureManager = textureManager;
            this.activeTextures = activeTextures;
            this.collidableIdentifier = collidableIdentifier;
            this.bepuScene = bepuScene;
            this.contextMenu = contextMenu;
            this.instanceData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Airship"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                this.cubeTexture = await textureManager.Checkout(new CCOTextureBindingDescription("Graphics/Textures/AmbientCG/Metal032_1K", reflective: true));

                var primaryHitShaderTask = primaryHitShaderFactory.Checkout();

                await Task.WhenAll
                (
                    cubeBLAS.WaitForLoad(),
                    primaryHitShaderTask
                );

                this.instanceData.pBLAS = cubeBLAS.Instance.BLAS.Obj;
                this.primaryHitShader = primaryHitShaderTask.Result;
                blasInstanceData = this.activeTextures.AddActiveTexture(this.cubeTexture);
                blasInstanceData.dispatchType = BlasInstanceDataConstants.GetShaderForDescription(cubeTexture.NormalMapSRV != null, cubeTexture.PhysicalDescriptorMapSRV != null, cubeTexture.Reflective, cubeTexture.EmissiveSRV != null, false);
                rtInstances.AddTlasBuild(instanceData);
                rtInstances.AddShaderTableBinder(Bind);
            });
        }

        public void Dispose()
        {
            this.activeTextures.RemoveActiveTexture(this.cubeTexture);
            primaryHitShaderFactory.TryReturn(primaryHitShader);
            textureManager.TryReturn(cubeTexture);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(instanceData);
        }

        public void SetTransform(in Vector3 trans, in Quaternion rot)
        {
            var hasPhysics = physicsCreated;
            if (hasPhysics)
            {
                DestroyPhysics();
            }
            this.instanceData.Transform = new InstanceMatrix(trans, rot);
            if (hasPhysics)
            {
                CreatePhysics();
            }
        }

        private unsafe void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            blasInstanceData.vertexOffset = cubeBLAS.Instance.VertexOffset;
            blasInstanceData.indexOffset = cubeBLAS.Instance.IndexOffset;
            fixed (BlasInstanceData* ptr = &blasInstanceData)
            {
                primaryHitShader.BindSbt(instanceData.InstanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(BlasInstanceData));
            }
        }

        public void CreatePhysics()
        {
            if (!physicsCreated)
            {
                physicsCreated = true;
                var shape = new Box(0.25f, 1000, 0.25f); //TODO: Each one creates its own, try to load from resources
                shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

                staticHandle = bepuScene.Simulation.Statics.Add(
                    new StaticDescription(
                        currentPosition.ToSystemNumerics(),
                        Quaternion.Identity.ToSystemNumerics(),
                        new CollidableDescription(shapeIndex, 0.1f)));

                bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), collisionEvent: HandleCollision, endEvent: HandleCollisionEnd);
            }
        }

        public void DestroyPhysics()
        {
            if (physicsCreated)
            {
                physicsCreated = false;
                bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
                bepuScene.Simulation.Shapes.Remove(shapeIndex);
                bepuScene.Simulation.Statics.Remove(staticHandle);
            }
        }

        private void HandleCollision(CollisionEvent evt)
        {
            if (collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.A, out var player)
               || collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.B, out player))
            {
                contextMenu.HandleContext("Enter", Enter, player.GamepadId);
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Enter);
        }

        private void Enter(ContextMenuArgs args)
        {
            contextMenu.ClearContext(Enter);
            //worldMapGameState.RequestZone(zoneIndex);
        }
    }
}
