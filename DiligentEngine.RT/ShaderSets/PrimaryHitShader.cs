using DiligentEngine.RT.Resources;
using Engine;
using System;
using System.Collections.Generic;
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
        private AutoPtr<IShader> pShadowAnyHit;
        private RayTracingRenderer renderer;
        private BLASBuilder builder;

        private ShaderResourceVariableDesc verticesDesc;
        private ShaderResourceVariableDesc indicesDesc;
        private ShaderResourceVariableDesc texturesDesc;
        private ShaderResourceVariableDesc textureSetsDesc;
        private int numTextures;

        private String TextureVarName = "g_textures";
        private String TextureSetsVarName = "g_texturesets";
        private String VerticesVarName = "g_vertices";
        private String IndicesVarName = "g_indices";

        private readonly ActiveTextures activeTextures;

        private String primaryShaderGroupName;
        private String shadowShaderGroupName;
        private RayTracingTriangleHitShaderGroup primaryHitShaderGroup;
        private RayTracingTriangleHitShaderGroup shadowHitShaderGroup;

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
            TextureSetsVarName = TextureSetsVarName + id;
            VerticesVarName = VerticesVarName + id;
            IndicesVarName = IndicesVarName + id;

            this.numTextures = activeTextures.MaxTextures;

            await Task.Run(() =>
            {
                this.primaryShaderGroupName = $"{Guid.NewGuid()}PrimaryHit";
                this.shadowShaderGroupName = $"{Guid.NewGuid()}ShadowHit";

                var m_pDevice = graphicsEngine.RenderDevice;

                // Define shader macros
                ShaderMacroHelper Macros = new ShaderMacroHelper();
                Macros.AddShaderMacro("NUM_LIGHTS", cameraAndLight.NumLights);
                Macros.AddShaderMacro("MAX_DISPERS_SAMPLES", 16);

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

                var shaderVars = new Dictionary<string, string>()
                {
                    { "NUM_TEXTURES", numTextures.ToString() },
                    { "G_TEXTURES", TextureVarName },
                    { "G_TEXTURESETS", TextureSetsVarName },
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
                    { "GLASSMATERIAL", HLSL.BlasInstanceDataConstants.Glass.ToString() },
                    { "WATERMATERIAL", HLSL.BlasInstanceDataConstants.Water.ToString() },
                };

                // Create closest hit shaders.
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT;
                ShaderCI.Desc.Name = $"primary ray closest hit shader";
                ShaderCI.Source = shaderLoader.LoadShader(shaderVars, $"assets/PrimaryHit.hlsl");
                ShaderCI.EntryPoint = "main";
                pCubePrimaryHit = m_pDevice.CreateShader(ShaderCI, Macros)
                  ?? throw new InvalidOperationException($"Could not create '{ShaderCI.Desc.Name}'");

                // Create primary any hit shaders.
                Macros.AddShaderMacro("PRIMARY_HIT", 1);
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT;
                ShaderCI.Desc.Name = $"primary ray any hit shader";
                ShaderCI.Source = shaderLoader.LoadShader(shaderVars, $"assets/AnyHit.hlsl");
                ShaderCI.EntryPoint = "main";
                pCubeAnyHit = m_pDevice.CreateShader(ShaderCI, Macros)
                  ?? throw new InvalidOperationException($"Could not create '{ShaderCI.Desc.Name}'");

                Macros.RemoveMacro("PRIMARY_HIT");
                Macros.AddShaderMacro("SHADOW_HIT", 1);
                ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT;
                ShaderCI.Desc.Name = $"shadow ray any hit shader";
                ShaderCI.Source = shaderLoader.LoadShader(shaderVars, $"assets/AnyHit.hlsl");
                ShaderCI.EntryPoint = "main";
                pShadowAnyHit = m_pDevice.CreateShader(ShaderCI, Macros)
                  ?? throw new InvalidOperationException($"Could not create '{ShaderCI.Desc.Name}'");

                // Primary ray hit group for the textured cube.
                primaryHitShaderGroup = new RayTracingTriangleHitShaderGroup { Name = primaryShaderGroupName, pClosestHitShader = pCubePrimaryHit.Obj, pAnyHitShader = pCubeAnyHit.Obj };
                shadowHitShaderGroup = new RayTracingTriangleHitShaderGroup { Name = shadowShaderGroupName, pClosestHitShader = pCubePrimaryHit.Obj, pAnyHitShader = pShadowAnyHit.Obj };

                //TODO: Remove SHADER_TYPE.SHADER_TYPE_RAY_GEN below, seems you don't need it, but needs testing
                verticesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT | SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, Name = VerticesVarName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
                indicesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT | SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, Name = IndicesVarName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
                texturesDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT | SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, Name = TextureVarName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
                textureSetsDesc = new ShaderResourceVariableDesc { ShaderStages = SHADER_TYPE.SHADER_TYPE_RAY_GEN | SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT | SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, Name = TextureSetsVarName, Type = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_DYNAMIC };
            });

            renderer.AddShaderResourceBinder(Bind);
            renderer.AddTlasShaderResourceBinder(BindTlas);
            renderer.OnSetupCreateInfo += Renderer_OnSetupCreateInfo;
        }

        public void Dispose()
        {
            renderer.OnSetupCreateInfo -= Renderer_OnSetupCreateInfo;
            renderer.RemoveTlasShaderResourceBinder(BindTlas);
            renderer.RemoveShaderResourceBinder(Bind);

            pShadowAnyHit?.Dispose();
            pCubeAnyHit?.Dispose();
            pCubePrimaryHit?.Dispose();
        }

        private void Renderer_OnSetupCreateInfo(RayTracingPipelineStateCreateInfo PSOCreateInfo)
        {
            PSOCreateInfo.pTriangleHitShaders.Add(primaryHitShaderGroup);
            PSOCreateInfo.pTriangleHitShaders.Add(shadowHitShaderGroup);
            //TODO: Adding this to the triangle hit shaders here assumes the BLAS is already created. This is setup to work ok now, but hopefully this can be unbound later

            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(verticesDesc);
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(indicesDesc);
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(texturesDesc);
            PSOCreateInfo.PSODesc.ResourceLayout.Variables.Add(textureSetsDesc);
        }

        public void BindSbt(String instanceName, IShaderBindingTable sbt, ITopLevelAS tlas, IntPtr data, uint size)
        {
            sbt.BindHitGroupForInstance(tlas, instanceName, RtStructures.PRIMARY_RAY_INDEX, primaryShaderGroupName, data, size);
            sbt.BindHitGroupForInstance(tlas, instanceName, RtStructures.SHADOW_RAY_INDEX, shadowShaderGroupName, data, size);
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

            activeTextures.UpdateSharedBuffers();
            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, TextureSetsVarName).Set(activeTextures.TexSetBuffer.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));
            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, TextureSetsVarName)?.Set(activeTextures.TexSetBuffer.GetDefaultView(BUFFER_VIEW_TYPE.BUFFER_VIEW_SHADER_RESOURCE));

            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_CLOSEST_HIT, TextureVarName)?.SetArray(activeTextures.Textures);
            rayTracingSRB.GetVariableByName(SHADER_TYPE.SHADER_TYPE_RAY_ANY_HIT, TextureVarName)?.SetArray(activeTextures.Textures);
        }

        private void BindTlas(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            sbt.BindHitGroupForTLAS(tlas, RtStructures.SHADOW_RAY_INDEX, shadowShaderGroupName, IntPtr.Zero, 0);
        }
    }
}
