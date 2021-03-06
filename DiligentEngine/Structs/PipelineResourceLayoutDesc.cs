using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Uint8 = System.Byte;
using Int8 = System.SByte;
using Bool = System.Boolean;
using Uint32 = System.UInt32;
using Uint64 = System.UInt64;
using Float32 = System.Single;
using Uint16 = System.UInt16;
using PVoid = System.IntPtr;
using float4 = Engine.Vector4;
using float3 = Engine.Vector3;
using float2 = Engine.Vector2;
using float4x4 = Engine.Matrix4x4;
using BOOL = System.Boolean;

namespace DiligentEngine
{
    public partial class PipelineResourceLayoutDesc
    {

        public PipelineResourceLayoutDesc()
        {
            
        }
        public SHADER_RESOURCE_VARIABLE_TYPE DefaultVariableType { get; set; } = SHADER_RESOURCE_VARIABLE_TYPE.SHADER_RESOURCE_VARIABLE_TYPE_STATIC;
        public SHADER_TYPE DefaultVariableMergeStages { get; set; } = SHADER_TYPE.SHADER_TYPE_UNKNOWN;
        public List<ShaderResourceVariableDesc> Variables { get; set; }
        public List<ImmutableSamplerDesc> ImmutableSamplers { get; set; }


    }
}
