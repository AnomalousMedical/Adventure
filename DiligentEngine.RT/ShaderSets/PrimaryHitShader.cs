﻿using DiligentEngine.RT.Resources;
using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.ShaderSets
{
    public enum PrimaryHitShaderType
    {
        Mesh,
        Sprite
    }

    public class PrimaryHitShader : IDisposable
    {
        public class Factory
        {
            private readonly PooledResourceManager<object, PrimaryHitShader> pooledResources
                = new PooledResourceManager<object, PrimaryHitShader>();
            private readonly object key = new object(); //Only 1 item goes in this pooled resource manager, so just declare its key here

            private readonly GraphicsEngine graphicsEngine;
            private readonly ShaderLoader<RTShaders> shaderLoader;
            private readonly RayTracingRenderer rayTracingRenderer;
            private readonly RTCameraAndLight cameraAndLight;
            private readonly BLASBuilder blasBuilder;
            private readonly ActiveTextures activeTextures;

            public Factory
            (
                GraphicsEngine graphicsEngine, 
                ShaderLoader<RTShaders> shaderLoader, 
                RayTracingRenderer rayTracingRenderer, 
                RTCameraAndLight cameraAndLight,
                BLASBuilder blasBuilder,
                ActiveTextures activeTextures
            )
            {
                this.graphicsEngine = graphicsEngine;
                this.shaderLoader = shaderLoader;
                this.rayTracingRenderer = rayTracingRenderer;
                this.cameraAndLight = cameraAndLight;
                this.blasBuilder = blasBuilder;
                this.activeTextures = activeTextures;
            }

            /// <summary>
            /// Create a shader. The caller is responsible for calling return.
            /// </summary>
            /// <param name="baseName"></param>
            /// <param name="numTextures"></param>
            /// <returns></returns>
            public Task<PrimaryHitShader> Checkout()
            {
                return pooledResources.Checkout(key, async () =>
                {
                    var shader = new PrimaryHitShader(activeTextures, rayTracingRenderer, blasBuilder);
                    await shader.SetupShaders(graphicsEngine, shaderLoader, cameraAndLight);
                    return pooledResources.CreateResult(shader);
                });
            }

            public void TryReturn(PrimaryHitShader item)
            {
                if (item != null)
                {
                    pooledResources.Return(item);
                }
            }
        }

        private AutoPtr<IShader> pCubePrimaryHit;
        private AutoPtr<IShader> pCubeAnyHit;
        private RayTracingTriangleHitShaderGroup primaryHitShaderGroup;
        private RayTracingRenderer renderer;
        private BLASBuilder builder;

        private ShaderResourceVariableDesc verticesDesc;
        private ShaderResourceVariableDesc indicesDesc;
        private ShaderResourceVariableDesc texturesDesc;
        private int numTextures;

        private String TextureVarName = "g_textures";
        private String VerticesVarName = "g_vertices";
        private String IndicesVarName = "g_indices";

        private readonly ActiveTextures activeTextures;
        private String shaderGroupName;

        public PrimaryHitShader(ActiveTextures activeTextures, RayTracingRenderer renderer, BLASBuilder builder)
        {
            this.builder = builder;
            this.renderer = renderer;
            this.activeTextures = activeTextures;
        }

        private async Task SetupShaders(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader, RTCameraAndLight cameraAndLight)
        {
            var id = RTId.CreateId("PrimaryHitShaderVariable").Replace("-", "");

            TextureVarName = TextureVarName + id;
            VerticesVarName = VerticesVarName + id;
            IndicesVarName = IndicesVarName + id;

            this.numTextures = activeTextures.MaxTextures;

            await Task.Run(() =>
            {
                this.shaderGroupName = $"{Guid.NewGuid()}PrimaryHit";

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
                ShaderCI.HLSLVersion = new ShaderVersion { Major = 6, Minor = 3 };
                ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;

                var shaderVars = new Dictionary<string, string>()
                {
                    { "NUM_TEXTURES", numTextures.ToString() },
                    { "G_TEXTURES", TextureVarName },
                    { "G_VERTICES", VerticesVarName },
                    { "G_INDICES", IndicesVarName },
                    { "MESH_DATA_TYPE", HLSL.BlasInstanceDataConstants.MeshData.ToString() },
                    { "SPRITE_DATA_TYPE", HLSL.BlasInstanceDataConstants.SpriteData.ToString() },
                    { "LIGHTANDSHADEBASE", HLSL.BlasInstanceDataConstants.LightAndShadeBase.ToString() },
                    { "LIGHTANDSHADEBASEEMISSIVE", HLSL.BlasInstanceDataConstants.LightAndShadeBaseEmissive.ToString() },
                    { "LIGHTANDSHADEBASENORMAL", HLSL.BlasInstanceDataConstants.LightAndShadeBaseNormal.ToString() },
                    { "LIGHTANDSHADEBASENORMALEMISSIVE", HLSL.BlasInstanceDataConstants.LightAndShadeBaseNormalEmissive.ToString() },
                    { "LIGHTANDSHADEBASENORMALPHYSICAL", HLSL.BlasInstanceDataConstants.LightAndShadeBaseNormalPhysical.ToString() },
                    { "LIGHTANDSHADEBASENORMALPHYSICALEMISSIVE", HLSL.BlasInstanceDataConstants.LightAndShadeBaseNormalPhysicalEmissive.ToString() },
                    { "LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE", HLSL.BlasInstanceDataConstants.LightAndShadeBaseNormalPhysicalReflective.ToString() },
                    { "LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVEEMISSIVE", HLSL.BlasInstanceDataConstants.LightAndShadeBaseNormalPhysicalReflectiveEmissive.ToString() },
                };

                // Create closest hit shaders.
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT;
                ShaderCI.Desc.Name = $"primary ray closest hit shader";
                ShaderCI.Source = shaderLoader.LoadShader(shaderVars, $"assets/PrimaryHit.hlsl");
                ShaderCI.EntryPoint = "main";
                pCubePrimaryHit = m_pDevice.CreateShader(ShaderCI, Macros);
                //VERIFY_EXPR(pCubePrimaryHit != nullptr);

                // Create any hit shaders.
                //TODO: Any hit is always the same and is turned on and off by the opaque flag. This can be shared between all instances.
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT;
                ShaderCI.Desc.Name = $"primary ray any hit shader";
                ShaderCI.Source = shaderLoader.LoadShader(shaderVars, $"assets/AnyHit.hlsl");
                ShaderCI.EntryPoint = "main";
                pCubeAnyHit = m_pDevice.CreateShader(ShaderCI, Macros);
                //VERIFY_EXPR(pCubeAnyHit != nullptr);

                // Primary ray hit group for the textured cube.
                primaryHitShaderGroup = new RayTracingTriangleHitShaderGroup { Name = shaderGroupName, pClosestHitShader = pCubePrimaryHit.Obj, pAnyHitShader = pCubeAnyHit.Obj };

                verticesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT | SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, Name = VerticesVarName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
                indicesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT | SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, Name = IndicesVarName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
                texturesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT | SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, Name = TextureVarName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
            });

            renderer.AddShaderResourceBinder(Bind);
            renderer.OnSetupCreateInfo += Renderer_OnSetupCreateInfo;
        }

        public void Dispose()
        {
            renderer.OnSetupCreateInfo -= Renderer_OnSetupCreateInfo;
            renderer.RemoveShaderResourceBinder(Bind);

            pCubeAnyHit?.Dispose();
            pCubePrimaryHit?.Dispose();
        }

        private void Renderer_OnSetupCreateInfo(RayTracingPipelineStateCreateInfo PSOCreateInfo)
        {
            PSOCreateInfo.pTriangleHitShaders.Add(primaryHitShaderGroup);
            //TODO: Adding this to the triangle hit shaders here assumes the BLAS is already created. This is setup to work ok now, but hopefully this can be unbound later

            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(verticesDesc);
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(indicesDesc);
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(texturesDesc);
        }

        public void BindSbt(String instanceName, IShaderBindingTable sbt, ITopLevelAS tlas, IntPtr data, uint size)
        {
            sbt.BindHitGroupForInstance(tlas, instanceName, RtStructures.PRIMARY_RAY_INDEX, shaderGroupName, data, size);
        }

        private void Bind(IShaderResourceBinding rayTracingSRB)
        {
            if (builder.AttrBuffer != null)
            {
                rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, VerticesVarName).Set(builder.AttrBuffer.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));
                rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, IndicesVarName).Set(builder.IndexBuffer.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));

                rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, VerticesVarName)?.Set(builder.AttrBuffer.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));
                rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, IndicesVarName)?.Set(builder.IndexBuffer.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));
            }

            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, TextureVarName)?.SetArray(activeTextures.Textures);
            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, TextureVarName)?.SetArray(activeTextures.Textures);
        }
    }
}
