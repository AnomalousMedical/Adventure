﻿using Anomalous.OSPlatform;
using DiligentEngine;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngineCube
{
    class CubeUpdateListener : UpdateListener, IDisposable
    {
        private readonly GraphicsEngine graphicsEngine;
        private readonly NativeOSWindow window;
        private readonly ISwapChain swapChain;
        private readonly IDeviceContext m_pImmediateContext;

        private AutoPtr<IPipelineState> pipelineState;
        private AutoPtr<IBuffer> m_VSConstants;
        private AutoPtr<IShaderResourceBinding> m_pSRB;
        private AutoPtr<IBuffer> m_CubeVertexBuffer;
        private AutoPtr<IBuffer> m_CubeIndexBuffer;

        public CubeUpdateListener(GraphicsEngine graphicsEngine, NativeOSWindow window)
        {
            this.graphicsEngine = graphicsEngine;
            this.window = window;
            this.swapChain = graphicsEngine.SwapChain;
            this.m_pImmediateContext = graphicsEngine.ImmediateContext;

            var m_pDevice = graphicsEngine.RenderDevice;
            var m_pSwapChain = graphicsEngine.SwapChain;

            var ShaderCI = new ShaderCreateInfo();
            ShaderCI.SourceLanguage = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_HLSL;
            // OpenGL backend requires emulated combined HLSL texture samplers (g_Texture + g_Texture_sampler combination)
            ShaderCI.UseCombinedTextureSamplers = true;

            // Create a vertex shader
            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_VERTEX;
            ShaderCI.EntryPoint = "main";
            ShaderCI.Desc.Name = "Cube VS";
            ShaderCI.Source = VSSource;
            using var pVS = this.graphicsEngine.RenderDevice.CreateShader(ShaderCI);

            {
                BufferDesc CBDesc = new BufferDesc();
                CBDesc.Name = "VS constants CB";
                CBDesc.Size = 64;// sizeof(float4x4);
                CBDesc.Usage = USAGE.USAGE_DYNAMIC;
                CBDesc.BindFlags = BIND_FLAGS.BIND_UNIFORM_BUFFER;
                CBDesc.CPUAccessFlags = CPU_ACCESS_FLAGS.CPU_ACCESS_WRITE;
                m_VSConstants = m_pDevice.CreateBuffer(CBDesc);
            }

            //Create pixel shader
            ShaderCI.Desc.ShaderType = SHADER_TYPE.SHADER_TYPE_PIXEL;
            ShaderCI.EntryPoint = "main";
            ShaderCI.Desc.Name = "Cube PS";
            ShaderCI.Source = PSSource;
            using var pPS = this.graphicsEngine.RenderDevice.CreateShader(ShaderCI);

            var PSOCreateInfo = new GraphicsPipelineStateCreateInfo();

            // Pipeline state name is used by the engine to report issues.
            // It is always a good idea to give objects descriptive names.
            PSOCreateInfo.PSODesc.Name = "Cube PSO";

            // This is a graphics pipeline
            PSOCreateInfo.PSODesc.PipelineType = PIPELINE_TYPE.PIPELINE_TYPE_GRAPHICS;

            // clang-format off
            // This tutorial will render to a single render target
            PSOCreateInfo.GraphicsPipeline.NumRenderTargets = 1;
            // Set render target format which is the format of the swap chain's color buffer
            PSOCreateInfo.GraphicsPipeline.RTVFormats_0 = m_pSwapChain.GetDesc_ColorBufferFormat;
            // Use the depth buffer format from the swap chain
            PSOCreateInfo.GraphicsPipeline.DSVFormat = m_pSwapChain.GetDesc_DepthBufferFormat;
            // Primitive topology defines what kind of primitives will be rendered by this pipeline state
            PSOCreateInfo.GraphicsPipeline.PrimitiveTopology = PRIMITIVE_TOPOLOGY.PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
            // Cull back faces
            PSOCreateInfo.GraphicsPipeline.RasterizerDesc.CullMode = CULL_MODE.CULL_MODE_BACK;
            // Enable depth testing
            PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.DepthEnable = true;
            // clang-format on

            // Define vertex shader input layout
            var LayoutElems = new List<LayoutElement>
            {
                // Attribute 0 - vertex position
                new LayoutElement()
                {
                    InputIndex = 0,
                    BufferSlot = 0,
                    NumComponents = 3,
                    ValueType = VALUE_TYPE.VT_FLOAT32,
                    IsNormalized = false
                },
                // Attribute 1 - vertex color
                new LayoutElement
                {
                    InputIndex = 1,
                    BufferSlot = 0,
                    NumComponents = 4,
                    ValueType = VALUE_TYPE.VT_FLOAT32,
                    IsNormalized = false
                },
            };
            // clang-format on
            PSOCreateInfo.GraphicsPipeline.InputLayout.LayoutElements = LayoutElems;

            PSOCreateInfo.pVS = pVS.Obj;
            PSOCreateInfo.pPS = pPS.Obj;

            // Define variable type that will be used by default
            PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableType = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC;


            this.pipelineState = m_pDevice.CreateGraphicsPipelineState(PSOCreateInfo);

            // Since we did not explcitly specify the type for 'Constants' variable, default
            // type (SHADER_RESOURCE_VARIABLE_TYPE_STATIC) will be used. Static variables never
            // change and are bound directly through the pipeline state object.
            pipelineState.Obj.GetStaticVariableByName(SHADER_TYPE.SHADER_TYPE_VERTEX, "Constants").Set(m_VSConstants.Obj);

            // Create a shader resource binding object and bind all static resources in it
            m_pSRB = pipelineState.Obj.CreateShaderResourceBinding(true);

            CreateVertexBuffer();
            CreateIndexBuffer();
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vector3 pos;
            public Vector4 color;
        };

        unsafe void CreateVertexBuffer()
        {
            var m_pDevice = graphicsEngine.RenderDevice;

            // Layout of this structure matches the one we defined in the pipeline state

            // Cube vertices

            //      (-1,+1,+1)________________(+1,+1,+1)
            //               /|              /|
            //              / |             / |
            //             /  |            /  |
            //            /   |           /   |
            //(-1,-1,+1) /____|__________/(+1,-1,+1)
            //           |    |__________|____|
            //           |   /(-1,+1,-1) |    /(+1,+1,-1)
            //           |  /            |   /
            //           | /             |  /
            //           |/              | /
            //           /_______________|/
            //        (-1,-1,-1)       (+1,-1,-1)
            //

            // clang-format off
            var CubeVerts = new Vertex[]
            {
                new Vertex{pos = new Vector3(-1,-1,-1), color = new Vector4(1,0,0,1)},
                new Vertex{pos = new Vector3(-1,+1,-1), color = new Vector4(0,1,0,1)},
                new Vertex{pos = new Vector3(+1,+1,-1), color = new Vector4(0,0,1,1)},
                new Vertex{pos = new Vector3(+1,-1,-1), color = new Vector4(1,1,1,1)},

                new Vertex{pos = new Vector3(-1,-1,+1), color = new Vector4(1,1,0,1)},
                new Vertex{pos = new Vector3(-1,+1,+1), color = new Vector4(0,1,1,1)},
                new Vertex{pos = new Vector3(+1,+1,+1), color = new Vector4(1,0,1,1)},
                new Vertex{pos = new Vector3(+1,-1,+1), color = new Vector4(0.2f,0.2f,0.2f,1)},
            };
            // clang-format on

            // Create a vertex buffer that stores cube vertices
            BufferDesc VertBuffDesc = new BufferDesc();
            VertBuffDesc.Name = "Cube vertex buffer";
            VertBuffDesc.Usage = USAGE.USAGE_IMMUTABLE;
            VertBuffDesc.BindFlags = BIND_FLAGS.BIND_VERTEX_BUFFER;
            VertBuffDesc.Size = (uint)(sizeof(Vertex) * CubeVerts.Length);
            BufferData VBData = new BufferData();
            fixed (Vertex* vertices = CubeVerts)
            {
                VBData.pData = new IntPtr(vertices);
                VBData.DataSize = (uint)(sizeof(Vertex) * CubeVerts.Length);
                m_CubeVertexBuffer = m_pDevice.CreateBuffer(VertBuffDesc, VBData);
            }
        }

        unsafe void CreateIndexBuffer()
        {
            var m_pDevice = graphicsEngine.RenderDevice;

            // clang-format off
            var Indices = new UInt32[]
            {
                2,0,1, 2,3,0,
                4,6,5, 4,7,6,
                0,7,4, 0,3,7,
                1,0,4, 1,4,5,
                1,5,2, 5,6,2,
                3,6,7, 3,2,6
            };
            // clang-format on

            BufferDesc IndBuffDesc = new BufferDesc();
            IndBuffDesc.Name = "Cube index buffer";
            IndBuffDesc.Usage = USAGE.USAGE_IMMUTABLE;
            IndBuffDesc.BindFlags = BIND_FLAGS.BIND_INDEX_BUFFER;
            IndBuffDesc.Size = (uint)(sizeof(UInt32) * Indices.Length);
            BufferData IBData = new BufferData();
            fixed (UInt32* pIndices = Indices)
            {
                IBData.pData = new IntPtr(pIndices);
                IBData.DataSize = (uint)(sizeof(UInt32) * Indices.Length);
                m_CubeIndexBuffer = m_pDevice.CreateBuffer(IndBuffDesc, IBData);
            }
        }

        public void Dispose()
        {
            m_CubeIndexBuffer.Dispose();
            m_CubeVertexBuffer.Dispose();
            m_pSRB.Dispose();
            pipelineState.Dispose();
            m_VSConstants.Dispose();
        }

        public void exceededMaxDelta()
        {

        }

        public void loopStarting()
        {

        }

        public unsafe void sendUpdate(Clock clock)
        {
            var pRTV = swapChain.GetCurrentBackBufferRTV();
            var pDSV = swapChain.GetDepthBufferDSV();
            var preTransform = swapChain.GetDesc_PreTransform;
            m_pImmediateContext.SetRenderTarget(pRTV, pDSV, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            // Clear the back buffer
            var ClearColor = new Color(0.350f, 0.350f, 0.350f, 1.0f);

            // Clear the back buffer
            // Let the engine perform required state transitions
            m_pImmediateContext.ClearRenderTarget(pRTV, ClearColor, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);
            m_pImmediateContext.ClearDepthStencil(pDSV, CLEAR_DEPTH_STENCIL_FLAGS.CLEAR_DEPTH_FLAG, 1.0f, 0, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            {
                var trans = Vector3.UnitY * -0.3f;
                var rot = new Quaternion(clock.CurrentTimeMicro * Clock.MicroToSeconds % (2 * MathFloat.PI), 0f, 0f);

                var CubeModelTransform = rot.toRotationMatrix4x4(trans);

                var View = Matrix4x4.Translation(0f, 0f, 5f);

                // Get pretransform matrix that rotates the scene according the surface orientation
                var SrfPreTransform = CameraHelpers.GetSurfacePretransformMatrix(new Vector3(0, 0, 1), preTransform);

                // Get projection matrix adjusted to the current screen orientation
                var Proj = CameraHelpers.GetAdjustedProjectionMatrix(MathFloat.PI / 4.0f, 0.1f, 100f, window.WindowWidth, window.WindowHeight, preTransform);

                // Compute world-view-projection matrix
                var m_WorldViewProjMatrix = CubeModelTransform * View * SrfPreTransform * Proj;

                // Map the buffer and write current world-view-projection matrix
                IntPtr data = m_pImmediateContext.MapBuffer(m_VSConstants.Obj, MAP_TYPE.MAP_WRITE, MAP_FLAGS.MAP_FLAG_DISCARD);
                Matrix4x4* viewProjMat = (Matrix4x4*)data.ToPointer();
                //Mat is d3d, ogre style row major, need to transpose to send to diligent
                viewProjMat[0] = m_WorldViewProjMatrix.Transpose();
                m_pImmediateContext.UnmapBuffer(m_VSConstants.Obj, MAP_TYPE.MAP_WRITE);
            }

            // Bind vertex and index buffers
            UInt64[] offset = new UInt64[] { 0 };
            IBuffer[] pBuffs = new IBuffer[] { m_CubeVertexBuffer.Obj };
            m_pImmediateContext.SetVertexBuffers(0, 1, pBuffs, offset, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION, SET_VERTEX_BUFFERS_FLAGS.SET_VERTEX_BUFFERS_FLAG_RESET);
            m_pImmediateContext.SetIndexBuffer(m_CubeIndexBuffer.Obj, 0, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            // Set the pipeline state
            m_pImmediateContext.SetPipelineState(pipelineState.Obj);
            // Commit shader resources. RESOURCE_STATE_TRANSITION_MODE_TRANSITION mode
            // makes sure that resources are transitioned to required states.
            m_pImmediateContext.CommitShaderResources(m_pSRB.Obj, RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_TRANSITION);

            DrawIndexedAttribs DrawAttrs = new DrawIndexedAttribs();     // This is an indexed draw call
            DrawAttrs.IndexType = VALUE_TYPE.VT_UINT32; // Index type
            DrawAttrs.NumIndices = 36;
            // Verify the state of vertex and index buffers
            DrawAttrs.Flags = DRAW_FLAGS.DRAW_FLAG_VERIFY_ALL;
            m_pImmediateContext.DrawIndexed(DrawAttrs);

            this.swapChain.Present(1);
        }

        const String VSSource =
@"cbuffer Constants
{
    float4x4 g_WorldViewProj;
};

// Vertex shader takes two inputs: vertex position and color.
// By convention, Diligent Engine expects vertex shader inputs to be 
// labeled 'ATTRIBn', where n is the attribute number.
struct VSInput
{
    float3 Pos   : ATTRIB0;
    float4 Color : ATTRIB1;
};

struct PSInput 
{ 
    float4 Pos   : SV_POSITION; 
    float4 Color : COLOR0; 
};

// Note that if separate shader objects are not supported (this is only the case for old GLES3.0 devices), vertex
// shader output variable name must match exactly the name of the pixel shader input variable.
// If the variable has structure type (like in this example), the structure declarations must also be indentical.
void main(in  VSInput VSIn,
          out PSInput PSIn) 
{
    PSIn.Pos   = mul( float4(VSIn.Pos,1.0), g_WorldViewProj);
    PSIn.Color = VSIn.Color;
}";

        const String PSSource =
@"struct PSInput 
{ 
    float4 Pos   : SV_POSITION; 
    float4 Color : COLOR0; 
};

struct PSOutput
{ 
    float4 Color : SV_TARGET; 
};

// Note that if separate shader objects are not supported (this is only the case for old GLES3.0 devices), vertex
// shader output variable name must match exactly the name of the pixel shader input variable.
// If the variable has structure type (like in this example), the structure declarations must also be indentical.
void main(in  PSInput  PSIn,
          out PSOutput PSOut)
{
    PSOut.Color = PSIn.Color; 
}";
    }
}
