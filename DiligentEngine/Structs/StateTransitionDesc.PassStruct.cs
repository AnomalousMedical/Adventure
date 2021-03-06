using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct StateTransitionDescPassStruct
    {
        public IntPtr pResourceBefore;
        public IntPtr pResource;
        public Uint32 FirstMipLevel;
        public Uint32 MipLevelsCount;
        public Uint32 FirstArraySlice;
        public Uint32 ArraySliceCount;
        public RESOURCE_STATE OldState;
        public RESOURCE_STATE NewState;
        public STATE_TRANSITION_TYPE TransitionType;
        public STATE_TRANSITION_FLAGS Flags;
        public static StateTransitionDescPassStruct[] ToStruct(IEnumerable<StateTransitionDesc> vals)
        {
            if(vals == null)
            {
                return null;
            }

            return vals.Select(i => new StateTransitionDescPassStruct
            {
                pResourceBefore = i.pResourceBefore == null ? IntPtr.Zero : i.pResourceBefore.objPtr,
                pResource = i.pResource == null ? IntPtr.Zero : i.pResource.objPtr,
                FirstMipLevel = i.FirstMipLevel,
                MipLevelsCount = i.MipLevelsCount,
                FirstArraySlice = i.FirstArraySlice,
                ArraySliceCount = i.ArraySliceCount,
                OldState = i.OldState,
                NewState = i.NewState,
                TransitionType = i.TransitionType,
                Flags = i.Flags,
            }).ToArray();
        }
    }
}
