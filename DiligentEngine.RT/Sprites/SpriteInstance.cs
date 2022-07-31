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
        private readonly SpritePlaneBLAS spritePlaneBLAS;
        private PrimaryHitShader primaryHitShader;
        private readonly PrimaryHitShader.Factory primaryHitShaderFactory;
        private HLSL.BlasInstanceData blasInstanceData;

        public SpriteInstance
        (
            PrimaryHitShader primaryHitShader,
            PrimaryHitShader.Factory primaryHitShaderFactory,
            SpriteMaterial spriteMaterial,
            ISpriteMaterialManager spriteMaterialManager,
            ActiveTextures activeTextures,
            SpritePlaneBLAS spritePlaneBLAS
        )
        {
            this.primaryHitShader = primaryHitShader;
            this.primaryHitShaderFactory = primaryHitShaderFactory;
            this.spriteMaterial = spriteMaterial;
            this.spriteMaterialManager = spriteMaterialManager;
            this.activeTextures = activeTextures;
            this.spritePlaneBLAS = spritePlaneBLAS;
            blasInstanceData = this.activeTextures.AddActiveTexture(spriteMaterial);
            blasInstanceData.dispatchType = HLSL.BlasInstanceDataConstants.GetShaderForDescription(spriteMaterial.NormalSRV != null, spriteMaterial.PhysicalSRV != null, spriteMaterial.Reflective, false, true);
        }

        public void Dispose()
        {
            this.activeTextures.RemoveActiveTexture(spriteMaterial);
            primaryHitShaderFactory.TryReturn(primaryHitShader);
            spriteMaterialManager.Return(spriteMaterial);
        }

        /// <summary>
        /// This will happen before the bind steps and sets up the tlas instance
        /// build data for this frame. This actually happens during update sprites,
        /// but the user will call that before render.
        /// </summary>
        internal void UpdateBlas(TLASInstanceData tlasInstanceData)
        {
            tlasInstanceData.pBLAS = spritePlaneBLAS.Instance.BLAS.Obj;
        }

        /// <summary>
        /// This happens after the tlas is created and binds the instance data into it.
        /// </summary>
        public unsafe void Bind(String instanceName, IShaderBindingTable sbt, ITopLevelAS tlas, ISprite sprite)
        {
            String currentAnimation = sprite.CurrentAnimationName;
            var frame = sprite.GetCurrentFrame();
            blasInstanceData.vertexOffset = spritePlaneBLAS.Instance.VertexOffset;
            blasInstanceData.indexOffset = spritePlaneBLAS.Instance.IndexOffset;
            blasInstanceData.u1 = frame.Right; blasInstanceData.v1 = frame.Top;
            blasInstanceData.u2 = frame.Left; blasInstanceData.v2 = frame.Top;
            blasInstanceData.u3 = frame.Left; blasInstanceData.v3 = frame.Bottom;
            blasInstanceData.u4 = frame.Right; blasInstanceData.v4 = frame.Bottom;

            fixed (HLSL.BlasInstanceData* ptr = &this.blasInstanceData)
            {
                primaryHitShader.BindSbt(instanceName, sbt, tlas, new IntPtr(ptr), (uint)sizeof(HLSL.BlasInstanceData));
            }
        }
    }
}
