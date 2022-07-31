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
    /// <summary>
    /// This class allows the use of the optimized version of creating a tlas.
    /// This means that is can use the TLASBuildInstanceDataPassStruct directly
    /// instead of relying on copies of the instance data being created. This
    /// is optimized for use with the RTRenderer class in DiligentEngine.RT.
    /// When using it you must supply the pInstances array and NumInstances.
    /// </summary>
    public partial class BuildTLASAttribsOptimized
    {
        public BuildTLASAttribsOptimized()
        {

        }
        public ITopLevelAS pTLAS { get; set; }
        public RESOURCE_STATE_TRANSITION_MODE TLASTransitionMode { get; set; } = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_NONE;
        public RESOURCE_STATE_TRANSITION_MODE BLASTransitionMode { get; set; } = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_NONE;
        public TLASBuildInstanceDataPassStruct[] pInstances { get; set; }
        public IBuffer pInstanceBuffer { get; set; }
        public Uint64 InstanceBufferOffset { get; set; } = 0;
        public RESOURCE_STATE_TRANSITION_MODE InstanceBufferTransitionMode { get; set; } = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_NONE;
        public Uint32 HitGroupStride { get; set; } = 1;
        public Uint32 BaseContributionToHitGroupIndex { get; set; } = 0;
        public HIT_GROUP_BINDING_MODE BindingMode { get; set; } = HIT_GROUP_BINDING_MODE.HIT_GROUP_BINDING_MODE_PER_GEOMETRY;
        public IBuffer pScratchBuffer { get; set; }
        public Uint64 ScratchBufferOffset { get; set; } = 0;
        public RESOURCE_STATE_TRANSITION_MODE ScratchBufferTransitionMode { get; set; } = RESOURCE_STATE_TRANSITION_MODE.RESOURCE_STATE_TRANSITION_MODE_NONE;
        public Bool Update { get; set; } = false;

        /// <summary>
        /// This must be set when using this version. It is optimized for the RT renderer.
        /// </summary>
        public uint NumInstances { get; set; }
    }
}
