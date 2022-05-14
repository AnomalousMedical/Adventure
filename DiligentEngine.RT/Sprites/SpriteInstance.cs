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
            blasInstanceData.dataType = HLSL.BlasInstanceDataConstants.SpriteData;
            blasInstanceData.lightingType = HLSL.BlasInstanceDataConstants.GetShaderForDescription(spriteMaterial.NormalSRV != null, spriteMaterial.PhysicalSRV != null, spriteMaterial.Reflective, false);
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

        public unsafe void Bind(String instanceName, IShaderBindingTable sbt, ITopLevelAS tlas, TLASBuildInstanceData tlasInstanceBuildData, String currentAnimation, int frame)
        {
            var spritePlaneBLAS = blasFrames[currentAnimation][frame].Instance;
            //TODO: This is technicaly glitchy, this will be 1 frame behind, since we build the tlas before calling this function
            tlasInstanceBuildData.pBLAS = spritePlaneBLAS.BLAS.Obj;
            blasInstanceData.vertexOffset = spritePlaneBLAS.VertexOffset;
            blasInstanceData.indexOffset = spritePlaneBLAS.IndexOffset;
            fixed (HLSL.BlasInstanceData* ptr = &this.blasInstanceData)
            {
                primaryHitShader.BindSbt(instanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(HLSL.BlasInstanceData));
            }
        }

        internal void InitFrame(TLASBuildInstanceData tlasInstanceBuildData, String currentAnimation, int frame)
        {
            var spritePlaneBLAS = blasFrames[currentAnimation][frame].Instance;
            tlasInstanceBuildData.pBLAS = spritePlaneBLAS.BLAS.Obj;
        }
    }
}
