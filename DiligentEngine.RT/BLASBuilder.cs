using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    public class BLASDesc
    {
        public String Name { get; private set; }

        public BLASDesc(String name)
        {
            this.Name = name;
        }

        public Vector3[] CubePos { get; set; }

        public Vector2[] CubeUV { get; set; }

        public Vector2[] GlobalCubeUV { get; set; }

        public Vector4[] CubeNormals { get; set; }

        public float[] Texture1 { get; set; }

        public float[] Texture2 { get; set; }

        public UInt32[] Indices { get; set; }

        public RAYTRACING_BUILD_AS_FLAGS BuildAsFlags { get; set; } = RAYTRACING_BUILD_AS_FLAGS.RAYTRACING_BUILD_AS_PREFER_FAST_TRACE;

        public RAYTRACING_GEOMETRY_FLAGS Flags { get; internal set; } = RAYTRACING_GEOMETRY_FLAGS.RAYTRACING_GEOMETRY_FLAG_OPAQUE;
    }

    public class BLASInstance : IDisposable
    {
        internal BLASInstance(BLASBuilder builder)
        {
            this.builder = builder;
        }

        public AutoPtr<IBottomLevelAS> BLAS { get; internal set; }
        internal AutoPtr<IBuffer> VertexBuffer { get; set; }
        internal AutoPtr<IBuffer> IndexBuffer { get; set; }

        public uint VertexOffset { get; internal set; }

        public uint IndexOffset { get; internal set; }

        internal CubeAttribVertex[] AttrVertices;

        internal uint[] Indices;

        private readonly BLASBuilder builder;

        public void Dispose()
        {
            builder.Remove(this);
            BLAS.Dispose();
            IndexBuffer.Dispose();
            VertexBuffer.Dispose();
        }
    }

    public class BLASBuilder : IDisposable
    {
        class BLASInstanceManager
        {
            private List<BLASInstance> instances = new List<BLASInstance>();
            private int vertexCount = 0;
            private int indexCount = 0;

            public int VertexCount => vertexCount;

            public int IndexCount => indexCount;

            public void Add(BLASInstance instance)
            {
                this.instances.Add(instance);
                vertexCount += instance.AttrVertices.Length;
                indexCount += instance.Indices.Length;
            }

            public void Remove(BLASInstance instance)
            {
                var index = instances.IndexOf(instance);
                if (index != -1)
                {
                    this.instances.RemoveAt(index);
                    vertexCount -= instance.AttrVertices.Length;
                    indexCount -= instance.Indices.Length;
                }
            }

            public CubeAttribVertex[] CreateAttrVerticesArray()
            {
                //TODO: For now just do a lame copy of the array, can improve this later once everything is working
                var array = new CubeAttribVertex[vertexCount];
                int offset = 0;
                foreach(var instance in instances)
                {
                    instance.VertexOffset = (uint)offset;
                    var length = instance.AttrVertices.Length;
                    var target = new Span<CubeAttribVertex>(array, offset, length);
                    var source = new Span<CubeAttribVertex>(instance.AttrVertices);
                    source.CopyTo(target);
                    offset += length;
                }
                return array;
            }

            public uint[] CreateAttrIndicesArray()
            {
                //TODO: For now just do a lame copy of the array, can improve this later once everything is working
                var array = new uint[indexCount];
                int offset = 0;
                foreach (var instance in instances)
                {
                    instance.IndexOffset = (uint)offset;
                    var length = instance.Indices.Length;
                    var target = new Span<uint>(array, offset, length);
                    var source = new Span<uint>(instance.Indices);
                    source.CopyTo(target);
                    offset += length;
                }
                return array;
            }
        }

        private readonly GraphicsEngine graphicsEngine;
        private readonly RayTracingRenderer renderer;
        private readonly BLASInstanceManager manager = new BLASInstanceManager();

        AutoPtr<IBuffer> attrBuffer;
        AutoPtr<IBuffer> indexBuffer;

        public IBuffer AttrBuffer => attrBuffer.Obj;

        public IBuffer IndexBuffer => indexBuffer.Obj;

        public BLASBuilder(GraphicsEngine graphicsEngine, RayTracingRenderer renderer)
        {
            this.graphicsEngine = graphicsEngine;
            this.renderer = renderer;
        }

        /// <summary>
        /// Create a blas mesh. The caller is responsible to dispose the result.
        /// </summary>
        /// <param name="blasMeshDesc">The description of the mesh to create.</param>
        /// <returns></returns>
        public async Task<BLASInstance> CreateBLAS(BLASDesc blasMeshDesc)
        {
            var m_pDevice = graphicsEngine.RenderDevice;

            var Attribs = new BuildBLASAttribs();
            var result = new BLASInstance(this);
            AutoPtr<IBuffer> pScratchBuffer = null;

            try
            {
                var barriers = new List<StateTransitionDesc>(10);

                await Task.Run(() =>
                {
                    var attrVertices = new CubeAttribVertex[blasMeshDesc.CubePos.Length];
                    var Indices = blasMeshDesc.Indices;

                    result.AttrVertices = attrVertices;
                    result.Indices = Indices;

                    for (var i = 0; i < blasMeshDesc.CubePos.Length; ++i)
                    {
                        var vertex = new CubeAttribVertex();
                        vertex.uv = blasMeshDesc.CubeUV[i];
                        vertex.globalUv = blasMeshDesc.GlobalCubeUV[i];
                        vertex.normal = blasMeshDesc.CubeNormals[i];
                        attrVertices[i] = vertex;
                    }

                    for (int i = 0; i < Indices.Length; i += 3)
                    {
                        var index1 = Indices[i];
                        var index2 = Indices[i + 1];
                        var index3 = Indices[i + 2];

                        var pos1 = blasMeshDesc.CubePos[index1];
                        var pos2 = blasMeshDesc.CubePos[index2];
                        var pos3 = blasMeshDesc.CubePos[index3];

                        var vertex1 = attrVertices[index1];
                        var vertex2 = attrVertices[index2];
                        var vertex3 = attrVertices[index3];

                        CalculateTangentBitangent(pos1, pos2, pos3, ref vertex1, ref vertex2, ref vertex3);

                        vertex1.position = pos1.ToVector4();
                        vertex2.position = pos2.ToVector4();
                        vertex3.position = pos3.ToVector4();

                        vertex1.tex = blasMeshDesc.Texture1[index1];
                        vertex2.tex = blasMeshDesc.Texture1[index2];
                        vertex3.tex = blasMeshDesc.Texture1[index3];

                        vertex1.tex2 = blasMeshDesc.Texture2[index1];
                        vertex2.tex2 = blasMeshDesc.Texture2[index2];
                        vertex3.tex2 = blasMeshDesc.Texture2[index3];

                        attrVertices[index1] = vertex1;
                        attrVertices[index2] = vertex2;
                        attrVertices[index3] = vertex3;
                    }

                    // Create vertex buffer
                    unsafe
                    {
                        var BuffDesc = new BufferDesc();
                        BuffDesc.Name = $"BLAS Vertices buffer";
                        BuffDesc.Usage = USAGE.USAGE_IMMUTABLE;
                        BuffDesc.BindFlags = BIND_FLAGS.BIND_RAY_TRACING;

                        BufferData BufData = new BufferData();
                        fixed (Vector3* vertices = blasMeshDesc.CubePos)
                        {
                            BufData.pData = new IntPtr(vertices);
                            BufData.DataSize = BuffDesc.Size = (uint)(sizeof(Vector3) * blasMeshDesc.CubePos.Length);
                            result.VertexBuffer = m_pDevice.CreateBuffer(BuffDesc, BufData);
                        }

                        barriers.Add(new StateTransitionDesc { pResource = result.VertexBuffer.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_BUILD_AS_READ, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }

                    // Create index buffer
                    unsafe
                    {
                        var BuffDesc = new BufferDesc();
                        BuffDesc.Name = $"BLAS Indices buffer";
                        BuffDesc.Usage = USAGE.USAGE_IMMUTABLE;
                        BuffDesc.BindFlags = BIND_FLAGS.BIND_RAY_TRACING;
                        BuffDesc.ElementByteStride = (uint)sizeof(uint);
                        BuffDesc.Mode = BUFFER_MODE.BUFFER_MODE_STRUCTURED;

                        BufferData BufData = new BufferData();
                        fixed (uint* p_indices = Indices)
                        {
                            BufData.pData = new IntPtr(p_indices);
                            BufData.DataSize = BuffDesc.Size = BuffDesc.ElementByteStride * (uint)Indices.Length;
                            result.IndexBuffer = m_pDevice.CreateBuffer(BuffDesc, BufData);
                        }

                        barriers.Add(new StateTransitionDesc { pResource = result.IndexBuffer.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_BUILD_AS_READ, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                    }

                    // Create & build bottom level acceleration structure
                    unsafe
                    {
                        // Create BLAS
                        var Triangles = new BLASTriangleDesc();
                        {
                            Triangles.GeometryName = blasMeshDesc.Name;
                            Triangles.MaxVertexCount = (uint)attrVertices.Length;
                            Triangles.VertexValueType = VALUE_TYPE.VT_FLOAT32;
                            Triangles.VertexComponentCount = 3;
                            Triangles.MaxPrimitiveCount = (uint)(Indices.Length / 3);
                            Triangles.IndexType = VALUE_TYPE.VT_UINT32;

                            var ASDesc = new BottomLevelASDesc();
                            ASDesc.Name = $"{blasMeshDesc.Name} BLAS";
                            ASDesc.Flags = blasMeshDesc.BuildAsFlags;
                            ASDesc.pTriangles = new List<BLASTriangleDesc> { Triangles };

                            result.BLAS = m_pDevice.CreateBLAS(ASDesc);

                            barriers.Add(new StateTransitionDesc { pResource = result.BLAS.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_BUILD_AS_WRITE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                        }

                        // Create scratch buffer
                        pScratchBuffer = m_pDevice.CreateBuffer(new BufferDesc()
                        {
                            Name = $"{blasMeshDesc.Name} BLAS Scratch Buffer",
                            Usage = USAGE.USAGE_DEFAULT,
                            BindFlags = BIND_FLAGS.BIND_RAY_TRACING,
                            Size = result.BLAS.Obj.ScratchBufferSizes_Build,
                        }, new BufferData());

                        barriers.Add(new StateTransitionDesc { pResource = pScratchBuffer.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_BUILD_AS_WRITE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });

                        // Build BLAS
                        var TriangleData = new BLASBuildTriangleData();
                        TriangleData.GeometryName = Triangles.GeometryName;
                        TriangleData.pVertexBuffer = result.VertexBuffer.Obj;
                        TriangleData.VertexStride = (uint)sizeof(Vector3);
                        TriangleData.VertexCount = Triangles.MaxVertexCount;
                        TriangleData.VertexValueType = Triangles.VertexValueType;
                        TriangleData.VertexComponentCount = Triangles.VertexComponentCount;
                        TriangleData.pIndexBuffer = result.IndexBuffer.Obj;
                        TriangleData.PrimitiveCount = Triangles.MaxPrimitiveCount;
                        TriangleData.IndexType = Triangles.IndexType;
                        TriangleData.Flags = blasMeshDesc.Flags;

                        Attribs.pBLAS = result.BLAS.Obj;
                        Attribs.pTriangleData = new List<BLASBuildTriangleData> { TriangleData };

                        // Scratch buffer will be used to store temporary data during BLAS build.
                        // Previous content in the scratch buffer will be discarded.
                        Attribs.pScratchBuffer = pScratchBuffer.Obj;

                        // Allow engine to change resource states.
                        Attribs.BLASTransitionMode = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_VERIFY;
                        Attribs.GeometryTransitionMode = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_VERIFY;
                        Attribs.ScratchBufferTransitionMode = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_VERIFY;
                    }
                });

                //TODO: For now this has no synchronization, so do it on the main thread, but this could be changed
                manager.Add(result);
                UpdateSharedBuffers(barriers);

                var m_pImmediateContext = graphicsEngine.ImmediateContext;
                m_pImmediateContext.TransitionResourceStates(barriers);
                m_pImmediateContext.BuildBLAS(Attribs);

                //Finish blas transition
                barriers.Clear();
                barriers.Add(new StateTransitionDesc { pResource = result.BLAS.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_BUILD_AS_READ, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                m_pImmediateContext.TransitionResourceStates(barriers);

                return result;
            }
            finally
            {
                pScratchBuffer?.Dispose();
            }
        }

        public void Dispose()
        {
            DestroyShaderBuffers();
        }

        public void CalculateTangentBitangent(
            in Vector3 pos1, in Vector3 pos2, in Vector3 pos3,
            ref CubeAttribVertex v1, ref CubeAttribVertex v2, ref CubeAttribVertex v3)
        {
            //This is adapted from
            //https://learnopengl.com/Advanced-Lighting/Normal-Mapping

            var edge1 = pos2 - pos1;
            var edge2 = pos3 - pos1;
            var deltaUV1 = v2.uv - v1.uv;
            var deltaUV2 = v3.uv - v1.uv;

            float f = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV2.x * deltaUV1.y);

            var tangent = new Vector4();
            tangent.x = f * (deltaUV2.y * edge1.x - deltaUV1.y * edge2.x);
            tangent.y = f * (deltaUV2.y * edge1.y - deltaUV1.y * edge2.y);
            tangent.z = f * (deltaUV2.y * edge1.z - deltaUV1.y * edge2.z);

            var bitangent = new Vector4();
            bitangent.x = f * (-deltaUV2.x * edge1.x + deltaUV1.x * edge2.x);
            bitangent.y = f * (-deltaUV2.x * edge1.y + deltaUV1.x * edge2.y);
            bitangent.z = f * (-deltaUV2.x * edge1.z + deltaUV1.x * edge2.z);

            v1.tangent = tangent;
            v2.tangent = tangent;
            v3.tangent = tangent;

            v1.binormal = bitangent;
            v2.binormal = bitangent;
            v3.binormal = bitangent;
        }

        private void DestroyShaderBuffers()
        {
            attrBuffer?.Dispose();
            indexBuffer?.Dispose();
            attrBuffer = null;
            indexBuffer = null;
        }

        private void UpdateSharedBuffers(List<StateTransitionDesc> barriers)
        {
            var m_pDevice = graphicsEngine.RenderDevice;

            DestroyShaderBuffers();

            // Create attribs vertex buffer
            unsafe
            {
                var attrVertices = manager.CreateAttrVerticesArray();
                if(attrVertices.Length == 0)
                {
                    return; //No vertices, bail
                }

                var BuffDesc = new BufferDesc();
                BuffDesc.Name = "Attrib vertices buffer";
                BuffDesc.Usage = USAGE.USAGE_IMMUTABLE;
                BuffDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
                BuffDesc.ElementByteStride = (uint)sizeof(CubeAttribVertex);
                BuffDesc.Mode = BUFFER_MODE.BUFFER_MODE_STRUCTURED;

                BufferData BufData = new BufferData();
                fixed (CubeAttribVertex* p_vertices = attrVertices)
                {
                    BufData.pData = new IntPtr(p_vertices);
                    BufData.DataSize = BuffDesc.Size = BuffDesc.ElementByteStride * (uint)attrVertices.Length;
                    attrBuffer = m_pDevice.CreateBuffer(BuffDesc, BufData)
                        ?? throw new InvalidOperationException("Cannot create attribute buffer");
                }

                barriers.Add(new StateTransitionDesc { pResource = attrBuffer.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
            }

            // Create index buffer
            unsafe
            {
                var Indices = manager.CreateAttrIndicesArray();
                if(Indices.Length == 0)
                {
                    return; //No Indices, bail
                }

                var BuffDesc = new BufferDesc();
                BuffDesc.Name = "Indices buffer";
                BuffDesc.Usage = USAGE.USAGE_IMMUTABLE;
                BuffDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
                BuffDesc.ElementByteStride = (uint)sizeof(uint);
                BuffDesc.Mode = BUFFER_MODE.BUFFER_MODE_STRUCTURED;

                BufferData BufData = new BufferData();
                fixed (uint* p_indices = Indices)
                {
                    BufData.pData = new IntPtr(p_indices);
                    BufData.DataSize = BuffDesc.Size = BuffDesc.ElementByteStride * (uint)Indices.Length;
                    indexBuffer = m_pDevice.CreateBuffer(BuffDesc, BufData)
                        ?? throw new InvalidOperationException("Cannot create index buffer");
                }

                barriers.Add(new StateTransitionDesc { pResource = indexBuffer.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
            }

            renderer.RequestRebind();
        }

        internal void Remove(BLASInstance blas)
        {
            manager.Remove(blas);
            var barriers = new List<StateTransitionDesc>();
            UpdateSharedBuffers(barriers);
            graphicsEngine.ImmediateContext.TransitionResourceStates(barriers);
        }
    }
}
