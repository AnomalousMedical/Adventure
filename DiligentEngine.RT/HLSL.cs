using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using System.Runtime.InteropServices;

namespace DiligentEngine.RT.HLSL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PrimaryRayPayload
    {
        float3 Color;
        float Depth;
        uint Recursion;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct EmissiveRayPayload
    {
        float3 Color;
        uint Recursion;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ShadowRayPayload
    {
        float Shading;   // 0 - fully shadowed, 1 - fully in light, 0..1 - for semi-transparent objects
        uint Recursion; // Current recusrsion depth
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct BlasInstanceData
    {
        public float u1;
        public float v1;
        public float u2;
        public float v2;
        public float u3;
        public float v3;
        public float u4;
        public float v4;

        public int baseTexture;
        public int normalTexture;
        public int physicalTexture;
        public int emissiveTexture;
        public uint indexOffset;
        public uint vertexOffset;
        public uint dataType;
        public uint lightingType;
    };

    public static class BlasInstanceDataConstants
    {
        public const uint MeshData = 0;
        public const uint SpriteData = 1;

        public const uint LightAndShadeBase = 0;
        public const uint LightAndShadeBaseEmissive = 1;
        public const uint LightAndShadeBaseNormal = 2;
        public const uint LightAndShadeBaseNormalEmissive = 3;
        public const uint LightAndShadeBaseNormalPhysical = 4;
        public const uint LightAndShadeBaseNormalPhysicalEmissive = 5;
        public const uint LightAndShadeBaseNormalPhysicalReflective = 6;
        public const uint LightAndShadeBaseNormalPhysicalReflectiveEmissive = 7;

        public static uint GetShaderForDescription(bool hasNormal, bool hasPhysical, bool reflective, bool emissive)
        {
            if (hasNormal && hasPhysical && reflective)
            {
                if (emissive)
                {
                    return LightAndShadeBaseNormalPhysicalReflectiveEmissive;
                }
                return LightAndShadeBaseNormalPhysicalReflective;
            }

            if(hasNormal && hasPhysical)
            {
                if (emissive)
                {
                    return LightAndShadeBaseNormalPhysicalEmissive;
                }
                return LightAndShadeBaseNormalPhysical;
            }

            if (hasNormal)
            {
                if (emissive)
                {
                    return LightAndShadeBaseNormalEmissive;
                }
                return LightAndShadeBaseNormal;
            }

            if (emissive)
            {
                return LightAndShadeBaseEmissive;
            }
            return LightAndShadeBase;
        }
    }
}
