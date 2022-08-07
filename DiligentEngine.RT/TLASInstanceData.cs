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

namespace DiligentEngine.RT
{
    public partial class TLASInstanceData
    {
        internal static readonly RTInstances DefaultRTInstance = new RTInstances();
        internal const int DefaultRTInstanceIndex = 0;

        int instanceIndex = DefaultRTInstanceIndex;
        RTInstances rTInstances = DefaultRTInstance;
        private String instanceName;
        private IBottomLevelAS pblas;
        private InstanceMatrix transform;
        private Uint32 customId;
        private RAYTRACING_INSTANCE_FLAGS flags = RAYTRACING_INSTANCE_FLAGS.RAYTRACING_INSTANCE_NONE;
        private Uint8 mask = 0xFF;
        private Uint32 contributionToHitGroupIndex = ITopLevelAS.TLAS_INSTANCE_OFFSET_AUTO;

        public TLASInstanceData()
        {

        }

        public String InstanceName
        {
            get
            {
                return instanceName;
            }
            set
            {
                instanceName = value;
                ref var modInstance = ref rTInstances.passInstances[instanceIndex];
                modInstance.InstanceName = value;
            }
        }

        public IBottomLevelAS pBLAS
        {
            get
            {
                return pblas;
            }
            set
            {
                pblas = value;
                ref var modInstance = ref rTInstances.passInstances[instanceIndex];
                modInstance.pBLAS = value?.ObjPtr ?? IntPtr.Zero;
            }
        }

        public InstanceMatrix Transform
        {
            get
            {
                return transform;
            }
            set
            {
                transform = value;
                ref var modInstance = ref rTInstances.passInstances[instanceIndex];
                modInstance.Transform = value;
            }
        }

        public Uint32 CustomId
        {
            get
            {
                return customId;
            }
            set
            {
                customId = value;
                ref var modInstance = ref rTInstances.passInstances[instanceIndex];
                modInstance.CustomId = value;
            }
        }

        public RAYTRACING_INSTANCE_FLAGS Flags
        {
            get
            {
                return flags;
            }
            set
            {
                flags = value;
                ref var modInstance = ref rTInstances.passInstances[instanceIndex];
                modInstance.Flags = value;
            }
        }

        public Uint8 Mask
        {
            get
            {
                return mask;
            }
            set
            {
                mask = value;
                ref var modInstance = ref rTInstances.passInstances[instanceIndex];
                modInstance.Mask = value;
            }
        }

        public Uint32 ContributionToHitGroupIndex
        {
            get
            {
                return contributionToHitGroupIndex;
            }
            set
            {
                contributionToHitGroupIndex = value;
                ref var modInstance = ref rTInstances.passInstances[instanceIndex];
                modInstance.ContributionToHitGroupIndex = value;
            }
        }

        internal int InstanceIndex
        {
            get
            {
                return instanceIndex;
            }
            set
            {
                instanceIndex = value;
            }
        }

        internal RTInstances RTInstances
        {
            get
            {
                return rTInstances;
            }
            set
            {
                rTInstances = value;
            }
        }
    }
}
