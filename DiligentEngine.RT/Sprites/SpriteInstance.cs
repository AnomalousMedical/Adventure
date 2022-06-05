using DiligentEngine;
using DiligentEngine.RT.Resources;
using DiligentEngine.RT.ShaderSets;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Sprites
{
    public class SpriteInstance : IDisposable
    {
        private readonly SpriteMaterial spriteMaterial;
        private readonly ISpriteMaterialManager spriteMaterialManager;
        private readonly ActiveTextures activeTextures;
        private readonly SpritePlaneBLAS.Factory spriteBlasFactory;
        private readonly Dictionary<String, List<SpritePlaneBLAS>> blasFrames;
        private PrimaryHitShader primaryHitShader;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private HLSL.BlasInstanceData blasInstanceData;

        public SpriteInstance
        (
            Dictionary<String, List<SpritePlaneBLAS>> blasFrames,
            PrimaryHitShader primaryHitShader,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            SpriteMaterial spriteMaterial,
            ISpriteMaterialManager spriteMaterialManager,
            ActiveTextures activeTextures,
            SpritePlaneBLAS.Factory spriteBlasFactory
        )
        {
            this.blasFrames = blasFrames;
            this.primaryHitShader = primaryHitShader;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.spriteMaterial = spriteMaterial;
            this.spriteMaterialManager = spriteMaterialManager;
            this.activeTextures = activeTextures;
            this.spriteBlasFactory = spriteBlasFactory;
            blasInstanceData = this.activeTextures.AddActiveTexture(spriteMaterial);
            blasInstanceData.dispatchType = HLSL.BlasInstanceDataConstants.GetShaderForDescription(spriteMaterial.NormalSRV != null, spriteMaterial.PhysicalSRV != null, spriteMaterial.Reflective, false, true);
        }

        public void Dispose()
        {
            this.activeTextures.RemoveActiveTexture(spriteMaterial);
            primaryHitShaderFactory.TryReturn(primaryHitShader);
            spriteMaterialManager.Return(spriteMaterial);
            foreach(var animation in blasFrames.Values)
            {
                foreach(var frame in animation)
                {
                    spriteBlasFactory.TryReturn(frame);
                }
            }
        }

        /// <summary>
        /// This will happen before the bind steps and sets up the tlas instance
        /// build data for this frame. This actually happens during update sprites,
        /// but the user will call that before render.
        /// </summary>
        internal void UpdateBlas(TLASBuildInstanceData tlasInstanceBuildData, ISprite sprite)
        {
            String currentAnimation = sprite.CurrentAnimationName;
            int frame = sprite.FrameIndex;
            var spritePlaneBLAS = blasFrames[currentAnimation][frame].Instance;
            tlasInstanceBuildData.pBLAS = spritePlaneBLAS.BLAS.Obj;
        }

        /// <summary>
        /// This happens after the tlas is created and binds the instance data into it.
        /// </summary>
        public unsafe void Bind(String instanceName, IShaderBindingTable sbt, ITopLevelAS tlas, ISprite sprite)
        {
            String currentAnimation = sprite.CurrentAnimationName;
            int frame = sprite.FrameIndex;
            var spritePlaneBLAS = blasFrames[currentAnimation][frame].Instance;
            blasInstanceData.vertexOffset = spritePlaneBLAS.VertexOffset;
            blasInstanceData.indexOffset = spritePlaneBLAS.IndexOffset;
            fixed (HLSL.BlasInstanceData* ptr = &this.blasInstanceData)
            {
                primaryHitShader.BindSbt(instanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(HLSL.BlasInstanceData));
            }
        }
    }
}
