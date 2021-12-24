﻿using DiligentEngine.RT.Resources;
using DiligentEngine.RT.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.ShaderSets
{
    public enum PrimaryHitShaderType
    {
        Cube,
        Sprite
    }

    public class PrimaryHitShader : IDisposable
    {
        public class Desc
        {
            public String baseName { get; set; }
            public int numTextures { get; set; }
            public PrimaryHitShaderType shaderType { get; set; }
        }

        public class Factory
        {
            private readonly GraphicsEngine graphicsEngine;
            private readonly ShaderLoader<RTShaders> shaderLoader;
            private readonly RayTracingRenderer rayTracingRenderer;

            public Factory(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader, RayTracingRenderer rayTracingRenderer)
            {
                this.graphicsEngine = graphicsEngine;
                this.shaderLoader = shaderLoader;
                this.rayTracingRenderer = rayTracingRenderer;
            }

            /// <summary>
            /// Create a shader. The caller is responsible for disposing the instance.
            /// </summary>
            /// <param name="baseName"></param>
            /// <param name="numTextures"></param>
            /// <returns></returns>
            public async Task<PrimaryHitShader> Create(String baseName, int numTextures, PrimaryHitShaderType shaderType)
            {
                var shader = new PrimaryHitShader();
                await shader.SetupShaders(baseName, numTextures, graphicsEngine, shaderLoader, rayTracingRenderer, shaderType);
                return shader;
            }
        }

        private RayTracingPipelineStateCreateInfo PSOCreateInfo;
        private AutoPtr<IShader> pCubePrimaryHit;
        private AutoPtr<IShader> pCubeAnyHit;
        private RayTracingTriangleHitShaderGroup primaryHitShaderGroup;

        private ShaderResourceVariableDesc verticesDesc;
        private ShaderResourceVariableDesc indicesDesc;
        private int numTextures;

        private String verticesName;
        private String indicesName;
        private String colorTexturesName;
        private String normalTexturesName;
        private String shaderGroupName;

        public String ShaderGroupName => shaderGroupName;

        public PrimaryHitShader()
        {      
        }

        private async Task SetupShaders(String baseName, int numTextures, GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader, RayTracingRenderer rayTracingRenderer, PrimaryHitShaderType shaderType)
        {
            this.PSOCreateInfo = rayTracingRenderer.PSOCreateInfo;
            this.numTextures = numTextures;
            await Task.Run(() =>
            {
                this.verticesName = $"vert_{baseName}";
                this.indicesName = $"idx_{baseName}";
                this.colorTexturesName = $"tex_{baseName}";
                this.normalTexturesName = $"nrmltex_{baseName}";
                this.shaderGroupName = $"{baseName}PrimaryHit";

                var m_pDevice = graphicsEngine.RenderDevice;

                // Define shader macros
                ShaderMacroHelper Macros = new ShaderMacroHelper();

                ShaderCreateInfo ShaderCI = new ShaderCreateInfo();
                // We will not be using combined texture samplers as they
                // are only required for compatibility with OpenGL, and ray
                // tracing is not supported in OpenGL backend.
                ShaderCI.UseCombinedTextureSamplers = false;

                // Only new DXC compiler can compile HLSL ray tracing shaders.
                ShaderCI.ShaderCompiler = SHADER_COMPILER.SHADER_COMPILER_DXC;

                // Shader model 6.3 is required for DXR 1.0, shader model 6.5 is required for DXR 1.1 and enables additional features.
                // Use 6.3 for compatibility with DXR 1.0 and VK_NV_ray_tracing.
                ShaderCI.HLSLVersion = new ShaderVersion { Major = 6, Minor = 3 };
                ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;

                var shaderVars = new Dictionary<string, string>()
                {
                    { "NUM_TEXTURES", numTextures.ToString() },
                    { "VERTICES", verticesName },
                    { "INDICES", indicesName },
                    { "COLOR_TEXTURES", colorTexturesName },
                    { "NORMAL_TEXTURES", normalTexturesName }
                };

                // Create closest hit shaders.
                var textureSuffix = numTextures == 1 ? "1Texture" : "";

                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT;
                ShaderCI.Desc.Name = "Cube primary ray closest hit shader";
                ShaderCI.Source = shaderLoader.LoadShader(shaderVars, $"assets/{shaderType}PrimaryHit{textureSuffix}.hlsl");
                ShaderCI.EntryPoint = "main";
                pCubePrimaryHit = m_pDevice.CreateShader(ShaderCI, Macros);
                //VERIFY_EXPR(pCubePrimaryHit != nullptr);

                // Create any hit shaders.
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT;
                ShaderCI.Desc.Name = "Cube primary ray any hit shader";
                ShaderCI.Source = shaderLoader.LoadShader(shaderVars, $"assets/{shaderType}AnyHit{textureSuffix}.hlsl");
                ShaderCI.EntryPoint = "main";
                pCubeAnyHit = m_pDevice.CreateShader(ShaderCI, Macros);
                //VERIFY_EXPR(pCubeAnyHit != nullptr);

                // Primary ray hit group for the textured cube.
                primaryHitShaderGroup = new RayTracingTriangleHitShaderGroup { Name = shaderGroupName, pClosestHitShader = pCubePrimaryHit.Obj, pAnyHitShader = pCubeAnyHit.Obj };

                verticesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, Name = verticesName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
                indicesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, Name = indicesName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
            });

            //Do these back on the main thread since they change the state of the renderer
            PSOCreateInfo.pTriangleHitShaders.Add(primaryHitShaderGroup);

            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(verticesDesc);
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(indicesDesc);
        }

        public void Dispose()
        {
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Remove(indicesDesc);
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Remove(verticesDesc);

            PSOCreateInfo.pTriangleHitShaders.Remove(primaryHitShaderGroup);

            pCubeAnyHit?.Dispose();
            pCubePrimaryHit?.Dispose();
        }

        public void BindBlas(BLASInstance bLASInstance, IShaderResourceBinding rayTracingSRB)
        {
            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, verticesName).Set(bLASInstance.AttrVertexBuffer.Obj.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));
            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, indicesName).Set(bLASInstance.IndexBuffer.Obj.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));

            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, verticesName)?.Set(bLASInstance.AttrVertexBuffer.Obj.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));
            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, indicesName)?.Set(bLASInstance.IndexBuffer.Obj.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));
        }

        public void BindTextures(IShaderResourceBinding m_pRayTracingSRB, TextureSet textureSet)
        {
            m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, colorTexturesName)?.SetArray(textureSet.TexSRVs);
            m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, normalTexturesName)?.SetArray(textureSet.TexNormalSRVs);

            m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, colorTexturesName)?.SetArray(textureSet.TexSRVs);
        }

        public void BindTextures(IShaderResourceBinding m_pRayTracingSRB, SpriteMaterial spriteMaterial)
        {
            m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, colorTexturesName)?.Set(spriteMaterial.ColorSRV);
            m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, normalTexturesName)?.Set(spriteMaterial.NormalSRV);

            m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, colorTexturesName)?.Set(spriteMaterial.ColorSRV);
        }

        public void BindTextures(IShaderResourceBinding m_pRayTracingSRB, CC0TextureResult textureSet)
        {
            if (numTextures == 1)
            {
                m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, colorTexturesName)?.Set(textureSet.BaseColorSRVs[0]);
                m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, normalTexturesName)?.Set(textureSet.NormalMapSRVs[0]);

                m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, colorTexturesName)?.Set(textureSet.BaseColorSRVs[0]);
            }
            else
            {
                m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, colorTexturesName)?.SetArray(textureSet.BaseColorSRVs);
                m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, normalTexturesName)?.SetArray(textureSet.NormalMapSRVs);

                m_pRayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, colorTexturesName)?.SetArray(textureSet.BaseColorSRVs);
            }
        }
    }
}
