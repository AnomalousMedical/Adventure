﻿using DiligentEngine.RT.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.ShaderSets
{
    public class GeneralShaders : IDisposable
    {
        private AutoPtr<IShader> pRayGen;
        private AutoPtr<IShader> pPrimaryMiss;
        private AutoPtr<IShader> pShadowMiss;

        private RayTracingGeneralShaderGroup rayGenShaderGroup;
        private RayTracingGeneralShaderGroup primaryMissShaderGroup;
        private RayTracingGeneralShaderGroup shadowMissShaderGroup;
        private readonly GraphicsEngine graphicsEngine;
        private readonly ShaderLoader shaderLoader;
        private readonly ActiveTextures activeTextures;
        private readonly RTCameraAndLight cameraAndLight;
        private readonly RayTracingRenderer renderer;

        public GeneralShaders(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader, ActiveTextures activeTextures, RTCameraAndLight cameraAndLight, RayTracingRenderer renderer, string textureVarName, string textureSetsVarName)
        {
            this.graphicsEngine = graphicsEngine;
            this.shaderLoader = shaderLoader;
            this.activeTextures = activeTextures;
            this.cameraAndLight = cameraAndLight;
            this.renderer = renderer;
        
            var shaderVars = new Dictionary<string, string>()
            {
                { "NUM_TEXTURES", activeTextures.MaxTextures.ToString() },
                { "G_TEXTURES", textureVarName },
                { "G_TEXTURESETS", textureSetsVarName },
            };

            var m_pDevice = graphicsEngine.RenderDevice;

            // Define shader macros
            ShaderMacroHelper Macros = new ShaderMacroHelper();
            Macros.AddShaderMacro("NUM_LIGHTS", cameraAndLight.NumLights);

            ShaderCreateInfo ShaderCI = new ShaderCreateInfo();
            // We will not be using combined texture samplers as they
            // are only required for compatibility with OpenGL, and ray
            // tracing is not supported in OpenGL backend.
            ShaderCI.UseCombinedTextureSamplers = false;

            // Only new DXC compiler can compile HLSL ray tracing shaders.
            ShaderCI.ShaderCompiler = SHADER_COMPILER.SHADER_COMPILER_DXC;

            // Shader model 6.3 is required for DXR 1.0, shader model 6.5 is required for DXR 1.1 and enables additional features.
            // Use 6.3 for compatibility with DXR 1.0 and VK_NV_ray_tracing.
            ShaderCI.HLSLVersion = new ShaderVersion { Major = 6, Minor = 5 };
            ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;

            // Create ray generation shader.
            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_GEN;
            ShaderCI.Desc.Name = "Ray tracing RG";
            ShaderCI.Source = shaderLoader.LoadShader("assets/RayTrace.hlsl");
            ShaderCI.EntryPoint = "main";
            pRayGen = m_pDevice.CreateShader(ShaderCI, Macros)
                ?? throw new InvalidOperationException($"Could not create '{ShaderCI.Desc.Name}'");

            // Create miss shaders.
            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_MISS;
            ShaderCI.Desc.Name = "Primary ray miss shader";
            ShaderCI.Source = shaderLoader.LoadShader(shaderVars, "assets/PrimaryMiss.hlsl");
            ShaderCI.EntryPoint = "main";
            pPrimaryMiss = m_pDevice.CreateShader(ShaderCI, Macros)
                ?? throw new InvalidOperationException($"Could not create '{ShaderCI.Desc.Name}'");

            ShaderCI.Desc.Name = "Shadow ray miss shader";
            ShaderCI.Source = shaderLoader.LoadShader("assets/ShadowMiss.hlsl");
            ShaderCI.EntryPoint = "main";
            pShadowMiss = m_pDevice.CreateShader(ShaderCI, Macros)
                ?? throw new InvalidOperationException($"Could not create '{ShaderCI.Desc.Name}'");

            // Ray generation shader is an entry point for a ray tracing pipeline.
            rayGenShaderGroup = new RayTracingGeneralShaderGroup { Name = "Main", pShader = pRayGen.Obj };
            // Primary ray miss shader.
            primaryMissShaderGroup = new RayTracingGeneralShaderGroup { Name = "PrimaryMiss", pShader = pPrimaryMiss.Obj };
            // Shadow ray miss shader.
            shadowMissShaderGroup = new RayTracingGeneralShaderGroup { Name = "ShadowMiss", pShader = pShadowMiss.Obj };

            renderer.OnSetupCreateInfo += Renderer_OnSetupCreateInfo;
        }

        private void Renderer_OnSetupCreateInfo(RayTracingPipelineStateCreateInfo PSOCreateInfo)
        {
            PSOCreateInfo.pGeneralShaders.Add(rayGenShaderGroup);
            PSOCreateInfo.pGeneralShaders.Add(primaryMissShaderGroup);
            PSOCreateInfo.pGeneralShaders.Add(shadowMissShaderGroup);
        }

        public void Dispose()
        {
            renderer.OnSetupCreateInfo -= Renderer_OnSetupCreateInfo;
            pRayGen.Dispose();
            pPrimaryMiss.Dispose();
            pShadowMiss.Dispose();
        }
    }
}
